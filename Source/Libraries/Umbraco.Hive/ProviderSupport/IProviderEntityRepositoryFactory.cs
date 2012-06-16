using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Hive.ProviderSupport
{
    public interface IProviderEntityRepositoryFactory 
        : IProviderReadonlyEntityRepositoryFactory
    {
        /// <summary>
        /// Gets the schema session factory.
        /// </summary>
        /// <value>The schema session factory.</value>
        new AbstractSchemaRepositoryFactory SchemaRepositoryFactory { get; }

        /// <summary>
        /// Gets the revision session factory.
        /// </summary>
        /// <value>The revision session factory.</value>
        new AbstractRevisionRepositoryFactory<TypedEntity> RevisionRepositoryFactory { get; }

        /// <summary>
        /// Gets the session from the factory.
        /// </summary>
        /// <returns></returns>
        AbstractEntityRepository GetRepository();
    }
}