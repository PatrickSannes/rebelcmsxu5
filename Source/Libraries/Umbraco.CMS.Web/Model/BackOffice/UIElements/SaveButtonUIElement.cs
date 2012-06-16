using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Save })]
    [UIElement("16B34920-4E91-4B25-B9EE-C25D998A9EDA", "Umbraco.UI.UIElements.ButtonUIElement")]
    public class SaveButtonUIElement : ButtonUIElement
    {
        public SaveButtonUIElement()
        {
            Alias = "Save";
            Title = "Save";
            CssClass = "save-button toolbar-button";
            AdditionalData = new Dictionary<string, string>
            {
                { "id", "submit_Save" },
                { "name", "submit.Save" },
                { "data-shortcut", "ctrl S click true" }
            };
        }
    }
}
