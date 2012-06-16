using System;
using System.Diagnostics.Contracts;

namespace Umbraco.Framework.Persistence.Abstractions.Versioning
{
    [ContractClassFor(typeof (IBranch))]
    internal abstract class IBranchCodeContract : IBranch
    {
        #region Implementation of IBranch

        public PersistenceEntityUri BranchId
        {
            get
            {
                Contract.Ensures(Contract.Result<IMappedIdentifier>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires<ArgumentNullException>(value != null); }
        }

        public string Name
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                // Dummy return
                return null;
            }
            set { Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(value)); }
        }

        #endregion
    }
}