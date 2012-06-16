using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    /// <summary>
    /// Represents a UI Element
    /// </summary>
    public abstract class UIElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UIElement"/> class.
        /// </summary>
        protected UIElement()
        {
            //Locate the editor attribute
            var editorAttributes = GetType()
                .GetCustomAttributes(typeof(UIElementAttribute), false)
                .OfType<UIElementAttribute>();

            if (!editorAttributes.Any())
            {
                throw new InvalidOperationException("The UI Element is missing the " + typeof(UIElementAttribute).FullName + " attribute");
            }

            var attr = editorAttributes.First();

            //assign the properties of this object to those of the metadata attribute
            JsType = attr.JsType;

            // Set the default UI Panel alias
            UIPanelAlias = "Default";
            AdditionalData = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the js type for this UIElement..
        /// </summary>
        /// <value>
        /// The type of the js.
        /// </value>
        public string JsType { get; private set; }

        /// <summary>
        /// Gets or sets the UI Panel alias.
        /// </summary>
        /// <value>
        /// The UI Panel alias.
        /// </value>
        public string UIPanelAlias { get; protected internal set; }

        /// <summary>
        /// The CSS class name to set for the UIElement.
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// Allows a developer to add additional meta data to the UIElement which can be queried in 
        /// JavaScript for any additional functionality
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; }

        /// <summary>
        /// Gets the type of the UI Element.
        /// </summary>
        public string Type
        {
            get
            {
                return this.GetType().FullName;
            }
        }
    }
}
