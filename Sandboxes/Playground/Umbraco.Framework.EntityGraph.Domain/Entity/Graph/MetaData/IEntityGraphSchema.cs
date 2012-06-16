namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData
{
    /// <summary>
    /// Provides a mechanism for placing restrictions on the permitted types of entity which may be associated with one another
    /// in the entity graph.
    /// </summary>
    public interface IEntityGraphSchema : IReferenceByAlias
    {
        /// <summary>
        /// Gets a list of the permitted list of <see cref="IEntityTypeDefinition"/> allowed as descendents of a given <see cref="IEntityTypeDefinition"/>.
        /// </summary>
        /// <value>The permitted descendents.</value>
        IEntityCollection<IEntityTypeDefinition> PermittedDescendentTypes { get; }

        /// <summary>
        /// Gets the permitted association list of <see cref="IEntityTypeDefinition"/> allowed as associations of a given <see cref="IEntityTypeDefinition"/>.
        /// </summary>
        /// <value>The permitted association types.</value>
        IEntityCollection<IEntityTypeDefinition> PermittedAssociationTypes { get; }
    }
}