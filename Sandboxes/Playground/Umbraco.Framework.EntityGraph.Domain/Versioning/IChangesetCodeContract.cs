using System.Diagnostics.Contracts;

namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    [ContractClassFor(typeof (IChangeset))]
    internal abstract class IChangesetCodeContract : IChangeset
    {
        #region Implementation of IChangeset

        public IMappedIdentifier Id
        {
            get
            {
                Contract.Ensures(Contract.Result<IMappedIdentifier>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires(value != null); }
        }

        public IBranch Branch
        {
            get
            {
                Contract.Ensures(Contract.Result<IBranch>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires(value != null); }
        }

        #endregion
    }
}