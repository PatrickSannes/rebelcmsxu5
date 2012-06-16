using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Permissions, "Permissions", FixedPermissionTypes.EntityAction)]
    public class PermissionsPermission : Permission
    { }
}