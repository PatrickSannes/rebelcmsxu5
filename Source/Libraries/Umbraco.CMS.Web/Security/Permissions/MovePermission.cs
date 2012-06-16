using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Move, "Move", FixedPermissionTypes.EntityAction)]
    public class MovePermission : Permission
    { }
}