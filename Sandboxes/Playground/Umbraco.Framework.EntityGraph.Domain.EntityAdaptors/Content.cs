using System;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Versioning;

namespace Umbraco.Framework.EntityGraph.Domain.EntityAdaptors
{
    public class Content : IContentVertex
    {
        [Obsolete("This functionality should be placed on the IContentResolver not the DTO", true)]
        public static Content NewDocument(IEntity fromEntity)
        {
            //return AutoMapper.Mapper.Map<IEntity, ContentDto>(fromEntity);
            return null; // pending a refactor
        }

        #region Implementation of ITracksConcurrency

        /// <summary>
        ///   Gets or sets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        public IConcurrencyToken ConcurrencyToken { get; set; }

        #endregion

        #region Implementation of IEntity

        /// <summary>
        ///   Gets or sets the status of the entity.
        /// </summary>
        /// <value>The status.</value>
        public IEntityStatus Status { get; set; }

        /// <summary>
        ///   Gets or sets the creation date of the entity (UTC).
        /// </summary>
        /// <value>The UTC created.</value>
        public DateTime UtcCreated { get; set; }

        /// <summary>
        ///   Gets or sets the modification date of the entity (UTC).
        /// </summary>
        /// <value>The UTC modified.</value>
        public DateTime UtcModified { get; set; }

        /// <summary>
        ///   Gets or sets the timestamp when the status of this entity was last changed (in UTC).
        /// </summary>
        /// <value>The UTC status changed.</value>
        public DateTime UtcStatusChanged { get; set; }

        /// <summary>
        ///   Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public IMappedIdentifier Id { get; set; }

        /// <summary>
        ///   Gets or sets the revision data.
        /// </summary>
        /// <value>The revision.</value>
        public IRevisionData Revision { get; set; }

        #endregion

        #region Implementation of IVertex

        /// <summary>
        ///   Gets a value indicating whether this instance is the root.
        /// </summary>
        /// <value><c>true</c> if this instance is root; otherwise, <c>false</c>.</value>
        public bool IsRoot { get;  set; }

        /// <summary>
        ///   Gets the depth at which this entity resides from the root.
        /// </summary>
        /// <value>The depth.</value>
        public int Depth { get;  set; }

        /// <summary>
        ///   Gets or sets a value indicating whether this instance has descendents.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has descendents; otherwise, <c>false</c>.
        /// </value>
        public bool HasDescendents { get; set; }

        /// <summary>
        ///   Gets depth of descendents below the current entity. This is useful for determining
        ///   how many levels of entities should be requested from the repository in a subsequent
        ///   request to load descendents from the entity resolver.
        /// </summary>
        /// <value>The descendents depth.</value>
        public int DescendentsDepth { get;  set; }

        /// <summary>
        ///   Gets or sets the path of this entity from the root.
        /// </summary>
        /// <value>The path.</value>
        public IEntityPath Path { get; set; }

        /// <summary>
        ///   Gets or sets the parent id.
        /// </summary>
        /// <value>The parent id.</value>
        public IMappedIdentifier ParentId { get; set; }

        /// <summary>
        ///   Gets the associated entities.
        /// </summary>
        /// <value>The associated entities.</value>
        public EntityAssociationCollection AssociatedEntities { get;  set; }

        /// <summary>
        /// The parent as a dynamic.
        /// </summary>
        /// <value>The parent dynamic.</value>
        public dynamic ParentDynamic { get;  set; }

        #endregion

        #region Implementation of ITypedEntityVertexAdaptor<IContentVertex>

        /// <summary>
        ///   Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public ITypedEntityVertexAdaptor<IContentVertex> ParentTypedAdaptor { get; set; }

        /// <summary>
        ///   Gets the descendents.
        /// </summary>
        /// <value>The descendents.</value>
        public ITypedEntityVertexAdaptor<IContentVertex> DescendentTypedAdaptors { get; private set; }

        /// <summary>
        ///   Gets the root entity.
        /// </summary>
        /// <value>The root.</value>
        public ITypedEntityVertexAdaptor<IContentVertex> RootTypedAdaptor { get; private set; }

        #endregion

        #region Implementation of ITypedEntity

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public ITypedAttributeCollection Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute groups.
        /// </summary>
        /// <value>The attribute groups.</value>
        public IAttributeGroupCollection AttributeGroups { get; set; }

        /// <summary>
        ///   Gets the attribute schema.
        /// </summary>
        /// <value>The attribute schema.</value>
        public IAttributionSchemaDefinition AttributionSchema
        {
            get
            {
                return this.EntityType.AttributeSchema;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>The type of the entity.</value>
        public IEntityTypeDefinition EntityType { get; set; }

        #endregion

        #region Implementation of IEntityVertex

        /// <summary>
        ///   All the available descendent entities in the graph.
        /// </summary>
        /// <value>The descendent entities.</value>
        public IEntityGraph DescendentEntities { get; set; }

        /// <summary>
        ///   The parent entity excluding its graph.
        /// </summary>
        /// <value>The parent entity.</value>
        public IEntityVertex ParentEntity { get; set; }

        /// <summary>
        ///   The root entity.
        /// </summary>
        /// <value>The root.</value>
        public IEntityVertex RootEntity { get; set; }

        #endregion

        #region Implementation of ITypedEntityVertex

        /// <summary>
        ///   All the available descendent entities in the graph.
        /// </summary>
        /// <value>The descendent entities.</value>
        public ITypedEntityGraph DescendentTypedEntities { get; private set; }

        /// <summary>
        ///   The parent entity excluding its graph.
        /// </summary>
        /// <value>The parent entity.</value>
        public ITypedEntity ParentTypedEntity { get; private set; }

        /// <summary>
        ///   The root entity.
        /// </summary>
        /// <value>The root.</value>
        public ITypedEntityVertex RootTypedEntity { get; private set; }

        #endregion
    }
}