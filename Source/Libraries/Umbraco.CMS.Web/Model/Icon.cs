using System.Drawing;
using Umbraco.Cms.Web.Model.BackOffice;

namespace Umbraco.Cms.Web.Model
{

    /// <summary>
    /// Represents an icon
    /// </summary>
    public class Icon
    {
        public string Name { get; set; }

        /// <summary>
        /// If the icon is an image, not a sprite, this represents it's URL/Path
        /// </summary>
        public string Url { get; set; }
        public Size IconSize { get; set; }

        /// <summary>
        /// Is a physical image or a sprite contained in an image
        /// </summary>
        public IconType IconType { get; set; }
    }
}
