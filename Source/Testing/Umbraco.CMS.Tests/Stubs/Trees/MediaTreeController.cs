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
    [Tree("1296D4BA-459F-4302-B35C-70B47E6F8117", "Content")]
    internal class MediaTreeController : TreeController
    {
        public MediaTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return UmbracoTree();

        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.MediaVirtualRoot; }
        }
    }
}