using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;
using Umbraco.Hive;

using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Trees
{
    /// <summary>
    /// Abstract tree used to render file/folder structures
    /// </summary>
    public abstract class AbstractFileSystemTreeController : SupportsEditorTreeController
    {
        protected AbstractFileSystemTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        protected ReadonlyGroupUnitFactory<IFileStore> Hive
        {
            get
            {
                return BackOfficeRequestContext
                    .Application
                    .Hive
                    .GetReader<IFileStore>(new Uri(HiveUriRouteMatch));
            }
        }

        protected virtual string GetDashboardUrl()
        {
            return Url.GetCurrentDashboardUrl();
        }

        protected abstract string HiveUriRouteMatch { get; }

        protected sealed override HiveId RootNodeId
        {
            get
            {
                //BUG: we need to lookup the root Id so its based on what Hive returns since that doesn't actually match what the id 'should' be
                return Hive.GetRootNodeId();                
            }
        }

        /// <summary>
        /// Adds the menu items to the node and sets the icon
        /// </summary>
        /// <param name="n"></param>
        /// <param name="queryStrings"></param>
        protected virtual void CustomizeFileNode(TreeNode n, FormCollection queryStrings)
        {
            n.HasChildren = false;
            n.Icon = "tree-doc";
            //add the menu items
            n.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");
        }

        /// <summary>
        /// Adds the menu items to the node and sets the icon
        /// </summary>
        /// <param name="n"></param>
        /// <param name="queryStrings"></param>
        protected virtual void CustomizeFolderNode(TreeNode n, FormCollection queryStrings)
        {
            //add the menu items
            n.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
            n.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");
            n.AddMenuItem<Reload>();
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
            using (var uow = Hive.CreateReadonly())
            {
                var repo = uow.Repositories;
                IEnumerable<File> files;

                //First node - get the root entities
                if (parentId == RootNodeId)
                {
                    files = repo.GetAll<File>();
                }
                else
                {
                    //get entities from the current depth
                    var fileRelations = repo.GetLazyChildRelations(parentId, FixedRelationTypes.DefaultRelationType);
                    files = fileRelations.Select(x => x.Destination).OfType<File>();
                }

                var treeNodes = files.Select(f => new
                {
                    treeNode = CreateTreeNode(
                        f.Id,
                        queryStrings,
                        f.GetFileNameForDisplay(),
                        f.IsContainer
                            ? GetDashboardUrl()
                            : Url.GetEditorUrl(f.Id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings)
                ),
                    entity = f
                }
                    );
                foreach (var file in treeNodes)
                {
                    if (file.entity.IsContainer)
                    {
                        //If container then see if it has children
                        file.treeNode.HasChildren = repo.GetChildRelations(file.entity.Id, FixedRelationTypes.DefaultRelationType).Any();
                        CustomizeFolderNode(file.treeNode, queryStrings);
                    }
                    else
                    {
                        CustomizeFileNode(file.treeNode, queryStrings);
                    }
                    NodeCollection.Add(file.treeNode);
                }
            }

            return UmbracoTree();
        }
    }
}