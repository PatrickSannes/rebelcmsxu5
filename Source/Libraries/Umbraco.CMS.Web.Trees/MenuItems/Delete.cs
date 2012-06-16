using System;
using System.Collections.Generic;
using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("9F38DF60-273F-45FA-AEF5-7A74716FEC41",
        "Delete", true, true,
        "Umbraco.Controls.MenuItems.deleteItem",
        "menu-delete")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Delete })]
    public class Delete : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "deleteUrl"}; }
        }
    }
}