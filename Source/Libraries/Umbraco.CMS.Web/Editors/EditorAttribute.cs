using Umbraco.Framework;

namespace Umbraco.Cms.Web.Editors
{
    /// <summary>
    /// Identifies a editor controller
    /// </summary>    
    public class EditorAttribute : PluginAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="id"></param>
        public EditorAttribute(string id)
            :base(id)
        {
        }

        /// <summary>
        /// Flag to advertise that this editor controller has child actions to be used for dashboards
        /// </summary>
        public bool HasChildActionDashboards { get; set; }

    }
}
