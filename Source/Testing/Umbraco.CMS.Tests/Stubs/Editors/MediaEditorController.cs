using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework;

namespace Umbraco.Tests.Cms.Stubs.Editors
{
    /// <summary>
    /// An editor for testing routes when there are multiple editors with the same name (as plugins)
    /// </summary>
    [Editor("1F614F25-F837-4889-9646-CE0A5E8B16F8")]
    internal class MediaEditorController : StandardEditorController
    {
        public MediaEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        public override ActionResult Edit(HiveId? id)
        {
            return null;
        }

        public ActionResult EditForm(HiveId? id)
        {
            return null;
        }
    }
}