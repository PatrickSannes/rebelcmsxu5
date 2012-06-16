

using System;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.DependencyManagement;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Hive
{
    /// <summary>
    /// An abstract type for creating Hive provider demand builders.
    /// Derived types must have a parameterless constructor to make instantiating simpler.
    /// </summary>
    public abstract class AbstractProviderDependencyBuilder : IDependencyDemandBuilder
    {
        /// <summary>Builds the dependency demands required by this implementation. </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="context">The context for this building session containing configuration etc.</param>
        public abstract void Build(IContainerBuilder containerBuilder, IBuilderContext context);

        /// <summary>
        /// This is the unique provider key which is unique to readers and writers. It is used for identifiying a reader or writer
        /// and is used in setting up the providers DemandBuilder
        /// </summary>
        public string ProviderKey { get; set; }

        /// <summary>
        /// Gets or sets the registering config element, if this builder has been invoked as a result of a configuration entry.
        /// </summary>
        /// <value>The registry config element.</value>
        public TypeLoaderElement RegistryConfigElement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can build dependencies. Used by <see cref="ProviderDemandRunner"/> to determine if the <see cref="Build(Umbraco.Framework.DependencyManagement.IContainerBuilder,Umbraco.Framework.DependencyManagement.IBuilderContext)"/> method on this demand builder should be run.
        /// </summary>
        /// <value><c>true</c> if this instance can build; otherwise, <c>false</c>.</value>
        public bool CanBuild { get; protected set; }


        /// <summary>
        /// Initialises the provider dependency builder. This method is run by <see cref="Umbraco.Hive.DependencyManagement.ProviderDemandRunner"/> prior to it calling <see cref="Build(Umbraco.Framework.DependencyManagement.IContainerBuilder,Umbraco.Framework.DependencyManagement.IBuilderContext)"/>.
        /// </summary>
        /// <param name="builderContext">The builder context.</param>
        public abstract void Initialise(IBuilderContext builderContext);

        /// <summary>
        /// Gets the provider bootstrapper factory. The <see cref="AbstractProviderDependencyBuilder"/> will use this to register the <see cref="AbstractProviderBootstrapper"/> against the ProviderKey.
        /// </summary>
        /// <param name="builderContext">The builder context.</param>
        /// <returns></returns>
        public abstract Func<IResolutionContext, AbstractProviderBootstrapper> GetProviderBootstrapperFactory(IBuilderContext builderContext);

        /// <summary>
        /// Gets the provider dependency helper factory. If a provider requires dependencies with a specific registration key, use this delegate to register a <see cref="ProviderDependencyHelper"/> with the appropriate
        /// keyed dependencies. Otherwise, if this method returns null, <see cref="ProviderDemandRunner"/> will register a <see cref="NullProviderDependencyHelper"/> in its place.
        /// </summary>
        /// <param name="builderContext">The builder context.</param>
        /// <returns></returns>
        public abstract Func<IResolutionContext, ProviderDependencyHelper> GetProviderDependencyHelperFactory(IBuilderContext builderContext);


        /// <summary>
        /// Validates the provider config section.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <param name="typeLoader">The type loader.</param>
        /// <returns>true if the validation succeeded or false if it doesn't and will log a warning</returns>
        protected bool ValidateProviderConfigSection<T>(AbstractProviderConfigurationSection configuration, TypeLoaderElement typeLoader)
        {
            // ElementInformation.Source appears to be one of few ways to determine if the config-section exists or was returned on-demand with default values
            if (configuration != null && !string.IsNullOrEmpty(configuration.ElementInformation.Source))
            {
                return true;
            }
            else
            {
                LogHelper.Warn<T>("Cannot register dependencies for provider {0} because ProviderConfigurationSection was specified with key '{1}' but no matching configSection was found. The provider may not be installed correctly.",
                    ProviderKey,
                    typeLoader.ConfigSectionKey);

                return false;
            }
        }
    }
}
