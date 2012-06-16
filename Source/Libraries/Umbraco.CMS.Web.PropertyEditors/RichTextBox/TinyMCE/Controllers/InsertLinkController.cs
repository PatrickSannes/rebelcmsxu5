using System.Web.Mvc;
using System.Web.UI;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.Editors;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertLink.InsertLink.js", "application/x-javascript")]

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("C05B6ACC-C08E-497B-9078-5B23F67C0823")]
    [UmbracoEditor]
    public class InsertLinkController : AbstractEditorController
    {
        public InsertLinkController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        #region Actions

        /// <summary>
        /// Inserts the link.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InsertLink()
        {
            return View(EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertLink.InsertLink.cshtml, Umbraco.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
