using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.View, "View", FixedPermissionTypes.EntityAction)]
    public class ViewPermission : Permission
    { }
}