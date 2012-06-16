using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public interface IRevisionRepositoryGroup<out TProviderFilter, in T> : ICoreRevisionRepository<T>
        where TProviderFilter : class, IProviderTypeFilter
        where T : class, IVersionableEntity
    {
    }
}