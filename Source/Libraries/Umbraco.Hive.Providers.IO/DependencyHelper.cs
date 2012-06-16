using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Hive.Providers.IO
{
    public class DependencyHelper : ProviderDependencyHelper 
    {
        public DependencyHelper(Settings settings, ProviderMetadata providerMetadata) : base(providerMetadata)
        {
            Settings = settings;
        }

        public Settings Settings { get; protected set; }

        protected override void DisposeResources()
        {
            return;
        }
    }
}
