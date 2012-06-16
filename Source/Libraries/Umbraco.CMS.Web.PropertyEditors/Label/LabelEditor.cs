using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.Label
{
    /// <summary>
    /// The Label property editor
    /// </summary>
    [PropertyEditor("6A1F4266-E3A6-4BC1-8B79-81426CBAD9F1", "Label", "Label")]
    public class LabelEditor : PropertyEditor<LabelEditorModel>
    {
        public override LabelEditorModel CreateEditorModel()
        {
            return new LabelEditorModel();
        }
        
    }
}
