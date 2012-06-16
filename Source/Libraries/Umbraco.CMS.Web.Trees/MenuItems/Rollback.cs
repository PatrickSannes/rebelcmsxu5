using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7B0926DD-A5CB-49BD-B60B-3C3FBC886603",
        "Rollback",
        false, true,
        "Umbraco.Controls.MenuItems.rollback",
        "menu-rollback")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Rollback })]
    public class Rollback : MenuItem { }
}