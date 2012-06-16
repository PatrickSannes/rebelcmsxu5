using System.Diagnostics.Contracts;

namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    [ContractClassFor(typeof (IRevisionData))]
    internal abstract class IRevisionDataCodeContract : IRevisionData
    {
        #region Implementation of IRevisionData

        public IMappedIdentifier RevisionId
        {
            get
            {
                Contract.Ensures(Contract.Result<IMappedIdentifier>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires(value != null); }
        }

        public IChangeset Changeset
        {
            get
            {
                Contract.Ensures(Contract.Result<IChangeset>() != null);

                // Dummy return
                return null;
            }
            set { Contract.Requires(value != null); }
        }

        #endregion
    }
}