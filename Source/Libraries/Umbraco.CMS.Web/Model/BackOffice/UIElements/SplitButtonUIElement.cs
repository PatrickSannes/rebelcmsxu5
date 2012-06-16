using System.Collections.Generic;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    /// <summary>
    /// Represents a split button UI Element
    /// </summary>
    public class SplitButtonUIElement : ButtonUIElement
    {
        /// <summary>
        /// The items that will make up the split button menu
        /// </summary>
        public IEnumerable<SelectListItem> Items { get; set; }
    }
}
