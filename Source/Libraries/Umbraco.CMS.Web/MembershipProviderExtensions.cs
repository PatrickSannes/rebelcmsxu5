using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Umbraco.Cms.Web.Security;

namespace Umbraco.Cms.Web
{
    public static class MembershipProviderExtensions
    {
        public static MembershipProvider GetBackOfficeMembershipProvider(this MembershipProviderCollection providers)
        {
            return providers["BackOfficeMembershipProvider"];
        }

        public static bool IsUsingDefaultBackOfficeMembershipProvider(this MembershipProviderCollection providers)
        {
            var provider = providers.GetBackOfficeMembershipProvider();
            return provider.IsDefaultBackOfficeMembershipProvider();
        }

        public static bool IsDefaultBackOfficeMembershipProvider(this MembershipProvider provider)
        {
            return provider is BackOfficeMembershipProvider;
        }
    }
}
