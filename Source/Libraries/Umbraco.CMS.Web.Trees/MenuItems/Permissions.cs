using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7C5D12C0-6D3B-482D-B80E-B6F3FFCEF934",
        "Permissions",
        false, true,
        "Umbraco.Controls.MenuItems.permissions",
        "menu-permissions")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Permissions })]
    public class Permissions : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "permissionsUrl" }; }
        }
    }
}