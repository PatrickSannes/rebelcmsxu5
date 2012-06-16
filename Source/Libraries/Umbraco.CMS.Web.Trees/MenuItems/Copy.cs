using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("022FD04F-44FF-47FC-B6CC-83FDBD276F55",
        "Copy",
        false, true,
        "Umbraco.Controls.MenuItems.copyItem",
        "menu-copy")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Copy })]
    public class Copy : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "copyUrl" }; }
        }
    }
}