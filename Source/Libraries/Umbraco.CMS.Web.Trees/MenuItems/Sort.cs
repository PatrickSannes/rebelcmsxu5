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
    [MenuItem("83177655-9F16-47BC-BD5D-B48E7E2D6CE3",
        "Sort",
        true, false,
        "Umbraco.Controls.MenuItems.sortItems",
        "menu-sort")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Sort })]
    public class Sort : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "sortUrl" }; }
        }
    }
}