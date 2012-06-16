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
    /// A tree for testing routes when there are multiple controllers with the same name as plugins
    /// </summary>
    [Tree("5C0BB383-83A9-4A9C-9B66-691C46B88C11", "Application")]
    internal class ApplicationTreeController : TreeController
    {
        public ApplicationTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return UmbracoTree();

        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.SystemRoot; }
        }
    }
}