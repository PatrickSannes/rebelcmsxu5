using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.LanguageTreeControllerId, "Languages")]
    [UmbracoTree]
    public class LanguageTreeController : SupportsEditorTreeController
    {
        public LanguageTreeController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.LanguageEditorControllerId); }
        }
        
        /// <summary>
        /// Returns a unique tree root node id
        /// </summary>
        /// <remarks>
        /// We are assigning a static unique id to this tree in order to facilitate tree node syncing
        /// </remarks>
        protected override HiveId RootNodeId
        {
            get { return new HiveId(FixedSchemaTypes.SystemRoot, null, new HiveIdValue(new Guid(CorePluginConstants.LanguageTreeRootNodeId))); }
        }

        /// <summary>
        /// Customize root node
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var n = base.CreateRootNode(queryStrings);
            AddMenuItemsToRootNode(n, queryStrings);
            return n;
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            if (parentId == RootNodeId)
            {
                foreach (var node in BackOfficeRequestContext.Application.Settings.Languages
                    .OrderBy(x => x.Name)
                    .Select(x =>
                            CreateTreeNode(
                                new HiveId(x.IsoCode),
                                queryStrings,
                                x.Name,
                                Url.GetEditorUrl("Edit", new HiveId(x.IsoCode), EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings),
                                "Edit",
                                false,
                                "tree-language")))
                {
                    node.AddEditorMenuItem<UnsecuredDelete>(this, "deleteUrl", "Delete");

                    NodeCollection.Add(node);
                }
            }
            else
            {
                throw new NotImplementedException("The Languages tree does not support more than 1 level");
            }

            return UmbracoTree();
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
                n.AddEditorMenuItem<CreateItem>(this, "createUrl", "Create");
                n.AddMenuItem<Reload>();
            }

        }
    }
}
