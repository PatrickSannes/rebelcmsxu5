using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Framework.Persistence.XmlStore
{
    public class HiveReadWriteProvider : AbstractHiveReadWriteProvider
    {
        public HiveReadWriteProvider(IHiveProviderSetup setup)
            : base(setup)
        {
        }
    }
}
