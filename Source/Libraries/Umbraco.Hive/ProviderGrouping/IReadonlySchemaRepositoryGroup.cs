using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public interface IReadonlySchemaRepositoryGroup<out TFilter> : ICoreReadonlyRepository<AbstractSchemaPart>
        where TFilter : class, IProviderTypeFilter
    {
        IReadonlyRevisionRepositoryGroup<TFilter, EntitySchema> Revisions { get; }
    }
}