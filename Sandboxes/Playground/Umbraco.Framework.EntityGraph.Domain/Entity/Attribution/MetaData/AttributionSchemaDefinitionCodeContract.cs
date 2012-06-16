using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Versioning;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData
{
    [ContractClassFor(typeof (IAttributionSchemaDefinition))]
    internal abstract class AttributionSchemaDefinitionCodeContract :
        IAttributionSchemaDefinition
    {
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrWhiteSpace(Alias));
        }

        #region Implementation of IReferenceByAlias

        public string Alias { get; set; }
        public LocalizedString Name { get; set; }

        #endregion

        #region Implementation of IAttributionSchemaDefinition

        public IEnumerable<IAttributeTypeDefinition> AttributeTypeDefinitions { get; set; }

        /// <summary>
        /// Gets the attribute group definitions.
        /// </summary>
        /// <value>The attribute group definitions.</value>
        public IEntityCollection<IAttributeGroupDefinition> AttributeGroupDefinitions { get; private set; }

        #endregion

        #region Implementation of ITracksConcurrency

        public IConcurrencyToken ConcurrencyToken { get; set; }

        #endregion

        #region Implementation of IEntity

        public IAttributionSchemaDefinition AttributionSchema
        {
            get { throw new NotImplementedException(); }
        }

        public IEntityStatus Status
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public DateTime UtcCreated
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public DateTime UtcModified
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public DateTime UtcStatusChanged
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IMappedIdentifier Id
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IRevisionData Revision
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region Implementation of IAttributionSchemaDefinition

        /// <summary>
        ///   Gets or sets the attribute type definitions for this schema.
        /// </summary>
        /// <value>The attribute type definitions.</value>
        IEntityCollection<IAttributeTypeDefinition> IAttributionSchemaDefinition.AttributeTypeDefinitions
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}