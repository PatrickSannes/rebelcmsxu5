using System;
using System.Diagnostics.Contracts;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Versioning;

namespace Umbraco.Framework.EntityGraph.Domain.Entity
{
    public class TypedEntity : ITypedEntity
    {
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(AttributionSchema != null);
            Contract.Invariant(UtcCreated < DateTime.UtcNow.AddDays(1));
            Contract.Invariant(UtcModified >= UtcCreated);
        }

        #region Implementation of ITracksConcurrency

        /// <summary>
        /// Gets or sets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        public IConcurrencyToken ConcurrencyToken { get; set; }

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
        /// Gets the attribute schema.
        /// </summary>
        /// <value>The attribute schema.</value>
        public IAttributionSchemaDefinition AttributionSchema { get;  set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>The type of the entity.</value>
        public IEntityTypeDefinition EntityType { get; set; }

        /// <summary>
        /// Gets or sets the status of the entity.
        /// </summary>
        /// <value>The status.</value>
        public IEntityStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the entity (UTC).
        /// </summary>
        /// <value>The UTC created.</value>
        public DateTime UtcCreated { get; set; }

        /// <summary>
        /// Gets or sets the modification date of the entity (UTC).
        /// </summary>
        /// <value>The UTC modified.</value>
        public DateTime UtcModified { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the status of this entity was last changed (in UTC).
        /// </summary>
        /// <value>The UTC status changed.</value>
        public DateTime UtcStatusChanged { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public IMappedIdentifier Id { get; set; }

        /// <summary>
        /// Gets or sets the revision data.
        /// </summary>
        /// <value>The revision.</value>
        public IRevisionData Revision { get; set; }

        #endregion
    }
}