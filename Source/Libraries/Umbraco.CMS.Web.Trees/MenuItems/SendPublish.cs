using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("58798353-9378-4BF1-8E61-B557C993511D",
        "Send to publish",
        "Umbraco.Controls.MenuItems.SendPublish",
        "menu-send-publish")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.SendToPublish })]
    public class SendPublish : MenuItem { }
}