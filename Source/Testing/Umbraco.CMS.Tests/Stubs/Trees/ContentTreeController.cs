using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Tests.Cms.Stubs.Trees
{
    /// <summary>
    /// A tree to test duplicate controller names as plugins
    /// </summary>
    [Tree("1C841BC7-915C-4362-B844-A27A0A3F4399", "TreeController")]
    internal class ContentTreeController : TreeController
    {
        public ContentTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return UmbracoTree();
        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.ContentVirtualRoot; }
        }
    }
}