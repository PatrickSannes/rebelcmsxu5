using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Packages.SystemInfo.Controllers
{
    [Tree("FD72E12C-2FF5-4B51-8F60-F3B57E06080E", "System")]
    public class SystemInfoTreeController : TreeController, ISearchableTree
    {
        private readonly Guid _editorControllerId = new Guid("5D85C1EC-ED5C-451E-A53F-78CC95AA53A2");

        public SystemInfoTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// Represents the list of nodes for the tree. 
        /// </summary>
        /// <remarks>
        /// Each item in the Tuple represents: 
        /// 
        /// the node ID, 
        /// the name, 
        /// the controller Action name
        /// 
        /// </remarks>
        private static IEnumerable<Tuple<HiveId, string, string>> SystemInfoNodes
        {
            get
            {
                return new List<Tuple<HiveId, string, string>>
                             {
                                 new Tuple<HiveId, string, string>(new HiveId(1), "Plugins", "PluginInfo"), 
                                 new Tuple<HiveId, string, string>(new HiveId(2), "Backup/Restore", "Backup"),
                                 new Tuple<HiveId, string, string>(new HiveId(3), "Permissions", "Permissions")
                             };
            }
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            //First node - get the root entities
            if (parentId == RootNodeId)
            {
                foreach (var node in SystemInfoNodes
                    .Select(x =>
                        CreateTreeNode(
                            x.Item1,
                            queryStrings,
                            x.Item2,
                            Url.GetEditorUrl(x.Item3, null, _editorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings),
                            x.Item3)))
                {
                    node.HasChildren = false;
                    NodeCollection.Add(node);
                }
            }
            else
            {
                throw new NotSupportedException("The SystemInfo tree does not support more than 1 level");    
            }

            return UmbracoTree();
        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.SystemRoot; }
        }

        public TreeSearchJsonResult Search(string searchText)
        {
            return new TreeSearchJsonResult(
                SystemInfoNodes.Where(x => x.Item2.StartsWith(searchText))
                    .Select(x => new SearchResultItem
                        {
                            Title = x.Item2,
                            Description = x.Item2,
                            Id = x.Item1.ToString()
                        }));

        }
    }
}
