using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Cms.Web.Mvc.ActionFilters;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    /// <summary>
    /// Represents a button UI Element
    /// </summary>
    [UIElement("3151725F-B3D9-4363-B759-E781EC9F2539", "Umbraco.UI.UIElements.ButtonUIElement")]
    public class ButtonUIElement : InteractiveUIElement
    {
        /// <summary>
        /// A relative path to an icon file to use for the button
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the type of the button.
        /// </summary>
        /// <value>
        /// The type of the button.
        /// </value>
        public ButtonType ButtonType { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ButtonType
    {
        Submit,
        Button,
        Image
    }
}
