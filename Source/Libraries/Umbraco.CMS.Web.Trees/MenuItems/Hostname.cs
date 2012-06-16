using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("075EAE6E-96C3-4CBC-9882-017ED1A03D65",
        "Manage hostnames",
        "Umbraco.Controls.MenuItems.hostname",
        "menu-domain")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Hostnames })]
    public class Hostname : MenuItem { }
}