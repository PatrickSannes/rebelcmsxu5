using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Framework.Persistence.XmlStore
{
    public class HiveReadProvider : AbstractHiveReadProvider
    {
        public HiveReadProvider(IHiveProviderSetup setup)
            : base(setup)
        {
        }
    }
}