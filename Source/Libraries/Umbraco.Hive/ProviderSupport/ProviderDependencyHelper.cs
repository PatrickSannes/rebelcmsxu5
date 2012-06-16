using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public abstract class ProviderDependencyHelper : DisposableObject
    {
        protected ProviderDependencyHelper(ProviderMetadata providerMetadata)
        {
            ProviderMetadata = providerMetadata;
        }

        public ProviderMetadata ProviderMetadata { get; protected set; }
    }
}
