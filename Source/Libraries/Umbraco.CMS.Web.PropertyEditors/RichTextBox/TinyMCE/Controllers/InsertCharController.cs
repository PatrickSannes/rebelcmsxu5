using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.Editors;

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("70AEA379-A639-43DE-81C9-FE5C5B275213")]
    [UmbracoEditor]
    public class InsertCharController : AbstractEditorController
    {
        public InsertCharController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        #region Actions

        /// <summary>
        /// Action to render the insert char dialog.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InsertChar()
        {
            return View(EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertChar.InsertChar.cshtml, Umbraco.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
