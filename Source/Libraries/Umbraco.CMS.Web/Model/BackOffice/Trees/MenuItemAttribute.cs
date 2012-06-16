using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Trees
{
    public class MenuItemAttribute : PluginAttribute
    {
        /// <summary>
        /// A property to allow for customized html data attributes to be rendered
        /// which can be useful for doing custom logic in JavaScript
        /// </summary>
        /// <remarks>
        /// Custom data will be rendered into the DOM with attributes named with the prefix:
        /// 'data-custom-'
        /// </remarks>
        public IDictionary<string, string> AdditionalData { get; set; }

        public string OnClientClick { get; set; }
        public string Title { get; private set; }
        public bool SeparatorBefore { get; set; }
        public bool SeparatorAfter { get; set; }
        public string Icon { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="id"></param>
        /// <param name="title"></param>
        private MenuItemAttribute(string id, string title)
            :base(id)
        {
            Title = title;
            SeparatorAfter = false;
            SeparatorBefore = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="onClientClick"></param>
        public MenuItemAttribute(string id, string title, string onClientClick)
            :this(id, title)
        {
            OnClientClick = onClientClick;
        }

        public MenuItemAttribute(string id, string title, string onClientClick, string icon)
            : this(id, title)
        {
            OnClientClick = onClientClick;
            Icon = icon;
        }

        /// <summary>
        /// Constructor with seperators
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="seperatorBefore"></param>
        /// <param name="seperatorAfter"></param>
        public MenuItemAttribute(string id, string title, bool seperatorBefore, bool seperatorAfter)
            : this(id, title)
        {
            SeparatorBefore = seperatorBefore;
            SeparatorAfter = seperatorAfter;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="seperatorBefore"></param>
        /// <param name="seperatorAfter"></param>
        /// <param name="onClientClick"></param>
        public MenuItemAttribute(string id, string title, bool seperatorBefore, bool seperatorAfter, string onClientClick)
            : this(id, title, seperatorBefore, seperatorAfter)
        {
            OnClientClick = onClientClick;
        }

        public MenuItemAttribute(string id, string title, bool seperatorBefore, bool seperatorAfter, string onClientClick, string icon)
            : this(id, title, seperatorBefore, seperatorAfter)
        {
            OnClientClick = onClientClick;
            Icon = icon;
        }
    }
}
