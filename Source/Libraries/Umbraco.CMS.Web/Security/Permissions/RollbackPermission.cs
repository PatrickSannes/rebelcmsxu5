using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Rollback, "Rollback", FixedPermissionTypes.EntityAction)]
    public class RollbackPermission : Permission
    { }
}