using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Model.BackOffice;
using Umbraco.Cms.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.PropertyEditors.Tags;

namespace Umbraco.Cms.Web.PropertyEditors.CodeEditor
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.CodeEditor.Views.CodeEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class CodeEditorModel : EditorModel<CodeEditorPreValueModel>
    {
        public CodeEditorModel(CodeEditorPreValueModel preValueModel)
            : base(preValueModel)
        { }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [ShowLabel(false)]
        public string Value { get; set; }

    
    }
}
