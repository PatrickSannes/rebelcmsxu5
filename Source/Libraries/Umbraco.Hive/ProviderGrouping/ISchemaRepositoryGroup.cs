using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public interface ISchemaRepositoryGroup<out TFilter> : ICoreRepository<AbstractSchemaPart>
        where TFilter : class, IProviderTypeFilter
    {
        /// <summary>
        /// Used to access providers that can get or set revisions for <see cref="AbstractSchemaPart"/> types.
        /// </summary>
        /// <value>The revisions.</value>
        IRevisionRepositoryGroup<TFilter, EntitySchema> Revisions { get; }
    }
}