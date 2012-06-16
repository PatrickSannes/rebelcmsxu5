using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using System.Xml.Linq;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker
{
    [ModelBinder(typeof(ListPickerEditorModelBinder))]
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.ListPicker.Views.ListPickerEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class ListPickerEditorModel : EditorModel<ListPickerPreValueModel>, IValidatableObject
    {
        public ListPickerEditorModel(ListPickerPreValueModel preValueModel)
            : base(preValueModel)
        { }

        [ShowLabel(false)]
        public IList<string> Value { get; set; }

        public override IDictionary<string, object> GetSerializedValue()
        {
            var vals = new Dictionary<string, object>();

            var count = 0;
            foreach (var item in Value)
            {
                vals.Add("val" + count, item);
                count++;
            }

            return vals;
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            Value = new List<string>();

            foreach (var item in serializedVal)
            {
                Value.Add(item.Value.ToString());
            }
        }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && (Value == null || Value.Count == 0))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
