namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    /// A graph of an entity synonum including its ascendants, descendants and associations
    /// </summary>
    /// <typeparam name = "TEntitySynonym">The type of the entity synonym.</typeparam>
    public interface IEntityAdaptorVertex<TEntitySynonym> : ITypedEntity, ITypedEntityVertex where TEntitySynonym : ITypedEntity
    {
        /// <summary>
        ///   Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        IEntityAdaptorVertex<TEntitySynonym> ParentAdaptor { get; set; }

        /// <summary>
        ///   Gets the descendents.
        /// </summary>
        /// <value>The descendents.</value>
        IEntityAdaptorGraph<TEntitySynonym> DescendentAdaptors { get; }

        /// <summary>
        ///   Gets the root entity.
        /// </summary>
        /// <value>The root.</value>
        IEntityAdaptorVertex<TEntitySynonym> RootAdaptor { get; }
    }
}