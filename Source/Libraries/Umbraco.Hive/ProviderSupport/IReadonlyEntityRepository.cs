using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Hive.ProviderSupport
{
    using Umbraco.Framework.Linq;

    public interface IReadonlyEntityRepository 
        : IReadonlyProviderRepository<TypedEntity>, IQueryableDataSource
    {
        /// <summary>
        /// Gets the revisions session.
        /// </summary>
        /// <value>The revisions.</value>
        AbstractReadonlyRevisionRepository<TypedEntity> Revisions { get; }

        /// <summary>
        /// Gets the schemas session.
        /// </summary>
        /// <value>The schemas.</value>
        AbstractReadonlySchemaRepository Schemas { get; }
    }
}