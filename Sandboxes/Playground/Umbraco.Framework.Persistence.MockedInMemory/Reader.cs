using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.Persistence.MockedInMemory
{
    public class Reader : ReaderBase
    {
        private Reader()
            : base()
        {
        }

        public Reader(IReadRepositoryUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }
    }
}