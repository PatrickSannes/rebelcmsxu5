using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public interface IReadonlyRevisionRepositoryGroup<out TFilter, in T> : ICoreReadonlyRevisionRepository<T>
        where T : class, IVersionableEntity
        where TFilter : class, IProviderTypeFilter
    {
        
    }
}