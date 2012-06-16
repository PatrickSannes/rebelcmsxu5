using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7AF3CE17-5FEA-4C20-B0F0-08676519194F",
        "Protect",
        true, true,
        "Umbraco.Controls.MenuItems.Protect",
        "menu-protect")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.PublicAccess })]
    public class Protect : MenuItem { }
}