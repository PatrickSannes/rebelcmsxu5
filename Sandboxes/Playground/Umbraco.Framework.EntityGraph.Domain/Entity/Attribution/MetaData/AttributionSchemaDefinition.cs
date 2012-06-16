using System;
using System.Collections.Generic;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;
using Umbraco.Framework.EntityGraph.Domain.Versioning;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData
{
    public class AttributionSchemaDefinition : IAttributionSchemaDefinition
    {
        public AttributionSchemaDefinition()
        {
            _attributeGroupDefinitions = new EntityCollection<IAttributeGroupDefinition>();
            _attributeTypeDefinitions = new EntityCollection<IAttributeTypeDefinition>();
        }

        #region Implementation of ITracksConcurrency

        private IEntityCollection<IAttributeTypeDefinition> _attributeTypeDefinitions;

        private IEntityCollection<IAttributeGroupDefinition> _attributeGroupDefinitions;

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

        #region Implementation of IReferenceByAlias

        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="IReferenceByAlias.Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the object. The name is a string intended to be human-readable, and
        /// is often a more descriptive version of the <see cref="IReferenceByAlias.Alias"/> property.
        /// </summary>
        /// <value>The name.</value>
        public LocalizedString Name { get; set; }

        #endregion

        #region Implementation of IAttributionSchemaDefinition

        /// <summary>
        ///   Gets or sets the attribute type definitions for this schema.
        /// </summary>
        /// <value>The attribute type definitions.</value>
        public IEnumerable<IAttributeTypeDefinition> AttributeTypeDefinitions { get; set; }

        /// <summary>
        /// Gets the attribute group definitions.
        /// </summary>
        /// <value>The attribute group definitions.</value>
        public IEntityCollection<IAttributeGroupDefinition> AttributeGroupDefinitions
        {
            get { return _attributeGroupDefinitions; }
        }

        #endregion

        #region Implementation of IAttributionSchemaDefinition

        /// <summary>
        ///   Gets or sets the attribute type definitions for this schema.
        /// </summary>
        /// <value>The attribute type definitions.</value>
        IEntityCollection<IAttributeTypeDefinition> IAttributionSchemaDefinition.AttributeTypeDefinitions
        {
            get { return _attributeTypeDefinitions; }
        }

        #endregion
    }
}