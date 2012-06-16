using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Mvc.Metadata;

namespace Umbraco.Cms.Web.PropertyEditors.ColorSwatchPicker
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.ColorSwatchPicker.Views.ColorSwatchPickerEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class ColorSwatchPickerEditorModel : EditorModel<ColorSwatchPickerPreValueModel>, IValidatableObject
    {
        public ColorSwatchPickerEditorModel(ColorSwatchPickerPreValueModel preValueModel)
            : base(preValueModel)
        { }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [ShowLabel(false)]
        public string Value { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Value)) Value = string.Empty;

            if (PreValueModel.IsRequired && string.IsNullOrEmpty(Value))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
