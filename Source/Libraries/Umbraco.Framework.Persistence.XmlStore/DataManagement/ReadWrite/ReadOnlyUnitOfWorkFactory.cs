
using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement.ReadWrite
{
    public class ReadOnlyUnitOfWorkFactory : AbstractReadOnlyUnitOfWorkFactory
    {
        

        public override IReadOnlyUnitOfWork CreateForReading(IHiveProvider hiveProvider)
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, "DemoData...ReadWriteUnitOfWorkFactory:CreateForReading");
            return new ReadOnlyUnitOfWork(hiveProvider.DataContextFactory.CreateDataContext(hiveProvider) as DataContext);
        }

    }
}