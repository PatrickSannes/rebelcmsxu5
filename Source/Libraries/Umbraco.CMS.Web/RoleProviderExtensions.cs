using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Umbraco.Cms.Web.Security;

namespace Umbraco.Cms.Web
{
    public static class RoleProviderExtensions
    {
        public static RoleProvider GetBackOfficeRoleProvider(this RoleProviderCollection providers)
        {
            return providers["BackOfficeRoleProvider"];
        }

        public static bool IsUsingDefaultBackOfficeRoleProvider(this RoleProviderCollection providers)
        {
            var provider = providers.GetBackOfficeRoleProvider();
            return provider.IsDefaultBackOfficeRoleProvider();
        }

        public static bool IsDefaultBackOfficeRoleProvider(this RoleProvider provider)
        {
            return provider is BackOfficeRoleProvider;
        }
    }
}
