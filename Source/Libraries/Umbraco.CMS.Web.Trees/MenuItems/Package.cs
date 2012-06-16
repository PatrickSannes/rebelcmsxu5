using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("1BA85CD2-978A-4D6E-AAE4-28AFF1CE4698",
        "Package",
        "Umbraco.Controls.MenuItems.Package",
        "menu-package")]
    public class Package : MenuItem { }
}