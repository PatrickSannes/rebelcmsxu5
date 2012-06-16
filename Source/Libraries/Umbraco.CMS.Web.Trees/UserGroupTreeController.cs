using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;

using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;


namespace Umbraco.Cms.Web.Trees
{
    /// <summary>
    /// Tree controller to render out the data types
    /// </summary>
    [Tree(CorePluginConstants.UserGroupTreeControllerId, "User groups")]
    [UmbracoTree]
    public class UserGroupTreeController : SupportsEditorTreeController
    {

        public UserGroupTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// Defines the data type editor
        /// </summary>
        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.UserGroupEditorControllerId); }
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
            n.AddEditorMenuItem<CreateItem>(this, "createUrl", "Create");
            n.AddMenuItem<Reload>();
            return n;
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            //if its the first level
            if (parentId == RootNodeId)
            {
                var hive = BackOfficeRequestContext.Application.Hive.GetReader<ISecurityStore>(
                    //BUG: this check is only a work around because the way that Hive currently works cannot return the 'real' id of the entity. SD.
                    parentId == RootNodeId
                        ? new Uri("security://user-groups/")
                        : parentId.ToUri());

                Mandate.That(hive != null, x => new NotSupportedException("Could not find a hive provider for route: " + parentId.ToString(HiveIdFormatStyle.AsUri)));

                using (var uow = hive.CreateReadonly())
                {
                    //TODO: not sure how this is supposed to be casted to UserGroup
                    var items = uow.Repositories.GetEntityByRelationType<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                        .OrderBy(x => x.Name)
                        .ToArray();

                    foreach (var treeNode in items.Select(user =>
                                (TreeNode)CreateTreeNode(
                                    user.Id,
                                    queryStrings,
                                    user.Name,
                                    Url.GetEditorUrl(user.Id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings))))
                    {
                        treeNode.Icon = "tree-user-type";
                        treeNode.HasChildren = false;

                        //add the menu items
                        if (treeNode.Title != "Administrator")
                            treeNode.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");

                        NodeCollection.Add(treeNode);
                    }
                }
            }
            else
            {
                throw new NotSupportedException("The User Group tree does not support more than 1 level");
            }

            return UmbracoTree();
        }

        protected override HiveId RootNodeId
        {
            get
            {
                //BUG: Because of the current way that Hive is (30/09/2011), Hive cannot return the real Id representation,
                // so in the meantime to be consistent so that tree syncing works properly, we need to lookup the root id 
                // from hive and use it's returned value as the root node id. SD.
                using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<ISecurityStore>(new Uri("security://user-groups")))
                {
                    var root = uow.Repositories.Get<TypedEntity>(FixedHiveIds.UserGroupVirtualRoot);
                    return root.Id;
                }

                //return FixedHiveIds.UserGroupVirtualRoot;
            }
        }

    }
}
