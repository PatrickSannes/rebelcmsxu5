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
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.DictionaryTreeControllerId, "Dictionary")]
    [UmbracoTree]
    public class DictionaryTreeController : ContentTreeController
    {
        public DictionaryTreeController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.DictionaryEditorControllerId); }
        }

        protected override HiveId RootNodeId
        {
            get { return new HiveId(new Uri("dictionary://"), "", FixedHiveIds.DictionaryVirtualRoot.Value); }
        }

        protected override HiveId ActualRootNodeId
        {
            get { return RootNodeId; }
        }

        public override HiveId RecycleBinId
        {
            get { return HiveId.Empty; }
        }

        protected override ReadonlyGroupUnitFactory GetHiveProvider(HiveId parentId, FormCollection queryStrings)
        {
            //we need to get the Hive Map based on Id                        
            return BackOfficeRequestContext.Application.Hive.GetReader(
                parentId == FixedHiveIds.DictionaryVirtualRoot ? new Uri("dictionary://") : parentId.ToUri());
        }

        protected override void AddMenuItemsToNode(TreeNode n, FormCollection queryStrings)
        {
            base.AddMenuItemsToNode(n, queryStrings);

            n.MenuActions.RemoveAll(x => x.Metadata.Id == BackOfficeRequestContext.RegisteredComponents.MenuItems.GetItem<Hostname>().Metadata.Id);
        }
    }
}
