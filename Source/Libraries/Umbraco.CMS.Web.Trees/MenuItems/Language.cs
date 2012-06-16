using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("D9AA73D3-2993-409E-9877-0551DC7D56BB",
        "Language",
        "Umbraco.Controls.MenuItems.language",
        "menu-language")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Language })]
    public class Language : MenuItem { }
}