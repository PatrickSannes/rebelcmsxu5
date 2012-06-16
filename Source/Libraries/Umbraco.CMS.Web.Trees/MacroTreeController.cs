using System;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MacroTreeControllerId, "Macros")]
    [UmbracoTree]
    public class MacroTreeController : AbstractFileSystemTreeController
    {

        public MacroTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {

        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MacroEditorControllerId); }
        }


        protected override string HiveUriRouteMatch
        {
            get { return "storage://macros"; }
        }

        protected override void CustomizeFileNode(TreeNode n, FormCollection queryStrings)
        {            
            base.CustomizeFileNode(n, queryStrings);
            n.Icon = "tree-developer-macro";
        }
    }
}