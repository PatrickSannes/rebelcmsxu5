using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Trees
{
    /// <summary>
    /// Tree controller to render out the javascript files
    /// </summary>
    [Tree(CorePluginConstants.ScriptTreeControllerId, "Scripts")]
    [UmbracoTree]
    public class ScriptTreeController : AbstractFileSystemTreeController
    {
        public ScriptTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.ScriptEditorControllerId); }
        }

        protected override string HiveUriRouteMatch
        {
            get { return "storage://scripts"; }
        }
    }
}
