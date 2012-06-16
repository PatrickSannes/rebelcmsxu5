using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Language, "Language", FixedPermissionTypes.EntityAction)]
    public class LanguagePermission : Permission
    { }
}