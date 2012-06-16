using System.Collections.Generic;
using System.Text;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Macros
{
    /// <summary>
    /// Identifies a editor controller
    /// </summary>    
    public class MacroEngineAttribute : PluginAttribute
    {
        public string EngineName { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="id"></param>
        /// <param name="engineName">The name of the Macro engine</param>
        public MacroEngineAttribute(string id, string engineName)
            : base(id)
        {
            EngineName = engineName;
        }
    }
}
