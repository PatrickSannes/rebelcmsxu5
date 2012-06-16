using System;
using System.Diagnostics.Contracts;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.Persistence.Abstractions.Attribution;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Abstractions.Versioning;

namespace Umbraco.Framework.Persistence.Abstractions
{
    [ContractClassFor(typeof (ITypedPersistenceEntity))]
    internal abstract class ITypedPersistenceEntityCodeContract : ITypedPersistenceEntity
    {
        #region Implementation of ITracksConcurrency

        public IConcurrencyToken ConcurrencyToken { get; set; }

        #endregion

        #region Implementation of IPersistenceEntity

        public IPersistenceEntityStatus Status
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

        public PersistenceEntityUri Id
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

        #region Implementation of ITypedPersistenceEntity

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
            set { Contract.Requires<ArgumentNullException>(value != null); }
        }

        #endregion

    }
}