using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.Persistence.DemoData
{
    public class Manager : PersistenceManagerBase
    {
        public Manager(string @alias, IPersistenceReadWriter reader, IPersistenceReadWriter readWriter, IFrameworkContext frameworkContext)
            : base(@alias, reader, readWriter, frameworkContext)
        { }
    }
}
