using System;
using System.Diagnostics.Contracts;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Versioning;

namespace Umbraco.Framework.EntityGraph.Domain.Entity
{
    [ContractClassFor(typeof (ITypedEntity))]
    internal abstract class ITypedEntityCodeContract : ITypedEntity
    {
        #region Implementation of ITracksConcurrency

        public IConcurrencyToken ConcurrencyToken { get; set; }

        #endregion

        #region Implementation of IEntity

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

        #region Implementation of ITypedEntity

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public ITypedAttributeCollection Attributes
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the attribute groups.
        /// </summary>
        /// <value>The attribute groups.</value>
        public IAttributeGroupCollection AttributeGroups
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IAttributionSchemaDefinition AttributionSchema
        {
            get
            {
                Contract.Ensures(Contract.Result<IAttributionSchemaDefinition>() != null);

                // Dummy return
                return null;
            }
             set { Contract.Requires(value != null); }
        }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>The type of the entity.</value>
        public IEntityTypeDefinition EntityType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

    }
}