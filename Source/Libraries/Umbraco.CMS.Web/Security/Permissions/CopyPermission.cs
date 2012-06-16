using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Copy, "Copy", FixedPermissionTypes.EntityAction)]
    public class CopyPermission : Permission
    { }
}