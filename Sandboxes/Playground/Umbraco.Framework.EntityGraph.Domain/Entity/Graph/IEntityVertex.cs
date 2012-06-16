namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    /// An entity including references to its ascendants, descendants and associations
    /// </summary>
    public interface IEntityVertex : IEntity, IVertex
    {
        /// <summary>
        ///   All the available descendent entities in the graph.
        /// </summary>
        /// <value>The descendent entities.</value>
        IEntityGraph DescendentEntities { get; set; }

        /// <summary>
        ///   The parent entity excluding its graph.
        /// </summary>
        /// <value>The parent entity.</value>
        IEntityVertex ParentEntity { get; set; }

        /// <summary>
        ///   The root entity.
        /// </summary>
        /// <value>The root.</value>
        IEntityVertex RootEntity { get; set; }
    }
}