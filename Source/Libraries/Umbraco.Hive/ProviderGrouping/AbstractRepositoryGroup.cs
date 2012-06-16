using System;
using System.Collections.Generic;
using Umbraco.Framework;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Hive.ProviderGrouping
{
    using System.Linq;
    using Umbraco.Framework.Caching;
    using Umbraco.Framework.Context;

    public abstract class AbstractRepositoryGroup : DisposableObject, IRequiresHiveContext
    {
        protected AbstractRepositoryGroup(IEnumerable<AbstractProviderRepository> childRepositories, Uri idRoot, AbstractScopedCache scopedCache, RepositoryContext hiveContext)
            : this(new GroupedProviderMetadata(childRepositories), idRoot, scopedCache, hiveContext)
        { }

        protected AbstractRepositoryGroup(GroupedProviderMetadata providers, Uri idRoot, AbstractScopedCache scopedCache, RepositoryContext hiveContext)
        {
            Providers = providers;
            IdRoot = idRoot;
            UnitScopedCache = scopedCache;
            HiveContext = hiveContext;
        }

        /// <summary>
        /// Gets or sets the repository context.
        /// </summary>
        /// <value>The repository context.</value>
        public RepositoryContext HiveContext { get; set; }

        /// <summary>
        /// Gets a unique Id for this session
        /// </summary>
        public readonly Guid SessionId = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the providers.
        /// </summary>
        /// <value>The providers.</value>
        public GroupedProviderMetadata Providers { get; protected set; }

        /// <summary>
        /// Gets or sets the id root for this provider group.
        /// </summary>
        /// <value>The id root.</value>
        protected Uri IdRoot { get; set; }

        /// <summary>
        /// Gets or sets the unit-scoped cache.
        /// </summary>
        /// <value>The unit-scoped cache.</value>
        public AbstractScopedCache UnitScopedCache { get; protected set; }
    }
}