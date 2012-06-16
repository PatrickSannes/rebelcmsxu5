using UmbracoFramework.EntityGraph.Domain;
using UmbracoFramework.EntityGraph.Domain.Entity;
using UmbracoFramework.EntityGraph.Domain.Entity.Graph;

namespace UmbracoFramework.EntityGraph.DataPersistence
{
    public interface IEntityRepositoryReader 
        : ISupportsProviderInjection, 
        IRepositoryReader<IEntityCollection, IEntityGraph, IEntity, IEntityVertex>
    {
        ///// <summary>
        ///// Gets the specified IEntity.
        ///// </summary>
        ///// <param name="identifier">The identifier of the entity.</param>
        ///// <remarks>By default, the implementation should not return ascendents or descendents.</remarks>
        ///// <returns></returns>
        //IEntity Get(IMappedIdentifier identifier);

        ///// <summary>
        ///// Gets the specified IEntity objects.
        ///// </summary>
        ///// <param name="identifiers">The identifiers of the entities.</param>
        ///// <remarks>By default, the implementation should not return ascendents or descendents.</remarks>
        ///// <returns></returns>
        //IEnumerable<IEntity> Get(IEnumerable<IMappedIdentifier> identifiers);

        ///// <summary>
        ///// Gets all IEntity objects in the repository.
        ///// </summary>
        ///// <returns></returns>
        //IEnumerable<IEntity> GetAll();

        ///// <summary>
        ///// Gets the specified IEntity, including descdendents, to a given depth.
        ///// </summary>
        ///// <param name="identifier">The identifier of the IEntity.</param>
        ///// <param name="traversalDepth">The traversal depth.</param>
        ///// <returns></returns>
        //IEntity Get(IMappedIdentifier identifier, int traversalDepth);


        ///// <summary>
        ///// Gets the specified IEntity objects to a given depth.
        ///// </summary>
        ///// <param name="identifiers">The identifiers.</param>
        ///// <param name="traversalDepth">The traversal depth.</param>
        ///// <returns></returns>
        //IEnumerable<IEntity> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth);

        ///// <summary>
        ///// Gets Ientity objects from the root to a given traversal depth.
        ///// </summary>
        ///// <param name="traversalDepth">The traversal depth.</param>
        ///// <returns></returns>
        //IEnumerable<IEntity> GetAll(int traversalDepth);

        ///// <summary>
        ///// Gets the parent IEntity for the given IEntity.
        ///// </summary>
        ///// <param name="forEntity">For entity.</param>
        ///// <returns></returns>
        //IEntity GetParent(IEntity forEntity);

        ///// <summary>
        ///// Gets the parent IEntity for the given identifier.
        ///// </summary>
        ///// <param name="forEntityIdentifier">For entity identifier.</param>
        ///// <returns></returns>
        //IEntity GetParent(IMappedIdentifier forEntityIdentifier);

        ///// <summary>
        ///// Gets the descendents of the given IEntity.
        ///// </summary>
        ///// <param name="forEntity">The ascendent entity.</param>
        ///// <returns></returns>
        //IEnumerable<IEntity> GetDescendents(IEntity forEntity);

        ///// <summary>
        ///// Gets the descendents of the IEntity with the given identifier.
        ///// </summary>
        ///// <param name="forEntityIdentifier">The ascendent entity identifier.</param>
        ///// <returns></returns>
        //IEnumerable<IEntity> GetDescendents(IMappedIdentifier forEntityIdentifier);

        /////// <summary>
        /////// Runs a query against the repository using the supplied IQueryBroker.
        /////// </summary>
        /////// <param name="queryBroker">The query broker.</param>
        /////// <returns></returns>
        ////IEnumerable<IEntity> GetFiltered(IQueryBroker queryBroker);

        /////// <summary>
        /////// Returns a new query broker for this repository.
        /////// </summary>
        /////// <returns></returns>
        ////IQueryBroker CreateQueryBroker();
    }
}