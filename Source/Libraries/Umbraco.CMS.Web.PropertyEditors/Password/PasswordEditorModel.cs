using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.Password
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.Password.Views.PasswordEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class PasswordEditorModel : EditorModel<PasswordPreValueModel>, IValidatableObject
    {
        public PasswordEditorModel(PasswordPreValueModel preValues) 
            : base(preValues)
        { }

        [ShowLabel(false)]
        public string Value { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && string.IsNullOrWhiteSpace(Value))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
