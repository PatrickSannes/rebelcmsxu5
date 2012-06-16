using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    public class UIElementAttribute : PluginAttribute
    {
        public string JsType { get; set; }

        public UIElementAttribute(string id) 
            : base(id)
        { }

        public UIElementAttribute(string id, string jsType)
            : this(id)
        {
            JsType = jsType;
        }
    }
}
