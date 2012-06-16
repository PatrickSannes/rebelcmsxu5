using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Trees
{
    /// <summary>
    /// Identifies a tree controller
    /// </summary>
    public class TreeAttribute : PluginAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="id"></param>
        /// <param name="title"></param>
        public TreeAttribute(string id, string title)
            :base(id)
        {            
            TreeTitle = title;
        }
        
        public string TreeTitle { get; private set; }

    }
}
