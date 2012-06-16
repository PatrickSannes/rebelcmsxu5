using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Update, "Update", FixedPermissionTypes.EntityAction)]
    public class UpdatePermission : Permission
    { }
}