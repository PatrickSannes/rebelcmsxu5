using System;
using System.Configuration;
using System.Linq;
using Examine;
using Examine.Config;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Umbraco.Framework.Configuration;
using Umbraco.Framework.Context;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Examine.Config;
using Umbraco.Framework.Persistence.Examine.Hive;
using Umbraco.Framework.Persistence.Examine.Mapping;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine
{
    /// <summary>
    /// Adds all of the Examine Hive dependencies to the IoC Container
    /// </summary>
    public class ExamineDemandBuilder : AbstractProviderDependencyBuilder
    {
        private ProviderConfigurationSection _localConfig;
        private ProviderSettings _internalIndexer;
        private ProviderSettings _internalSearcher;
        private IndexSet _internalIndexSet;

        /// <summary>
        /// Initializes the provider and ensures that all configuration can be read
        /// </summary>
        /// <param name="builderContext"></param>
        public override void Initialise(IBuilderContext builderContext)
        {
            Mandate.ParameterNotNull(builderContext, "builderContext");

            var configMain = builderContext.ConfigurationResolver.GetConfigSection(HiveConfigSection.ConfigXmlKey) as HiveConfigSection;

            if (configMain == null)
                throw new ConfigurationErrorsException(
                    string.Format("Configuration section '{0}' not found when building packaging provider '{1}'",
                                  HiveConfigSection.ConfigXmlKey, ProviderKey));

            var config2Rw = RegistryConfigElement ?? configMain.Providers.ReadWriters[ProviderKey] ?? configMain.Providers.Readers[ProviderKey];

            if (config2Rw == null)
                throw new ConfigurationErrorsException(
                    string.Format("No configuration found for persistence provider '{0}'", ProviderKey));

            //get the Hive provider config section
            _localConfig = DeepConfigManager.Default.GetFirstPluginSection<ProviderConfigurationSection>(config2Rw.ConfigSectionKey);

            if (!ValidateProviderConfigSection<ExamineDemandBuilder>(_localConfig, config2Rw))
            {
                CanBuild = false;
                return;
            }

            var configMgr = DeepConfigManager.Default;

            //get the internal indexer provider
            _internalIndexer = configMgr.GetFirstPluginSetting<ExamineSettings, ProviderSettings>("examine/examine.settings",
                                                                                             x => x.IndexProviders.SingleOrDefault(indexer => indexer.Name == _localConfig.InternalIndexer));
            if (_internalIndexer == null)
            {
                LogHelper.Warn<ExamineDemandBuilder>("Could not load UmbracoInternalIndexer, the configuration section could not be read.");
                CanBuild = false;
                return;
            }
                
            //get the internal searcher provider
            _internalSearcher = configMgr.GetFirstPluginSetting<ExamineSettings, ProviderSettings>("examine/examine.settings",
                                                                                              x => x.SearchProviders.SingleOrDefault(indexer => indexer.Name == _localConfig.InternalSearcher));
            if (_internalSearcher == null)
            {
                LogHelper.Warn<ExamineDemandBuilder>("Could not load UmbracoInternalSearcher, the configuration section could not be read.");
                CanBuild = false;
                return;
            }                

            //get the internal index set to use for the searcher/indexer
            _internalIndexSet = configMgr.GetFirstPluginSetting<IndexSets, IndexSet>("examine/examine.indexes",
                                                                                x => x.SingleOrDefault(set => set.SetName == _localConfig.InternalIndexSet));
            if (_internalIndexSet == null)
            {
                LogHelper.Warn<ExamineDemandBuilder>("Could not load UmbracoInternalIndexSet, the configuration section could not be read.");
                CanBuild = false;
                return;
            }

            CanBuild = true;
        }

        /// <summary>
        /// Builds the dependency demands required by this implementation.
        /// </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="context">The context for this building session containing configuration etc.</param>
        public override void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");
            Mandate.ParameterNotNull(context, "context");
            
            //create a new index set collection for use in our provider construction
            var indexSets = new[] { _internalIndexSet };

            //register the internal indexer
            containerBuilder.ForFactory(x => new UmbracoExamineIndexer(indexSets))
                .Named<IIndexer>(ProviderKey)
                .OnActivated((resolutionContext, indexProvider) => indexProvider.Initialize(_internalIndexer.Name, _internalIndexer.Parameters))
                .ScopedAs.Singleton();
            
            //register the internal searcher
            containerBuilder.ForFactory(x => new LuceneSearcher(indexSets))
                .Named<ISearcher>(ProviderKey)
                .OnActivated((resolutionContext, indexSearcher) => indexSearcher.Initialize(_internalSearcher.Name, _internalSearcher.Parameters))
                .ScopedAs.Singleton();
            
            //register an Examine Manager with the configured searchers/indexers
            containerBuilder.ForFactory(x => new ExamineManager(
                                                 new[] { x.Resolve<ISearcher>(ProviderKey) },
                                                 new[] { x.Resolve<IIndexer>(ProviderKey) },
                                                 x.Resolve<ISearcher>(ProviderKey)
                                                 )
                )
                .Named<ExamineManager>(ProviderKey)
                .ScopedAs.Singleton();

            //register the helper
            containerBuilder
                .For<ExamineHelper>()
                .WithResolvedParam(x => x.Resolve<ExamineManager>(ProviderKey))
                .Named<ExamineHelper>(ProviderKey)
                .ScopedAs.Singleton();

            //register the type mapper
            containerBuilder
                .For<ExamineModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithResolvedParam(x => x.Resolve<ExamineHelper>(ProviderKey))
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();            
        }

        public override Func<IResolutionContext, AbstractProviderBootstrapper> GetProviderBootstrapperFactory(IBuilderContext builderContext)
        {
            if (!CanBuild)
            {
                // Returning a bootstrapper without configuration will tell the installer that this provider needs configuring
                return x => new ProviderBootstrapper(null, null, x.Resolve<IFrameworkContext>());
            }
            return x => new ProviderBootstrapper(_localConfig, x.Resolve<ExamineManager>(ProviderKey), x.Resolve<IFrameworkContext>());
        }

        public override Func<IResolutionContext, ProviderDependencyHelper> GetProviderDependencyHelperFactory(IBuilderContext builderContext)
        {
            return x => new DependencyHelper(x.Resolve<ExamineHelper>(ProviderKey), x.Resolve<ProviderMetadata>(ProviderKey));
        }
    }
}