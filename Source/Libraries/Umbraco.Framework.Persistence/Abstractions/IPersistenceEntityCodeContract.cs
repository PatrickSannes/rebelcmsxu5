using System;
using System.Diagnostics.Contracts;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.Persistence.Abstractions.Versioning;

namespace Umbraco.Framework.Persistence.Abstractions
{
    [ContractClassFor(typeof (IPersistenceEntity))]
    internal abstract class IPersistenceEntityCodeContract : IPersistenceEntity
    {
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(UtcCreated < DateTime.UtcNow.AddDays(1));
            Contract.Invariant(UtcModified >= UtcCreated);
        }

        #region Implementation of ITracksConcurrency

        public IConcurrencyToken ConcurrencyToken { get; set; }

        #endregion

        #region Implementation of IPersistenceEntity

        public IPersistenceEntityStatus Status { get; set; }
        public DateTime UtcCreated { get; set; }
        public DateTime UtcModified { get; set; }
        public DateTime UtcStatusChanged { get; set; }
        public PersistenceEntityUri Id { get; set; }
        public IRevisionData Revision { get; set; }

        #endregion
    }
}