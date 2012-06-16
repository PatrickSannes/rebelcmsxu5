using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Configuration;
using Umbraco.Framework.Context;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.DependencyManagement;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Hive.DemandBuilders
{
    public class LoadFromPersistenceConfig : IDependencyDemandBuilder
    {
        private readonly HashSet<Tuple<string, Assembly>> _providersAlreadyScanned = new HashSet<Tuple<string, Assembly>>();

        /// <summary>Builds the dependency demands required by this implementation. </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="context">The context for this building session containing configuration etc.</param>
        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            var config =  context.ConfigurationResolver.GetConfigSection(HiveConfigSection.ConfigXmlKey) as HiveConfigSection;

            if (config == null)
                throw new ConfigurationErrorsException(string.Format("Cannot find {0} section in application configuration.", HiveConfigSection.ConfigXmlKey));

            var readWriters = DeepConfigManager.Default.GetPluginSettings<HiveConfigSection, TypeLoaderElementCollection>(HiveConfigSection.ConfigXmlKey, x => x.Providers.ReadWriters)
                .SelectMany(x => x).Distinct().ToArray();

            var readers = DeepConfigManager.Default.GetPluginSettings<HiveConfigSection, TypeLoaderElementCollection>(HiveConfigSection.ConfigXmlKey, x => x.Providers.Readers)
                .SelectMany(x => x).Distinct().ToArray();

            if (readWriters.Length == 0 && readers.Length == 0)
                throw new ConfigurationErrorsException(string.Format("There appear to be no Hive providers listed in the readers and read-writers parts of the application configuration ({0}).", HiveConfigSection.ConfigXmlKey));

            RegisterRepositoryFactories<AbstractEntityRepositoryFactory, AbstractRevisionRepositoryFactory<TypedEntity>, AbstractSchemaRepositoryFactory, AbstractRevisionRepositoryFactory<EntitySchema>>(containerBuilder, readWriters);

            RegisterRepositoryFactories<AbstractReadonlyEntityRepositoryFactory, AbstractReadonlyRevisionRepositoryFactory<TypedEntity>, AbstractReadonlySchemaRepositoryFactory, AbstractReadonlyRevisionRepositoryFactory<EntitySchema>>(containerBuilder, readers);
        }

        private static void RegisterRepositoryFactories
            <TEntityRepositoryFactory,
            TEntityRevisionRepositoryFactory,
            TSchemaRepositoryFactory,
            TSchemaRevisionRepositoryFactory>
            (IContainerBuilder containerBuilder, IEnumerable<TypeLoaderElement> loaderElements)
        {
            foreach (var element in loaderElements)
            {
                Type entityRevisionRepositoryFactoryType = null, schemaRepositoryFactoryType = null, schemaRevisionRepositoryFactoryType = null;

                var providerKey = element.Key;

                // Get the EntityRepositoryFactory type in config
                Type entityRepositoryFactoryType = ConfigurationHelper.GetTypeFromTypeConfigName(element.Type);

                if (entityRepositoryFactoryType == null)
                    throw new InvalidOperationException("Could not get a type for the root of a provider; type in config was {0}"
                                                            .InvariantFormat(element.Type));

                // Offer to invoke the provider's setup module (if any exist) so long as we haven't already done so
                ProviderDemandRunner.Run(containerBuilder, providerKey, entityRepositoryFactoryType, element);

                // If it is specified, get the RevisionRepositoryFactory type for the entities
                if (element.Revisioning != null && !string.IsNullOrEmpty(element.Revisioning.Type))
                {
                    entityRevisionRepositoryFactoryType = ConfigurationHelper.GetTypeFromTypeConfigName(element.Revisioning.Type);
                    // TODO: Enable provider demands for anything other than entity providers (need to add compound key for providerkey + Repositoryfactory type to bootstrapper registration)
                    // ProviderDemandRunner.Run(containerBuilder, providerKey, entityRevisionRepositoryFactoryType);
                }

                // If it is specified, get the SchemaRepositoryFactory type
                if (element.Schema != null && !string.IsNullOrEmpty(element.Schema.Type))
                {
                    schemaRepositoryFactoryType = ConfigurationHelper.GetTypeFromTypeConfigName(element.Schema.Type);
                    // TODO: Enable provider demands for anything other than entity providers (need to add compound key for providerkey + Repositoryfactory type to bootstrapper registration)
                    // ProviderDemandRunner.Run(containerBuilder, providerKey, schemaRepositoryFactoryType);

                    // If it is specified, get the RevisionRepositoryFactory type for the schemas
                    if (element.Schema.Revisioning != null && !string.IsNullOrEmpty(element.Schema.Revisioning.Type))
                    {
                        schemaRevisionRepositoryFactoryType = ConfigurationHelper.GetTypeFromTypeConfigName(element.Schema.Revisioning.Type);
                        // TODO: Enable provider demands for anything other than entity providers (need to add compound key for providerkey + Repositoryfactory type to bootstrapper registration)
                        // ProviderDemandRunner.Run(containerBuilder, providerKey, schemaRevisionRepositoryFactoryType);
                    }
                }

                // Register the ProviderMetadata for this provider
                containerBuilder.ForFactory(x => new ProviderMetadata(providerKey, null, true, false))
                    .NamedForSelf(providerKey)
                    .ScopedAs.Singleton();

                // Register the provided types, and if they are null, register the NullXX version in their place
                containerBuilder.For(entityRepositoryFactoryType)
                    .Named(providerKey, entityRepositoryFactoryType) // Register against the concrete type too so providers can ask for it specifically
                    .Named<TEntityRepositoryFactory>(providerKey)
                    .WithResolvedParam(x => x.Resolve<ProviderMetadata>(providerKey))
                    .WithResolvedParam(x => x.Resolve<ProviderDependencyHelper>(providerKey))
                    .WithResolvedParam(x => x.Resolve<TEntityRevisionRepositoryFactory>(providerKey))
                    .WithResolvedParam(x => x.Resolve<TSchemaRepositoryFactory>(providerKey))
                    .WithResolvedParam(x => x.Resolve<TSchemaRevisionRepositoryFactory>(providerKey))
                    .ScopedAs.Singleton();

                
                    
                if (schemaRepositoryFactoryType == null)
                    schemaRepositoryFactoryType = typeof(NullProviderSchemaRepositoryFactory);

                if (schemaRevisionRepositoryFactoryType == null)
                    schemaRevisionRepositoryFactoryType = typeof(NullProviderRevisionRepositoryFactory<EntitySchema>);

                if (entityRevisionRepositoryFactoryType == null)
                {
                    entityRevisionRepositoryFactoryType = typeof(NullProviderRevisionRepositoryFactory<TypedEntity>);
                    containerBuilder.ForFactory(
                        x => new NullProviderRevisionRepositoryFactory<TypedEntity>(
                                 x.Resolve<ProviderMetadata>(providerKey),
                                 x.Resolve<IFrameworkContext>()))
                        .Named(providerKey, entityRevisionRepositoryFactoryType)
                        .Named<TEntityRevisionRepositoryFactory>(providerKey)
                        .OnActivated((ctx, factory) => factory.FallbackProviderFactory = ctx.Resolve<AbstractEntityRepositoryFactory>(providerKey))
                        .ScopedAs.Singleton();
                }
                else
                {
                    containerBuilder.For(entityRevisionRepositoryFactoryType)
                        .WithResolvedParam(x => x.Resolve<ProviderMetadata>(providerKey))
                        .WithResolvedParam(x => x.Resolve<ProviderDependencyHelper>(providerKey))
                        .Named(providerKey, entityRevisionRepositoryFactoryType) // Register against the concrete type too so providers can ask for it specifically
                        .Named<TEntityRevisionRepositoryFactory>(providerKey)
                        .ScopedAs.Singleton();
                }

                containerBuilder.For(schemaRevisionRepositoryFactoryType)
                    .WithResolvedParam(x => x.Resolve<ProviderMetadata>(providerKey))
                    .WithResolvedParam(x => x.Resolve<ProviderDependencyHelper>(providerKey))
                    .Named(providerKey, schemaRevisionRepositoryFactoryType) // Register against the concrete type too so providers can ask for it specifically
                    .Named<TSchemaRevisionRepositoryFactory>(providerKey)
                    .ScopedAs.Singleton();

                containerBuilder.For(schemaRepositoryFactoryType)
                    .Named<TSchemaRepositoryFactory>(providerKey)
                    .Named(providerKey, schemaRepositoryFactoryType) // Register against the concrete type too so providers can ask for it specifically
                    .WithResolvedParam(x => x.Resolve<ProviderMetadata>(providerKey))
                    .WithResolvedParam(x => x.Resolve<ProviderDependencyHelper>(providerKey))
                    .WithResolvedParam(x => x.Resolve<TSchemaRevisionRepositoryFactory>(providerKey))
                    .ScopedAs.Singleton();
            }
        }
    }
}
