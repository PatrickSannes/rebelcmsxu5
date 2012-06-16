using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Hive.RepositoryTypes;
using System.Linq;

namespace Umbraco.Hive.ProviderGrouping
{
    using Umbraco.Framework.Linq;

    public interface IReadonlyEntityRepositoryGroup<out TFilter> : ICoreReadonlyRepository<TypedEntity>, IQueryable<TypedEntity>, IQueryContext<TypedEntity>, IRequiresFrameworkContext
        where TFilter : class, IProviderTypeFilter
    {
        IReadonlyRevisionRepositoryGroup<TFilter, TypedEntity> Revisions { get; }
        IReadonlySchemaRepositoryGroup<TFilter> Schemas { get; }
        IQueryContext<TypedEntity> QueryContext { get; }
    }
}