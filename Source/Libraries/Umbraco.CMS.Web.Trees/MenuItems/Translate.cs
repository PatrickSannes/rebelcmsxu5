using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("CB7CD841-A7B7-495D-B6A7-179A05A0C506",
        "Translate",
        "Umbraco.Controls.MenuItems.Translate",
        "menu-translate")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Translate })]
    public class Translate : MenuItem { }
}