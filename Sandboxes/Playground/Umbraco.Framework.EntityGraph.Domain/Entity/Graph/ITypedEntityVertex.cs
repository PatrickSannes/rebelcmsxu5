namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    /// An entity including references to its ascendants, descendants and associations
    /// </summary>
    public interface ITypedEntityVertex : ITypedEntity, IVertex, IEntityVertex
    {
        /// <summary>
        ///   All the available descendent entities in the graph.
        /// </summary>
        /// <value>The descendent entities.</value>
        ITypedEntityGraph DescendentTypedEntities { get; }

        /// <summary>
        ///   The parent entity excluding its graph.
        /// </summary>
        /// <value>The parent entity.</value>
        ITypedEntity ParentTypedEntity { get; }

        /// <summary>
        ///   The root entity.
        /// </summary>
        /// <value>The root.</value>
        ITypedEntityVertex RootTypedEntity { get; }
    }
}