using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.Abstractions.Versioning;

namespace Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData
{
    [ContractClassFor(typeof (IAttributeTypeDefinition))]
    internal abstract class IAttributeTypeDefinitionCodeContract :
        IAttributeTypeDefinition
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

        #region Implementation of IAttributeTypeDefinition

        public DataSerializationTypes DataSerializationType { get; set; }
        public LocalizedString Description { get; set; }

        /// <summary>
        /// A keyed collection of UI data.
        /// </summary>
        /// <value>The UI data.</value>
        public Dictionary<string, string> UIData
        {
            get { throw new NotImplementedException(); }
        }

        public int Ordinal { get; set; }

        #endregion

        #region Implementation of ITracksConcurrency

        /// <summary>
        ///   Gets or sets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        public IConcurrencyToken ConcurrencyToken { get; set; }

        #endregion

        #region Implementation of IPersistenceEntity

        /// <summary>
        ///   Gets or sets the status of the entity.
        /// </summary>
        /// <value>The status.</value>
        public IPersistenceEntityStatus Status
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///   Gets or sets the creation date of the entity (UTC).
        /// </summary>
        /// <value>The UTC created.</value>
        public DateTime UtcCreated
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///   Gets or sets the modification date of the entity (UTC).
        /// </summary>
        /// <value>The UTC modified.</value>
        public DateTime UtcModified
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///   Gets or sets the timestamp when the status of this entity was last changed (in UTC).
        /// </summary>
        /// <value>The UTC status changed.</value>
        public DateTime UtcStatusChanged
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///   Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public PersistenceEntityUri Id
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///   Gets or sets the revision data.
        /// </summary>
        /// <value>The revision.</value>
        public IRevisionData Revision
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region Implementation of IComparable<in int>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(int other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IAttributeTypeDefinition

        public IAttributeSerializationDefinition SerializationType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}