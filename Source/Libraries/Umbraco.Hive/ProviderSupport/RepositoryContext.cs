using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Hive.ProviderSupport
{
    using Umbraco.Framework;
    using Umbraco.Framework.Caching;
    using Umbraco.Framework.Context;

    public class RepositoryContext : DisposableObject, IRequiresFrameworkContext 
    {
        public RepositoryContext(AbstractCacheProvider generationScopedCache, AbstractCacheProvider batchScopedCache, IFrameworkContext frameworkContext)
        {
            GenerationScopedCache = generationScopedCache;
            BatchScopedCache = batchScopedCache;
            FrameworkContext = frameworkContext;
        }

        /// <summary>
        /// Gets or sets the hive manager.
        /// </summary>
        /// <value>The hive manager.</value>
        public IHiveManager HiveManager { get; protected set; }

        /// <summary>
        /// Gets or sets the generation-scoped cache. A generation-scoped cache may last the life of an application, or may span two or more lifetimes of the application.
        /// </summary>
        /// <value>The generation scoped cache.</value>
        public AbstractCacheProvider GenerationScopedCache { get; protected set; }

        /// <summary>
        /// Gets or sets the batch-scoped cache. The scope of a batch is determined by the application, and may span two or more units of work. For example, typically
        /// it might encompass a whole http request.
        /// </summary>
        /// <value>The batch scoped cache.</value>
        public AbstractCacheProvider BatchScopedCache { get; protected set; }

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; protected set; }

        #endregion

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            GenerationScopedCache.IfNotNull(x => x.Dispose());
            BatchScopedCache.IfNotNull(x => x.Dispose());
        }

        #endregion
    }
}
