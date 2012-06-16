using System.Web.Mvc;
using System.Web.UI;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.Editors;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertAnchor.InsertAnchor.js", "application/x-javascript")]

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("FD1C3D06-2044-4145-9EBF-F41C04B89D61")]
    [UmbracoEditor]
    public class InsertAnchorController : AbstractEditorController
    {
        public InsertAnchorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        #region Actions

        /// <summary>
        /// Action to render the insert anchor dialog.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InsertAnchor()
        {
            return View(EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertAnchor.InsertAnchor.cshtml, Umbraco.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
