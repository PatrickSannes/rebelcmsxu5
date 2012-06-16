using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.BackOfficeAccess, "Back-Office Access", FixedPermissionTypes.System)]
    public class BackOfficeAccessPermission : Permission
    { }
}