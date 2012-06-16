using System.Collections.Generic;
using Umbraco.Cms.Web.Model.BackOffice;

namespace Umbraco.Cms.Web.Editors
{
    public class EditorMetadata : ControllerPluginMetadata
    {
        public EditorMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }


        /// <summary>
        /// Whether or not this is an built-in Umbraco editor
        /// </summary>
        public bool IsInternalUmbracoEditor { get; set; }

        /// <summary>
        /// Flag to advertise if this Editor controller exposes child action dashboards
        /// </summary>
        public bool HasChildActionDashboards { get; set; }
        
    }
}
