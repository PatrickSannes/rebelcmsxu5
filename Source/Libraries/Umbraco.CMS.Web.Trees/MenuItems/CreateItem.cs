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
    [MenuItem("D0635D22-B66C-4DC9-8442-3325CE5D5EE9", "Create", false, true, "Umbraco.Controls.MenuItems.createItem", "menu-create")]
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Create })]
    public class CreateItem : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "createUrl" }; }
        }
    }
}
