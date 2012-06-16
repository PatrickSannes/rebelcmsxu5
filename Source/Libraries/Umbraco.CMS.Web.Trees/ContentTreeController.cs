using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Security;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;
using Umbraco.Framework.Security;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.ContentTreeControllerId, "Content")]
    [UmbracoTree]
    public class ContentTreeController : RecycleBinTreeController
        //, ISearchableTree
    {

        public ContentTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        [MenuItemsPermissionFilter]
        public override UmbracoTreeResult Index(HiveId id, FormCollection querystrings)
        {
            return base.Index(id, querystrings);
        }
        
        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.ContentEditorControllerId); }
        }
        public override HiveId RecycleBinId
        {
            get { return FixedHiveIds.ContentRecylceBin; }
        }

        protected override HiveId RootNodeId
        {
            get
            {
                var rootNodeId = FixedHiveIds.ContentVirtualRoot;

                if (Request.IsAuthenticated && HttpContext.User.Identity is UmbracoBackOfficeIdentity)
                {
                    var currentIdentity = (UmbracoBackOfficeIdentity)HttpContext.User.Identity;
                    if (currentIdentity.StartContentNode != HiveId.Empty)
                        rootNodeId = currentIdentity.StartContentNode;
                }

                return rootNodeId;
            }
        }

        protected virtual HiveId ActualRootNodeId
        {
            get
            {
                return FixedHiveIds.ContentVirtualRoot;
            }
        }

        //#region ISearchableTree Members

        //public virtual TreeSearchJsonResult Search(string searchText)
        //{
        //    //TODO: do the searching!

        //    //for now, i'm just going to return dummy data that i know is in demo data just to get the ui working
        //    var result = new TreeSearchJsonResult(new[]
        //                {
        //                    new SearchResultItem { Id = new HiveId(1050).ToString(), Rank = 0, Title = "Go further", Description="this is a node" },
        //                    new SearchResultItem { Id = new HiveId(1060).ToString(), Rank = 0, Title = "Functionality", Description="this is a node" }
        //                });

        //    return result;
        //}

        //#endregion

        /// <summary>
        /// Customize the root node
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            if(RootNodeId == ActualRootNodeId)
            {
                var n = base.CreateRootNode(queryStrings);
                AddMenuItemsToRootNode(n, queryStrings);
                return n;
            }
            else
            {
                var hiveProvider = GetHiveProvider(RootNodeId, queryStrings);

                using (var securityUow = hiveProvider.CreateReadonly<ISecurityStore>())
                using (var uow = hiveProvider.CreateReadonly<IContentStore>())
                {
                    //Get the items and ensure Browse permissions are applied
                    var childIds = new[] { RootNodeId };

                    using (DisposableTimer.TraceDuration<ContentTreeController>("FilterWithPermissions", "FilterWithPermissions"))
                    {
                        childIds = childIds.FilterWithPermissions(
                                BackOfficeRequestContext.Application.Security,
                                ((UmbracoBackOfficeIdentity)User.Identity).Id,
                                uow,
                                securityUow,
                                new ViewPermission().Id
                                ).ToArray();
                    }

                    if(childIds.Length == 1)
                    {
                        var node = uow.Repositories.Get<TypedEntity>(childIds[0]);
                        var treeNode = ConvertEntityToTreeNode(node, RootNodeId, queryStrings, uow);

                        //add the tree id to the root
                        treeNode.AdditionalData.Add("treeId", TreeId.ToString("N"));
                        treeNode.HasChildren = true; // Force true so that recycle bin is displayed

                        //add the tree-root css class
                        treeNode.Style.AddCustom("tree-root");

                        //node.AdditionalData.Add("id", node.HiveId.ToString());
                        //node.AdditionalData.Add("title", node.Title);

                        //check if the tree is searchable and add that to the meta data as well
                        if (this is ISearchableTree)
                        {
                            treeNode.AdditionalData.Add("searchable", "true");
                        }

                        AddMenuItemsToNode(treeNode, queryStrings);

                        return treeNode;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the Hive provider to be used to query for child nodes
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override ReadonlyGroupUnitFactory GetHiveProvider(HiveId parentId, FormCollection queryStrings)
        {
            return BackOfficeRequestContext.Application.Hive.GetReader(
                parentId == FixedHiveIds.ContentVirtualRoot ? new Uri("content://") : parentId.ToUri());
        }

        /// <summary>
        /// Get the child tree nodes for the id specified
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override IEnumerable<TreeNode> GetChildTreeNodes(HiveId parentId, FormCollection queryStrings)
        {
            var hiveProvider = GetHiveProvider(parentId, queryStrings);

            using (var securityUow = hiveProvider.CreateReadonly<ISecurityStore>())
            using (var uow = hiveProvider.CreateReadonly<IContentStore>())
            {
                //Get the items and ensure Browse permissions are applied
                var childIds = GetChildContentIds(parentId, uow);

                using (DisposableTimer.TraceDuration<ContentTreeController>("FilterWithPermissions", "FilterWithPermissions"))
                {
                    childIds = childIds.FilterWithPermissions(
                            BackOfficeRequestContext.Application.Security,
                            ((UmbracoBackOfficeIdentity) User.Identity).Id,
                            uow,
                            securityUow,
                            new ViewPermission().Id
                            ).ToArray();
                }

                IEnumerable<TypedEntity> children = null;

                using (DisposableTimer.TraceDuration<ContentTreeController>("Get childIds", "Get childIds"))
                {
                   children = uow.Repositories.Get<TypedEntity>(true, childIds);
                }

                //var items = uow.Repositories
                //    .GetEntityByRelationType<TypedEntity>(FixedRelationTypes.ContentTreeRelationType, parentId)
                //    .FilterWithPermissions<TypedEntity>(
                //        new BrowsePermission().Id, 
                //        BackOfficeRequestContext.Application.Security, 
                //        ((UmbracoBackOfficeIdentity)User.Identity).Id);

                //lookup all the items, conver to a custom object of the tree node & entity, then populate the 
                //icon based on the EntitySchema of the item.
                return children.Select(x => ConvertEntityToTreeNode(x, parentId, queryStrings, uow))
                    .Where(x => x != null);
            }
        }

        /// <summary>
        /// Gets the child content ids for this tree given a <paramref name="parentId"/>.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="uow">The uow.</param>
        /// <returns></returns>
        protected virtual HiveId[] GetChildContentIds(HiveId parentId, IReadonlyGroupUnit<IContentStore> uow)
        {
            var childIds = uow.Repositories.GetChildRelations(parentId, FixedRelationTypes.DefaultRelationType)
                .Select(x => x.DestinationId).ToArray();
            return childIds;
        }

        protected override void AddMenuItemsToNodeInRecycleBin(TreeNode n, FormCollection queryStrings)
        {
            if (!queryStrings.GetValue<bool>(TreeQueryStringParameters.DialogMode))
            {
                //TODO: implement security restrictions here!
                //add the menu items
                n.AddEditorMenuItem<Move>(this, "moveUrl", "Move");
                n.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");
                n.AddMenuItem<Reload>();
            }
        }

        /// <summary>
        /// Adds menu items to each node
        /// </summary>
        /// <param name="n"></param>
        /// <param name="queryStrings"></param>
        protected override void AddMenuItemsToNode(TreeNode n, FormCollection queryStrings)
        {
            if (!queryStrings.GetValue<bool>(TreeQueryStringParameters.DialogMode))
            {
                //add the menu items
                n.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
                //we need to add the recycle bin Id to the metadata so that the UI knows to refresh the bin on delete
                n.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete", new Dictionary<string, object> {{"recycleBinId", RecycleBinId.ToJsonObject()}});
                n.AddEditorMenuItem<Move>(this, "moveUrl", "Move");
                n.AddEditorMenuItem<Copy>(this, "copyUrl", "Copy");
                n.AddEditorMenuItem<Sort>(this, "sortUrl", "Sort");
                n.AddEditorMenuItem<Rollback>(this, "rollbackUrl", "Rollback");
                n.AddEditorMenuItem<Publish>(this, "publishUrl", "Publish");
                n.AddEditorMenuItem<Permissions>(this, "permissionsUrl", "Permissions");
                //n.AddMenuItem<Protect>();
                //n.AddMenuItem<Audit>();
                //n.AddMenuItem<Notify>();
                //nodeData.treeNode.AddMenuItem<SendTranslate>(MenuItems);
                n.AddEditorMenuItem<Hostname>(this, "hostnameUrl", "Hostname");
                n.AddEditorMenuItem<Language>(this, "languageUrl", "Language");
                n.AddMenuItem<Reload>();
            }
        }

        /// <summary>
        /// Adds menu items to root node
        /// </summary>
        /// <param name="n"></param>
        /// <param name="queryStrings"></param>
        protected virtual void AddMenuItemsToRootNode(TreeNode n, FormCollection queryStrings)
        {
            if (!queryStrings.GetValue<bool>(TreeQueryStringParameters.DialogMode))
            {
                //add some menu items to the created root node
                n.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
                n.AddEditorMenuItem<Sort>(this, "sortUrl", "Sort");
                n.AddMenuItem<Reload>();
            }

        }

        protected virtual TreeNode ConvertEntityToTreeNode(TypedEntity entity, HiveId parentId, FormCollection queryStrings, IReadonlyGroupUnit<IContentStore> uow)
        {
            var treeNode = CreateTreeNode(entity.Id,
                queryStrings,
                GetTreeNodeAlias(entity),
                GetEditorUrl(entity.Id, queryStrings),
                uow.Repositories.GetChildRelations(entity.Id, FixedRelationTypes.DefaultRelationType).Any());

            var icon = entity.EntitySchema.GetXmlConfigProperty("icon");
            if (!string.IsNullOrEmpty(icon))
            {
                if (icon.Contains("."))
                {
                    //need to get the path
                    //TODO: Load this in from the right hive provider --Aaron
                    treeNode.Icon =
                        Url.Content(
                            BackOfficeRequestContext.Application.Settings.UmbracoFolders.
                                DocTypeIconFolder + "/" + icon);
                }
                else
                {
                    treeNode.Icon = icon;
                }
            }

            //TODO: This is going to make a few calls to the db for each node, is there a better way?
            var snapshot = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(entity.Id);
            if (!snapshot.IsPublished())
                treeNode.Style.DimNode();
            else if (snapshot.IsPublishPending())
                treeNode.Style.HighlightNode();

            //now, check if we're in the recycle bin, if so we need to filter the nodes based on the user's start node
            //and where the nodes originated from.
            if (parentId == RecycleBinId)
            {
                //TODO: Hopefully there's a better way of looking all this up as this will make quite a few queries as well...

                var recycledRelation = uow.Repositories.GetParentRelations(entity, FixedRelationTypes.RecycledRelationType);
                var allowed = true;
                foreach (var r in recycledRelation)
                {
                    //get the path of the original parent
                    var path = uow.Repositories.GetEntityPath(r.SourceId, FixedRelationTypes.DefaultRelationType).ToList();
                    //append the current node to the path to create the full original path
                    path.Add(r.DestinationId);

                    //BUG: This currently will always be false because NH is not returning the correct Ids for 'system' ids.
                    // so we've commented out the return null below until fixed

                    //now we can determine if this node can be viewed
                    if (!path.Contains(RootNodeId, new HiveIdComparer(true)))
                    {
                        allowed = false;
                    }
                }
                //if (!allowed)
                //    return null;
            }

            //TODO: Check if node has been secured

            // Add additional data
            foreach (var key in queryStrings.AllKeys)
            {
                treeNode.AdditionalData.Add(key, queryStrings[key]);
            }

            return treeNode;
        }

        protected virtual string GetTreeNodeAlias(TypedEntity x)
        {
            var toReturn = string.Empty;
            var nodeName = x.Attributes.FirstOrDefault(y => y.AttributeDefinition.Alias == NodeNameAttributeDefinition.AliasValue);
            if (nodeName != null) toReturn = nodeName.GetValueAsString("Name");
            if (string.IsNullOrEmpty(toReturn))
            {
                if (x.EntitySchema != null)
                {
                    toReturn = "[Unnamed {0}]".InvariantFormat(string.IsNullOrEmpty(x.EntitySchema.Name)
                                                                   ? x.EntitySchema.Alias
                                                                   : (string)x.EntitySchema.Name);
                }
                else
                {
                    toReturn = "[Unnamed, untyped]";
                }

            }
            return toReturn;
        }

        /// <summary>
        /// Return the editor URL for the currrent node depending on the data found in the query strings
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected virtual string GetEditorUrl(HiveId id, FormCollection queryStrings)
        {
            var isDialog = queryStrings.GetValue<bool>(TreeQueryStringParameters.DialogMode);
            return queryStrings.HasKey(TreeQueryStringParameters.OnNodeClick) //has a node click handler?
                       ? queryStrings.Get(TreeQueryStringParameters.OnNodeClick) //return node click handler
                       : isDialog //is in dialog mode without a click handler ?
                             ? string.Empty //return empty string, otherwise, return an editor URL:
                             : Url.GetEditorUrl(id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings);
        }

    }
}
