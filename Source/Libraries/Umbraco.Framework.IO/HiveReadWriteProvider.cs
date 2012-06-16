using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.IO
{
    public class HiveReadWriteProvider : AbstractHiveReadWriteProvider
    {
        public HiveReadWriteProvider(IHiveProviderSetup setup)
            : base(setup)
        {
        }
    }
}