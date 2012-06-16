using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("B75585C9-44AC-4397-9326-A0D8B58A027D",
        "Empty recycle bin", false, true,
        "Umbraco.Controls.MenuItems.emptyBin",
        "menu-empty-bin")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.EmptyRecycleBin })]
    public class EmptyRecycleBin : MenuItem { }
}