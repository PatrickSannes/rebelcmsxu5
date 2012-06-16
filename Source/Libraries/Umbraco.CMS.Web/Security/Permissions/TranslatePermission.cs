using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Translate, "Translate", FixedPermissionTypes.EntityAction)]
    public class TranslatePermission : Permission
    { }
}