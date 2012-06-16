using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MediaTypeTreeControllerId, "Media types")]
    [UmbracoTree]
    public class MediaTypeTreeController : DocumentTypeTreeController
    {
        public MediaTypeTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.MediaRootSchema; }
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MediaTypeEditorControllerId); }
        }

    }
}