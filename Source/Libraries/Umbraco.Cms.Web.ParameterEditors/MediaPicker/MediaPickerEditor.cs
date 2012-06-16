using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web.ParameterEditors.MediaPicker
{
    [ParameterEditor("154D8737-F0A6-457F-B0B6-019541F2F2A3", "MediaPicker", "Media Picker", CorePluginConstants.TreeNodePickerPropertyEditorId)]
    public class MediaPickerEditor : ParameterEditor
    {
        public MediaPickerEditor(IPropertyEditorFactory propertyEditorFactory)
            : base(propertyEditorFactory)
        { }

        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='TreeId'><![CDATA[" + CorePluginConstants.MediaTreeControllerId + @"]]></preValue>
                    </preValues>";
            }
        }
    }
}
