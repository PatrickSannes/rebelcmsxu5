using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("37B61A49-6595-491C-92E4-28C7A37508EE",
        "Create Folder",
        "Umbraco.Controls.MenuItems.CreateFolder",
        "menu-create-folder")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Create })]
    public class CreateFolder : MenuItem { }
}