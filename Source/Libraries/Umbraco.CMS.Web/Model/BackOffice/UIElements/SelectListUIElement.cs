using System.Collections.Generic;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    /// <summary>
    /// Represents a select list UI Element
    /// </summary>
    [UIElement("926C640D-8E13-4208-B98D-13B5E746AE13", "Umbraco.UI.UIElements.SelectListUIElement")]
    public class SelectListUIElement : InteractiveUIElement
    {
        /// <summary>
        /// The items that will make up the select list
        /// </summary>
        public IEnumerable<SelectListItem> Items { get; set; }
    }
}
