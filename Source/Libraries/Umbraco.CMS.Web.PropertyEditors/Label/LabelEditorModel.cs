using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Mvc.Metadata;

namespace Umbraco.Cms.Web.PropertyEditors.Label
{
    /// <summary>
    /// The model for the label property editor
    /// </summary>
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.Label.Views.LabelEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class LabelEditorModel : EditorModel
    {
        /// <summary>
        /// The Label value
        /// </summary>
        [ShowLabel(false)]
        public string Value { get; set; }
    }
}
