using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.DateTimePicker
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.DateTimePicker.Views.DateTimePickerEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class DateTimePickerEditorModel : EditorModel<DateTimePickerPreValueModel>, IValidatableObject
    {
        public DateTimePickerEditorModel(DateTimePickerPreValueModel preValueModel)
            : base(preValueModel)
        {}

       
        [ShowLabel(false)]
        public DateTime? Value { get; set; }

        public bool HasValue
        {
            get
            {
                return Value != null;
            }
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            if (serializedVal.ContainsKey("Value") && serializedVal["Value"] != null && serializedVal["Value"] is DateTimeOffset)
                Value = ((DateTimeOffset)serializedVal["Value"]).DateTime;
            else
                base.SetModelValues(serializedVal);
            
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && Value == null)
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
