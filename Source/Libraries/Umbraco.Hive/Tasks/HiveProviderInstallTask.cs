using Umbraco.Framework.Context;
using Umbraco.Framework.Tasks;

namespace Umbraco.Hive.Tasks
{
    public abstract class HiveProviderInstallTask : ProviderInstallTask
    {
        private readonly IHiveManager _coreManager;

        protected HiveProviderInstallTask(IFrameworkContext context, IHiveManager coreManager) : base(context)
        {
            _coreManager = coreManager;
        }

        public IHiveManager CoreManager
        {
            get { return _coreManager; }
        }
    }
}
