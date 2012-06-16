using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Tests.Extensions.Stubs.PropertyEditors
{
    [PropertyEditor("746A8F96-1BDF-4D79-A094-D0E14794B396", "RegexPropertyEditor", "Regex Property")]
    public class RegexPropertyEditor : PropertyEditor<RegexPropertyEditor.RegexEditorModel>
    {
        public override RegexEditorModel CreateEditorModel()
        {
            return new RegexEditorModel();
        }

        public class RegexEditorModel : EditorModel
        {
            [RegularExpression(@"\d+")]
            public string Value { get; set; }

        }
    }
}
