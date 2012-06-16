using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Hive
{
    using Umbraco.Framework.Linq.QueryModel;

    public class HiveEntityPostActionEventArgs : EventArgs
    {
        public HiveEntityPostActionEventArgs(IRelatableEntity entity, AbstractScopedCache scopedCache)
        {
            Entity = entity;
            ScopedCache = scopedCache;
        }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>The entity.</value>
        public IRelatableEntity Entity { get; protected set; }

        /// <summary>
        /// Gets or sets the scoped cache.
        /// </summary>
        /// <value>The scoped cache.</value>
        public AbstractScopedCache ScopedCache { get; protected set; }
    }

    public class HiveQueryResultEventArgs : EventArgs
    {
        public HiveQueryResultEventArgs(object result, QueryDescription query, AbstractScopedCache scopedCache)
            : this(Enumerable.Repeat(result, 1).ToArray(), query, scopedCache)
        {
        }

        public HiveQueryResultEventArgs(IEnumerable<object> results, QueryDescription query, AbstractScopedCache scopedCache)
        {
            Results = results;
            ScopedCache = scopedCache;
            QueryDescription = query;
        }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>The results.</value>
        public IEnumerable<object> Results { get; protected set; }

        /// <summary>
        /// Gets or sets the query description.
        /// </summary>
        /// <value>The query description.</value>
        public QueryDescription QueryDescription { get; protected set; }

        /// <summary>
        /// Gets or sets the scoped cache.
        /// </summary>
        /// <value>The scoped cache.</value>
        public AbstractScopedCache ScopedCache { get; protected set; }
    }
}