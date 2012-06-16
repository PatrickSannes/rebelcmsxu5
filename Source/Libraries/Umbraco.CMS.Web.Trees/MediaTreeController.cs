using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Security;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Cms.Web;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MediaTreeControllerId, "Media")]
    [UmbracoTree]
    public class MediaTreeController : ContentTreeController
    {

        public MediaTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MediaEditorControllerId); }
        }
        
        public override HiveId RecycleBinId
        {
            get { return FixedHiveIds.MediaRecylceBin; }
        }

        protected override HiveId RootNodeId
        {
            get
            {
                var rootNodeId = FixedHiveIds.MediaVirtualRoot;

                if (Request.IsAuthenticated && HttpContext.User.Identity is UmbracoBackOfficeIdentity)
                {
                    var currentIdentity = (UmbracoBackOfficeIdentity)HttpContext.User.Identity;
                    if(currentIdentity.StartMediaNode != HiveId.Empty)
                        rootNodeId = currentIdentity.StartMediaNode;
                }

                return rootNodeId;
            }
        }

        protected override HiveId ActualRootNodeId
        {
            get
            {
                return FixedHiveIds.MediaVirtualRoot;
            }
        }

        protected override ReadonlyGroupUnitFactory GetHiveProvider(HiveId parentId, FormCollection queryStrings)
        {
            //we need to get the Hive Map based on Id                        
            return BackOfficeRequestContext.Application.Hive.GetReader(
                parentId == FixedHiveIds.MediaVirtualRoot ? new Uri("media://") : parentId.ToUri());
        }

        protected override void AddMenuItemsToNode(TreeNode n, FormCollection queryStrings)
        {
            base.AddMenuItemsToNode(n, queryStrings);
            
            //TODO: now we just need to remove the ones we don't want from content
            n.MenuActions.RemoveAll(x =>
                                    x.Metadata.Id == BackOfficeRequestContext.RegisteredComponents.MenuItems.GetItem<Hostname>().Metadata.Id);
            n.MenuActions.RemoveAll(x =>
                                    x.Metadata.Id == BackOfficeRequestContext.RegisteredComponents.MenuItems.GetItem<Rollback>().Metadata.Id);

        }


    }
}
