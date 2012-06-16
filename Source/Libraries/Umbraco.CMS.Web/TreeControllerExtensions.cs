using System.Web;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;

namespace Umbraco.Cms.Web
{
    public static class TreeControllerExtensions
    {

        /// <summary>
        /// Returns the application root node for a tree
        /// </summary>
        /// <param name="treeController"></param>
        /// <returns></returns>
        public static HiveId GetRootNodeId(this TreeController treeController)
        {
            return new HiveId(treeController.TreeId);
        }

    }
}
