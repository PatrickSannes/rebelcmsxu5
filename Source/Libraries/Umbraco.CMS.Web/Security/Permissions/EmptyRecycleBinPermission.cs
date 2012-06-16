using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.EmptyRecycleBin, "Empty Recycle Bin", FixedPermissionTypes.EntityAction)]
    public class EmptyRecycleBinPermission : Permission
    { }
}