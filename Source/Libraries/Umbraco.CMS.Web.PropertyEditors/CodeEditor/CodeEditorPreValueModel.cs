using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.EmbeddedViewEngine;

namespace Umbraco.Cms.Web.PropertyEditors.CodeEditor
{
    public class CodeEditorPreValueModel : PreValueModel
    {
        public CodeEditorPreValueModel()
            : this(string.Empty)
        { }

        public CodeEditorPreValueModel(string preValues)
            : base(preValues)
        { }

        /// <summary>
        /// The language of code to display
        /// </summary>
        [UIHint("EnumDropDownList")]
        public CodeEditorLanguage CodeLanguage { get; set; }
    }
}
