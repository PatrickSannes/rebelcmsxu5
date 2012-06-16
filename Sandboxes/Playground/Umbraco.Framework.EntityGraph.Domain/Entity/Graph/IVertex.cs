using System;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    /// Defines the members required to treat an object as a vertex in a directed object graph.
    /// In simple terms, allows an <see cref="IEntity"/> to be part of a hierarchy.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        ///   Gets a value indicating whether this instance is the root.
        /// </summary>
        /// <value><c>true</c> if this instance is root; otherwise, <c>false</c>.</value>
        bool IsRoot { get; set; }

        /// <summary>
        ///   Gets the depth at which this entity resides from the root.
        /// </summary>
        /// <value>The depth.</value>
        int Depth { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether this instance has descendents.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has descendents; otherwise, <c>false</c>.
        /// </value>
        Boolean HasDescendents { get; set; }

        /// <summary>
        ///   Gets depth of descendents below the current entity. This is useful for determining
        ///   how many levels of entities should be requested from the repository in a subsequent
        ///   request to load descendents from the entity resolver.
        /// </summary>
        /// <value>The descendents depth.</value>
        int DescendentsDepth { get; set; }

        /// <summary>
        ///   Gets or sets the path of this entity from the root.
        /// </summary>
        /// <value>The path.</value>
        IEntityPath Path { get; set; }

        /// <summary>
        ///   Gets or sets the parent id.
        /// </summary>
        /// <value>The parent id.</value>
        IMappedIdentifier ParentId { get; set; }

        /// <summary>
        ///   Gets the associated entities.
        /// </summary>
        /// <value>The associated entities.</value>
        EntityAssociationCollection AssociatedEntities { get; set; }

        /// <summary>
        /// The parent as a dynamic.
        /// </summary>
        /// <value>The parent dynamic.</value>
        dynamic ParentDynamic { get; set; }
    }
}