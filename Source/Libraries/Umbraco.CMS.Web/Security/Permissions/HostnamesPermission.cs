using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Hostnames, "Hostnames", FixedPermissionTypes.EntityAction)]
    public class HostnamesPermission : Permission
    { }
}