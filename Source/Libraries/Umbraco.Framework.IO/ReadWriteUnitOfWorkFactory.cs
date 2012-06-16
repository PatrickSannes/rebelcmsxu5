using System.Web;
using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.IO
{
    public class ReadWriteUnitOfWorkFactory : AbstractReadWriteUnitOfWorkFactory
    {

        public override IReadOnlyUnitOfWork CreateForReading(IHiveProvider hiveProvider)
        {
            return new ReadWriteUnitOfWork(hiveProvider.DataContextFactory.CreateDataContext(hiveProvider));
        }

        public override IReadWriteUnitOfWork CreateForReadWriting(IHiveProvider hiveProvider)
        {
            return new ReadWriteUnitOfWork(hiveProvider.DataContextFactory.CreateDataContext(hiveProvider));
        }
    }
}