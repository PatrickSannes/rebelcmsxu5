using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Trees
{
    public class MenuItemMetadata : PluginMetadataComposition
    {
        public MenuItemMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }

        /// <summary>
        /// The icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The text to display for the menu item
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Show seperator before
        /// </summary>
        public bool SeperatorBefore { get; set; }

        /// <summary>
        /// Show seperator after
        /// </summary>
        public bool SeperatorAfter { get; set; }

        /// <summary>
        /// The JS function to call when the menu item is clicked
        /// </summary>
        public string OnClientClick { get; set; }
    }
}