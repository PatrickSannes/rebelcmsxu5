using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;

namespace Umbraco.Cms.Web.Trees.MenuItems
{

    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("FAEEA1A6-5EEE-408C-98E7-22168335A246",
        "Refresh", true, false,
        "Umbraco.Controls.MenuItems.reloadChildren",
        "menu-refresh")]
    public class Reload : MenuItem { }
}