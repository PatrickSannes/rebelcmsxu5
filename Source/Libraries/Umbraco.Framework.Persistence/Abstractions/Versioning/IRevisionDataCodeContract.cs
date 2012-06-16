using System;
using System.Diagnostics.Contracts;

namespace Umbraco.Framework.Persistence.Abstractions.Versioning
{
    [ContractClassFor(typeof (IRevisionData))]
    internal abstract class IRevisionDataCodeContract : IRevisionData
    {
        #region Implementation of IRevisionData

        public PersistenceEntityUri RevisionId
        {
            get
            {
                Contract.Ensures(Contract.Result<IMappedIdentifier>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires<ArgumentNullException>(value != null); }
        }

        public IChangeset Changeset
        {
            get
            {
                Contract.Ensures(Contract.Result<IChangeset>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires<ArgumentNullException>(value != null); }
        }

        #endregion
    }
}