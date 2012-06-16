using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.WebPages;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Templates;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.TemplateTreeControllerId, "Templates")]
    [UmbracoTree]
    public class TemplateTreeController : AbstractFileSystemTreeController
    {
        public TemplateTreeController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.TemplateEditorControllerId); }
        }

        protected override string HiveUriRouteMatch
        {
            get { return "storage://templates"; }
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            using (var uow = Hive.CreateReadonly())
            {
                var repo = uow.Repositories;
                var files = repo.GetAll<File>().Where(x => !x.IsContainer);

                // Create map of files based upon Layout properties
                var fileMap = TemplateHelper.CreateLayoutFileMap(files);

                // Choose the tree depth to render
                var currentKey = "None";
                if (parentId != RootNodeId)
                {
                    var parentFile = files.SingleOrDefault(x => x.Id == parentId);
                    currentKey = Server.MapPath(parentFile.RootRelativePath);
                }

                // Construct tree nodes
                var treeNodes = fileMap[currentKey].Select(f => new
                {
                    treeNode = CreateTreeNode(
                        f.Id,
                        queryStrings,
                        f.GetFileNameForDisplay(),
                        Url.GetEditorUrl(f.Id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings),
                        fileMap.ContainsKey(Server.MapPath(f.RootRelativePath)),
                        f.Name.ToLower().StartsWith("_viewstart.") 
                            ? "tree-viewstart-template" 
                            : f.Name.StartsWith("_") || f.RelationProxies.GetChildRelations(FixedRelationTypes.DefaultRelationType).Any()
                                ? "tree-master-template" 
                                : "tree-template"
                    ),
                    entity = f
                });

                foreach (var file in treeNodes)
                {
                    // Set menu items
                    file.treeNode.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
                    file.treeNode.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");
                    file.treeNode.AddMenuItem<Reload>();

                    // Set tree expand url
                    if (file.treeNode.HasChildren)
                    {
                        file.treeNode.JsonUrl = Url.GetTreeUrl(GetType(), file.treeNode.HiveId, queryStrings);
                    }

                    // Add to node collection
                    NodeCollection.Add(file.treeNode);
                }
            }

            return UmbracoTree();   
        }
    }
}
