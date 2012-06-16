using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.DateTimePicker
{
    [PropertyEditor("8D1DB331-B91E-49EF-9EEB-3F82AD3CBB46", "DateTimePicker", "DateTime Picker", IsParameterEditor = true)]
    public class DateTimePickerEditor : PropertyEditor<DateTimePickerEditorModel, DateTimePickerPreValueModel>
    {

        public override DateTimePickerEditorModel CreateEditorModel(DateTimePickerPreValueModel preValues)
        {
            return new DateTimePickerEditorModel(preValues);
        }

        public override DateTimePickerPreValueModel CreatePreValueEditorModel()
        {
            return new DateTimePickerPreValueModel();
        }

    }
}
