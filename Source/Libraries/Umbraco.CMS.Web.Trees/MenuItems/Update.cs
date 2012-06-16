using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("11C20C07-B76D-4C87-9C92-97054CAEC092",
        "Update",
        "Umbraco.Controls.MenuItems.Update",
        "menu-update")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Update })]
    public class Update : MenuItem { }
}