using System;
using System.Collections.Generic;
using System.Web.Security;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.Providers.Membership.Config;

namespace Umbraco.Hive.Providers.Membership.Hive
{
    public class DependencyHelper : ProviderDependencyHelper
    {

        /// <summary>        
        /// Returns the MembershipProvider's associated with this Hive provider
        /// </summary>
        /// <remarks>
        /// NOTE: This must be lazy call because we cannot initialize membership providers now as exceptions will occur
        /// because our Hive membersip provider hasn't been initialized yet, plus its a bit more performant.
        /// </remarks>
        public Lazy<IEnumerable<MembershipProvider>> MembershipProviders { get; private set; }

        public IEnumerable<ProviderElement> ConfiguredProviders { get; private set; }


        public DependencyHelper(
            IEnumerable<ProviderElement> configuredProviders,
            Lazy<IEnumerable<MembershipProvider>> membershipProviders,             
            ProviderMetadata providerMetadata)
            : base(providerMetadata)
        {
            MembershipProviders = membershipProviders;
            ConfiguredProviders = configuredProviders;
        }

        protected override void DisposeResources()
        {
            
        }
    }
}