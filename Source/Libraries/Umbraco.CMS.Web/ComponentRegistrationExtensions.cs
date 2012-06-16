using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees;

namespace Umbraco.Cms.Web
{
    public static class ComponentRegistrationExtensions
    {
        /// <summary>
        /// Returns the tree controller name for the tree id passed in
        /// </summary>
        /// <param name="trees"></param>
        /// <param name="treeId"></param>
        /// <returns></returns>
        public static string GetTreeName(this IEnumerable<Lazy<TreeController, TreeMetadata>> trees, Guid treeId)
        {
            var tree = trees.Where(x => x.Metadata.Id == treeId).SingleOrDefault();
            return tree != null ? tree.Metadata.ControllerName : null;
        }

    }
}
