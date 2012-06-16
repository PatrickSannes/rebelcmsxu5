using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("78FEF28C-4090-43A7-8176-27C26727DAB0",
        "Publish",
        true, false,
        "Umbraco.Controls.MenuItems.publish",
        "menu-publish")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Publish })]
    public class Publish : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "publishUrl" }; }
        }
    }
}