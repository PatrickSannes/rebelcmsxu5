using System.Diagnostics.Contracts;

namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    [ContractClassFor(typeof (IBranch))]
    internal abstract class IBranchCodeContract : IBranch
    {
        #region Implementation of IBranch

        public IMappedIdentifier BranchId
        {
            get
            {
                Contract.Ensures(Contract.Result<IMappedIdentifier>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires(value != null); }
        }

        public string Name
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                // Dummy return
                return null;
            }
            set { Contract.Requires(!string.IsNullOrWhiteSpace(value)); }
        }

        #endregion
    }
}