using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Publish, "Publish", FixedPermissionTypes.EntityAction)]
    public class PublishPermission : Permission
    { }
}