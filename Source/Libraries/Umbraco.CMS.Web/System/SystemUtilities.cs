using System.Security;
using System.Web;

namespace Umbraco.Cms.Web.System
{
    public static class SystemUtilities
    {
        public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            foreach (var trustLevel in new[] {
                                                 AspNetHostingPermissionLevel.Unrestricted,
                                                 AspNetHostingPermissionLevel.High,
                                                 AspNetHostingPermissionLevel.Medium,
                                                 AspNetHostingPermissionLevel.Low,
                                                 AspNetHostingPermissionLevel.Minimal })
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                }
                catch (SecurityException)
                {
                    continue;
                }

                return trustLevel;
            }

            return AspNetHostingPermissionLevel.None;
        }
    }
}