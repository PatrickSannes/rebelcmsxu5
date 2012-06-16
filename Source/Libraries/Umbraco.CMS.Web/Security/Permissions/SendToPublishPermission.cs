using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.SendToPublish, "Send to Publish", FixedPermissionTypes.EntityAction)]
    public class SendToPublishPermission : Permission
    { }
}