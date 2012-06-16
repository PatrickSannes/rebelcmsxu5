using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web.ParameterEditors.ContentPicker
{
    [ParameterEditor("BF7654FE-8463-4FC5-A807-6DBDEC0B696B", "ContentPicker", "Content Picker", CorePluginConstants.TreeNodePickerPropertyEditorId)]
    public class ContentPickerEditor : ParameterEditor
    {
        public ContentPickerEditor(IPropertyEditorFactory propertyEditorFactory)
            : base(propertyEditorFactory)
        { }

        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='TreeId'><![CDATA[" + CorePluginConstants.ContentTreeControllerId + @"]]></preValue>
                    </preValues>";
            }
        }
    }
}
