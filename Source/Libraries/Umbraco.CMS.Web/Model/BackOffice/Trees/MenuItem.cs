using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Cms.Web.Model.BackOffice.Trees
{
    /// <summary>
    /// Represents a context menu item
    /// </summary>    
    public abstract class MenuItem
    {

        /// <summary>
        /// Constructor
        /// </summary>
        protected MenuItem()
        {
            //Locate the editor attribute
            var editorAttributes = GetType()
                .GetCustomAttributes(typeof(MenuItemAttribute), false)
                .OfType<MenuItemAttribute>();

            if (!editorAttributes.Any())
            {
                throw new InvalidOperationException("The Menu item is missing the " + typeof(MenuItemAttribute).FullName + " attribute");
            }

            //assign the properties of this object to those of the metadata attribute
            var attr = editorAttributes.First();
            
            //set this objects properties
            this.MenuItemId = attr.Id;
            this.AdditionalData = attr.AdditionalData ?? new Dictionary<string, string>();
            this.Icon = attr.Icon;
            this.OnClientClick = attr.OnClientClick;
            this.SeparatorAfter = attr.SeparatorAfter;
            this.SeparatorBefore = attr.SeparatorBefore;
            this.Title = attr.Title;
        }

        private readonly List<MenuItem> _subMenuItems = new List<MenuItem>();

        /// <summary>
        /// Used to validate the additional data / meta data that is appended to a tree node as some menu items require
        /// certain data to operate with their JS methods.
        /// </summary>
        /// <param name="additionalData"></param>
        public virtual void ValidateRequiredData(IDictionary<string, object> additionalData)
        {
            //do nothing, leave for inheritors
        }

        /// <summary>
        /// The text to display for the menu item
        /// </summary>
        public string Title { get; private set; }
        
        /// <summary>
        /// A collection of sub menu items for this menu item
        /// </summary>
        public virtual IEnumerable<MenuItem> SubMenuItems
        {
            get { return _subMenuItems; }
        }

        /// <summary>
        /// A property to allow for customized html data attributes to be rendered
        /// which can be useful for doing custom logic in JavaScript
        /// </summary>
        /// <remarks>
        /// Custom data will be rendered into the DOM with attributes named with the prefix:
        /// 'data-custom-'
        /// </remarks>
        public IDictionary<string, string> AdditionalData { get; private set; }

        /// <summary>
        /// The icon to be used for the menu item.
        /// This can either be a path to an image or a css class reference
        /// </summary>
        public string Icon { get; private set; }

        /// <summary>
        /// Returns the menu item id assigned to the menu item via attributes
        /// </summary>
        public Guid MenuItemId { get; private set; }

        /// <summary>
        /// Show seperator before
        /// </summary>
        public bool SeparatorBefore { get; private set; }

        /// <summary>
        /// Show seperator after
        /// </summary>
        public bool SeparatorAfter { get; private set; }

        /// <summary>
        /// The JavaScript method to call when the menu item is clicked
        /// </summary>
        /// <remarks>
        /// The JavaScript method called will be passed the node Id that has had it's context menu initialized
        /// </remarks>
        public string OnClientClick { get; private set; }
    }
}