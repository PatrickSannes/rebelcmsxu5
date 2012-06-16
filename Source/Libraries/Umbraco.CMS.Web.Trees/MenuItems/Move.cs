using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("A3940031-5A80-4633-BB7C-2D2D2521E95F", 
        "Move",
        true, false,
        "Umbraco.Controls.MenuItems.moveItem",
        "menu-move")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Move })]
    public class Move : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "moveUrl" }; }
        }
    }
}