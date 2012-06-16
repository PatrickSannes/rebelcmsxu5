using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public class NullProviderDependencyHelper : ProviderDependencyHelper
    {
        public NullProviderDependencyHelper(ProviderMetadata providerMetadata) : base(providerMetadata)
        {
        }

        protected override void DisposeResources()
        {
            return;
        }
    }
}
