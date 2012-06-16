using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.ListPicker.Resources.ListPickerPrevalueEditor.js", "application/x-javascript")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.ListPicker.Resources.ListPickerPrevalueEditor.css", "text/css")]

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker
{
    [PropertyEditor(CorePluginConstants.ListPickerPropertyEditorId, "ListPicker", "List Picker")]
    public class ListPickerEditor : PropertyEditor<ListPickerEditorModel, ListPickerPreValueModel>
    {
        public override ListPickerEditorModel CreateEditorModel(ListPickerPreValueModel preValues)
        {
            return new ListPickerEditorModel(preValues);
        }

        public override ListPickerPreValueModel CreatePreValueEditorModel()
        {
            return new ListPickerPreValueModel();
        }
    }
}
