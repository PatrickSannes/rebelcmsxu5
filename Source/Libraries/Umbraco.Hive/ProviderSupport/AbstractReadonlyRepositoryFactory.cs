using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    /// <summary>
    /// Represents a Hive provider's factory for sessions that have read operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractReadonlyRepositoryFactory<T> 
        : DisposableObject, IProviderReadonlyRepositoryFactory<T>
        where T : AbstractProviderRepository
    {
        protected AbstractReadonlyRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper)
        {
            ProviderMetadata = providerMetadata;
            FrameworkContext = frameworkContext;
            DependencyHelper = dependencyHelper;
        }

        /// <summary>
        /// Gets or sets the dependency helper for this provider.
        /// </summary>
        /// <value>The dependency helper.</value>
        protected ProviderDependencyHelper DependencyHelper { get; set; }

        /// <summary>
        /// Gets or sets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        public IFrameworkContext FrameworkContext { get; protected set; }

        /// <summary>
        /// Gets or sets the provider metadata.
        /// </summary>
        /// <value>The provider metadata.</value>
        public ProviderMetadata ProviderMetadata { get; protected set; }

        /// <summary>
        /// Gets an <see cref="AbstractProviderRepository"/> of type <see cref="T"/>. It will have only read operations.
        /// </summary>
        /// <returns></returns>
        public abstract T GetReadonlyRepository();
    }
}