using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    using Umbraco.Framework.Linq;

    public interface IEntityRepositoryGroup : IEntityRepositoryGroup<IProviderTypeFilter>
    {
        
    }

    public interface IEntityRepositoryGroup<out TFilter>
        : ICoreRepository<TypedEntity>, ICoreReadonlyRepository<TypedEntity>, IQueryable<TypedEntity>, IQueryContext<TypedEntity>, IRequiresFrameworkContext
        where TFilter : class, IProviderTypeFilter
    {
        /// <summary>
        /// Used to access providers that can get or set revisions for <see cref="TypedEntity"/> types.
        /// </summary>
        /// <value>The revisions.</value>
        IRevisionRepositoryGroup<TFilter, TypedEntity> Revisions { get; }

        /// <summary>
        /// Used to access providers that can get or set <see cref="AbstractSchemaPart"/> types.
        /// </summary>
        /// <value>The schemas.</value>
        ISchemaRepositoryGroup<TFilter> Schemas { get; }

        /// <summary>
        /// Gets the query context.
        /// </summary>
        /// <value>The query context.</value>
        IQueryContext<TypedEntity> QueryContext { get; }
    }
}