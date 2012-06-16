using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Delete, "Delete", FixedPermissionTypes.EntityAction)]
    public class DeletePermission : Permission
    { }
}