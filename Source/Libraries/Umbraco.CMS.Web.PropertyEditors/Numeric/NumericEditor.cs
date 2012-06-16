using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.PropertyEditors.Numeric;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.Numeric.Resources.NumericPropertyEditor.js", "application/x-javascript")]

namespace Umbraco.Cms.Web.PropertyEditors.Numeric
{
    [PropertyEditor(CorePluginConstants.NumericPropertyEditorId, "Numeric", "Numeric", IsParameterEditor = true)]
    public class NumericEditor : PropertyEditor<NumericEditorModel, NumericPreValueModel>
    {
        public override NumericEditorModel CreateEditorModel(NumericPreValueModel preValues)
        {
            return new NumericEditorModel(preValues);
        }

        public override NumericPreValueModel CreatePreValueEditorModel()
        {
            return new NumericPreValueModel();
        }
    }
}
