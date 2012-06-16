using Umbraco.Framework;

namespace Umbraco.Cms.Web.Surface
{
    /// <summary>
    /// Identifies a editor controller
    /// </summary>    
    public class SurfaceAttribute : PluginAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="id"></param>
        public SurfaceAttribute(string id)
            : base(id)
        {
        }

    }
}
