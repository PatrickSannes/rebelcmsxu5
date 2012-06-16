using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    ///   Represents an association between two entities
    /// </summary>
    public interface IEntityAssociation : IEntity
    {
        /// <summary>
        ///   Gets or sets the entity.
        /// </summary>
        /// <value>The entity.</value>
        IEntityVertex Entity { get; set; }

        /// <summary>
        ///   Gets or sets the associated entity.
        /// </summary>
        /// <value>The associated entity.</value>
        IEntityVertex AssociatedEntity { get; set; }

        /// <summary>
        ///   Gets or sets the type of the association.
        /// </summary>
        /// <value>The type of the association.</value>
        IEntityAssociationType AssociationType { get; set; }
    }
}