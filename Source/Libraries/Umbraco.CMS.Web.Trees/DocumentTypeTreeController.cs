using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.DocumentTypeTreeControllerId, "Document types")]
    [UmbracoTree]
    public class DocumentTypeTreeController : SupportsEditorTreeController
    {

        public DocumentTypeTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// Returns the SystemRoot node as the start node for this tree
        /// </summary>
        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.ContentRootSchema; }
        }


        /// <summary>
        /// Defines the document type editor
        /// </summary>
        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.DocumentTypeEditorControllerId); }
        }

        /// <summary>
        /// Customize root node
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var n = base.CreateRootNode(queryStrings);
            //add some menu items to the created root node
            n.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
            n.AddMenuItem<Reload>();
            return n;
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
            {
                var items = uow.Repositories.Schemas.GetEntityByRelationType<EntitySchema>(FixedRelationTypes.DefaultRelationType, parentId)
                    //don't include the 'special' schemas
                    .Where(x => !x.Id.IsSystem())
                    .OrderBy(x => x.Name.Value);

                foreach (var treeNode in
                    items
                        .Select(dt =>
                                CreateTreeNode(
                                    dt.Id,
                                    queryStrings,
                                    dt.Name,
                                    Url.GetEditorUrl(dt.Id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings),
                                    dt.RelationProxies.GetChildRelations(FixedRelationTypes.DefaultRelationType).Any(),
                                    "tree-data-type")))
                {

                    //add the menu items
                    treeNode.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
                    treeNode.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");
                    treeNode.AddMenuItem<Reload>();

                    NodeCollection.Add(treeNode);
                }
            }

            return UmbracoTree();
        }




    }
}
