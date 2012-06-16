using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("8977395A-9DF8-4A38-97C3-E35B36BE2151",
        "Audit",
        true, true,
        "Umbraco.Controls.MenuItems.Audit",
        "menu-audit")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Audit })]
    public class Audit : MenuItem { }
}