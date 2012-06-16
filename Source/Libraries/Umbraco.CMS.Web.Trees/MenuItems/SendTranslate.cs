using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("0C177931-2812-483F-85AB-320EEBCBF217",
        "Send to Translate",
        false, true,
        "Umbraco.Controls.MenuItems.SendTranslate",
        "menu-send-translate")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.SendToTranslate })]
    public class SendTranslate : MenuItem { }
}