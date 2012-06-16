using System;
using System.Configuration;
using Umbraco.Framework.Configuration;
using Umbraco.Framework.Context;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.NHibernate.Config;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Persistence.RdbmsModel.Mapping;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.NHibernate.Dependencies
{
    public class NHibernateDemandBuilder : AbstractProviderDependencyBuilder
    {
        private ProviderConfigurationSection _localConfig = null;

        const string RegisterTraceMessage = "Registering {0} with alias {1}";

        /// <summary>
        /// Initialises the provider dependency builder. This method is run by <see cref="Umbraco.Hive.DependencyManagement.ProviderDemandRunner"/> prior to it calling <see cref="Build"/>.
        /// </summary>
        /// <param name="builderContext">The builder context.</param>
        public override void Initialise(IBuilderContext builderContext)
        {
            Mandate.ParameterNotNull(builderContext, "builderContext");

            var configMain = builderContext.ConfigurationResolver.GetConfigSection(HiveConfigSection.ConfigXmlKey) as HiveConfigSection;
            if (configMain == null)
                throw new ConfigurationErrorsException(
                    string.Format("Configuration section '{0}' not found when building persistence provider '{1}'",
                                  HiveConfigSection.ConfigXmlKey, ProviderKey));

            var config2rw = RegistryConfigElement ?? configMain.Providers.ReadWriters[ProviderKey] ?? configMain.Providers.Readers[ProviderKey];

            if (config2rw == null)
            {
                throw new ConfigurationErrorsException(
                    string.Format("No configuration found for persistence provider '{0}'", ProviderKey));
            }

            _localConfig = !string.IsNullOrEmpty(config2rw.ConfigSectionKey)
                              ? DeepConfigManager.Default.GetFirstWebSetting
                                    <ProviderConfigurationSection, ProviderConfigurationSection>(
                                        config2rw.ConfigSectionKey, x => x, "~/App_Data/Umbraco/HiveConfig")
                                ??
                                config2rw.GetLocalProviderConfig() as ProviderConfigurationSection
                              : null;

            if (ValidateProviderConfigSection<NHibernateDemandBuilder>(_localConfig, config2rw))
            {
                CanBuild = true;
            }          
        }

        /// <summary>Builds the dependency demands required by this implementation. 
        /// This method will only be executed if <see cref="AbstractProviderDependencyBuilder.CanBuild"/> is set to true. Providers have an opportunity to do this in <see cref="Initialise"/> dependent upon whatever checks are necessary.</summary>
        /// <param name="containerBuilder">The <see cref="Framework.DependencyManagement.IContainerBuilder"/> .</param>
        /// <param name="builderContext"></param>
        public override void Build(IContainerBuilder containerBuilder, IBuilderContext builderContext)
        {
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");
            Mandate.ParameterNotNull(builderContext, "builderContext");

            containerBuilder.AddDependencyDemandBuilder(new NHibernateConfigBuilder(ProviderKey, _localConfig));

            containerBuilder
                .ForFactory(x => new ManualMapperv2(new NhLookupHelper(x.Resolve<EntityRepositoryFactory>(ProviderKey)), x.Resolve<ProviderMetadata>(ProviderKey)))
                .KnownAs<AbstractMappingEngine>()
                .KnownAsSelf()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();

            containerBuilder.ForFactory(x => new NhFactoryHelper(x.Resolve<global::NHibernate.Cfg.Configuration>(ProviderKey), null, false, false, x.Resolve<IFrameworkContext>()))
                .NamedForSelf(ProviderKey)
                .ScopedAs.Singleton();
        }

        /// <summary>
        /// Gets the provider bootstrapper factory. The <see cref="AbstractProviderDependencyBuilder"/> will use this to register the <see cref="AbstractProviderBootstrapper"/> against the ProviderKey.
        /// This method will still be called even if the <see cref="Initialise"/> function set <see cref="CanBuild"/> to false, such that a provider can have a bootstrapper that may be used to install it.
        /// </summary>
        /// <param name="builderContext">The builder context.</param>
        /// <returns></returns>
        public override Func<IResolutionContext, AbstractProviderBootstrapper> GetProviderBootstrapperFactory(IBuilderContext builderContext)
        {
            if (_localConfig == null)
            {
                // Returning a bootstrapper without configuration will tell the installer that this provider needs configuring
                return x => new ProviderBootstrapper(null, null);
            }
            return x => new ProviderBootstrapper(x.Resolve<global::NHibernate.Cfg.Configuration>(ProviderKey), _localConfig);
        }

        public override Func<IResolutionContext, ProviderDependencyHelper> GetProviderDependencyHelperFactory(IBuilderContext builderContext)
        {
            if (_localConfig == null)
            {
                return null;
            }
            return x => new DependencyHelper(x.Resolve<NhFactoryHelper>(ProviderKey), x.Resolve<ProviderMetadata>(ProviderKey));
        }
    }
}
