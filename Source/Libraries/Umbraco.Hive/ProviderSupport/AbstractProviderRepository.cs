using System;
using System.Collections.Generic;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    using Umbraco.Framework.Caching;

    public interface IRequiresHiveContext
    {
        /// <summary>
        /// Gets or sets the repository context.
        /// </summary>
        /// <value>The repository context.</value>
        RepositoryContext HiveContext { get; }
    }

    public abstract class AbstractProviderRepository : DisposableObject, IRequiresHiveContext
    {
        protected AbstractProviderRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext)
        {
            CanRead = true;
            ProviderMetadata = providerMetadata;
            FrameworkContext = frameworkContext;
            RepositoryScopedCache = new DictionaryScopedCache();

            //HiveContext = new RepositoryContext(RuntimeCacheProvider.Default, PerHttpRequestCacheProvider.Default, frameworkContext);
        }

        /// <summary>
        /// Gets or sets the repository context.
        /// </summary>
        /// <value>The repository context.</value>
        public RepositoryContext HiveContext { get; internal protected set; }

        /// <summary>
        /// Gets a unique Id for this session
        /// </summary>
        public readonly Guid SessionId = Guid.NewGuid();

        /// <summary>
        /// Gets or sets a value indicating whether this instance can read.
        /// </summary>
        /// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
        public bool CanRead { get; protected set; }

        /// <summary>
        /// Gets or sets the unit-scoped cache.
        /// </summary>
        /// <value>The unit-scoped cache.</value>
        public AbstractScopedCache RepositoryScopedCache { get; set; }

        /// <summary>
        /// Gets or sets the provider metadata.
        /// </summary>
        /// <value>The provider metadata.</value>
        public ProviderMetadata ProviderMetadata { get; protected set; }

        /// <summary>
        /// Gets or sets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        public IFrameworkContext FrameworkContext { get; protected set; }

        protected bool ContextCacheAvailable()
        {
            return (HiveContext != null && HiveContext.GenerationScopedCache != null);
        }
    }
}