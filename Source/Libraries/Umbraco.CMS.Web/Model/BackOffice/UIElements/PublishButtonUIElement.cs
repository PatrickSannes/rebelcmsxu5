using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Publish })]
    [UIElement("CC9CB1EB-75BE-49A4-938B-1332E4D43859", "Umbraco.UI.UIElements.ButtonUIElement")]
    public class PublishButtonUIElement : ButtonUIElement
    {
        public PublishButtonUIElement()
        {
            Alias = "Publish";
            Title = "Publish";
            CssClass = "publish-button toolbar-button";
            AdditionalData = new Dictionary<string, string>
            {
                { "id", "submit_Publish" },
                { "name", "submit.Publish" }
            };
        }
    }
}
