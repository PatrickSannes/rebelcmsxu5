using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("992019E5-FC42-4BD8-B8AB-859A7E29B1FB",
        "Logout",
        "Umbraco.Controls.MenuItems.Logout",
        "menu-logout")]
    public class Logout : MenuItem { }
}