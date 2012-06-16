namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    /// A graph of an entity synonum including its ascendants, descendants and associations
    /// </summary>
    /// <typeparam name = "TEntityAdaptor">The type of the entity synonym.</typeparam>
    public interface ITypedEntityVertexAdaptor<TEntityAdaptor> : ITypedEntityVertex where TEntityAdaptor : ITypedEntity
    {
        /// <summary>
        ///   Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        ITypedEntityVertexAdaptor<TEntityAdaptor> ParentTypedAdaptor { get; set; }

        /// <summary>
        ///   Gets the descendents.
        /// </summary>
        /// <value>The descendents.</value>
        ITypedEntityVertexAdaptor<TEntityAdaptor> DescendentTypedAdaptors { get; }

        /// <summary>
        ///   Gets the root entity.
        /// </summary>
        /// <value>The root.</value>
        ITypedEntityVertexAdaptor<TEntityAdaptor> RootTypedAdaptor { get; }
    }
}