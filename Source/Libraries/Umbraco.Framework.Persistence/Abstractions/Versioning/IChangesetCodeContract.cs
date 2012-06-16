using System;
using System.Diagnostics.Contracts;

namespace Umbraco.Framework.Persistence.Abstractions.Versioning
{
    [ContractClassFor(typeof (IChangeset))]
    internal abstract class IChangesetCodeContract : IChangeset
    {
        #region Implementation of IChangeset

        public PersistenceEntityUri Id
        {
            get
            {
                Contract.Ensures(Contract.Result<IMappedIdentifier>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires<ArgumentNullException>(value != null); }
        }

        public IBranch Branch
        {
            get
            {
                Contract.Ensures(Contract.Result<IBranch>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires<ArgumentNullException>(value != null); }
        }

        #endregion
    }
}