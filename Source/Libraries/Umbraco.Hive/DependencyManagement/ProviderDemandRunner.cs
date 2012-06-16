using System;
using System.Linq;
using System.Reflection;
using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Hive.DependencyManagement
{
    using Umbraco.Hive.Configuration;

    internal class ProviderDemandRunner
    {
        internal static void Run(IContainerBuilder containerBuilder, string providerKey, Type type, TypeLoaderElement element)
        {
            LogHelper.TraceIfEnabled<ProviderDemandRunner>("Calling setup module for {0} in {1}", () => providerKey, () => type.Assembly.GetName().Name);

            // Check to see if the type has ProviderDemandsDependenciesAttribute, and if so, invoke the builder
            var attribs = type.GetCustomAttributes(typeof(DemandsDependenciesAttribute), true).OfType<DemandsDependenciesAttribute>();
            var demandBuilders = attribs.Select(attrib => Activator.CreateInstance(attrib.DemandBuilderType)).OfType<AbstractProviderDependencyBuilder>().ToList();
            if (!demandBuilders.Any())
            {
                // The provider does not implement a demand builder, in which case we need to at least scaffold a noop bootstrapper so the installer can cope
                // and also a noop dependency helper
                LogHelper.TraceIfEnabled<ProviderDemandRunner>("No demand builders found of type AbstractProviderDependencyBuilder for {0} in {1}", () => providerKey, () => type.Assembly.GetName().Name);
                RegisterNoopBootstrapper(containerBuilder, providerKey);
                RegisterNoopDependencyHelper(containerBuilder, providerKey);
            }
            else
                foreach (var demandBuilder in demandBuilders)
                {
                    demandBuilder.ProviderKey = providerKey;

                    demandBuilder.RegistryConfigElement = element;

                    // First, run Initialise to give the demand builder a chance to set things up
                    demandBuilder.Initialise(containerBuilder.Context);

                    // If the builder can build, run the rest
                    if (demandBuilder.CanBuild)
                    {
                        containerBuilder.AddDependencyDemandBuilder(demandBuilder);
                    }

                    // Now get the factory for the provider's bootstrapper and register that
                    var factory = demandBuilder.GetProviderBootstrapperFactory(containerBuilder.Context);
                    if (factory == null)
                        RegisterNoopBootstrapper(containerBuilder, providerKey);
                    else
                    {
                        LogHelper.TraceIfEnabled<ProviderDemandRunner>("Registering a bootstrapper for {0}", () => providerKey);
                        containerBuilder.ForFactory(factory)
                            .Named<AbstractProviderBootstrapper>(providerKey)
                            .ScopedAs.Singleton();
                    }

                    // Now get the factory for the provider's dependency helper and register that
                    var helperFactory = demandBuilder.GetProviderDependencyHelperFactory(containerBuilder.Context);
                    if (helperFactory == null)
                    {
                        RegisterNoopDependencyHelper(containerBuilder, providerKey);
                    }
                    else
                    {
                        LogHelper.TraceIfEnabled<ProviderDemandRunner>("Registering a ProviderDependencyHelper for {0}", () => providerKey);
                        containerBuilder.ForFactory(helperFactory)
                            .NamedForSelf(providerKey)
                            .ScopedAs.Singleton();
                    }
                }
        }

        private static void RegisterNoopBootstrapper(IContainerBuilder containerBuilder, string providerKey)
        {
            LogHelper.TraceIfEnabled<ProviderDemandRunner>("Registering a no-op bootstrapper for {0}", () => providerKey);
            containerBuilder.ForFactory(x => new NoopProviderBootstrapper())
                .Named<AbstractProviderBootstrapper>(providerKey)
                .ScopedAs.Singleton();
        }

        private static void RegisterNoopDependencyHelper(IContainerBuilder containerBuilder, string providerKey)
        {
            LogHelper.TraceIfEnabled<ProviderDemandRunner>("Registering a NullProviderDependencyHelper for {0}", () => providerKey);
            containerBuilder.ForFactory(x => new NullProviderDependencyHelper(x.Resolve<ProviderMetadata>(providerKey)))
                .Named<ProviderDependencyHelper>(providerKey)
                .ScopedAs.Singleton();
        }
    }
}