using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Sort, "Sort", FixedPermissionTypes.EntityAction)]
    public class SortPermission : Permission
    { }
}