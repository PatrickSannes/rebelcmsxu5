using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Tests.Extensions.Stubs.PropertyEditors
{
    [PropertyEditor("7945F467-7BB6-41C0-91DC-A1D00C43E310", "PreValueRegexPropertyEditor", "PreValue Regex Property")]
    public class PreValueRegexPropertyEditor : PropertyEditor<PreValueRegexPropertyEditor.PreValueRegexEditorModel, PreValueRegexPropertyEditor.PreValueRegexPreValue>
    {
        public override PreValueRegexEditorModel CreateEditorModel(PreValueRegexPreValue preValues)
        {
            return new PreValueRegexEditorModel(new PreValueRegexPreValue());
        }

        public override PreValueRegexPreValue CreatePreValueEditorModel()
        {
            return new PreValueRegexPreValue();
        }

        public class PreValueRegexEditorModel : EditorModel<PreValueRegexPreValue>, IValidatableObject
        {
            public PreValueRegexEditorModel(PreValueRegexPreValue preValues)
                : base(preValues)
            {
            }

            public string Value { get; set; }
           

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!string.IsNullOrEmpty(PreValueModel.RegexValidationStatement) && !Regex.IsMatch(Value, PreValueModel.RegexValidationStatement))
                {
                    yield return new ValidationResult("Regex isn't valid!");
                }  
            }
        }

        public class PreValueRegexPreValue : PreValueModel
        {
            private string _regex;

            [AllowDocumentTypePropertyOverride]
            public string RegexValidationStatement
            {
                get
                {
                    return _regex ?? @"\d+";
                }
                set
                {
                    _regex = value;
                }
            }
        }
    }
}
