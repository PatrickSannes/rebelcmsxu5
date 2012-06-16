using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web.ParameterEditors.TrueFalse
{
    [ParameterEditor("20F44A55-4739-4A3F-89E4-FD82E53551AF", "TrueFalse", "True / False", CorePluginConstants.TrueFalsePropertyEditorId)]
    public class TrueFalseEditor : ParameterEditor
    {
        public TrueFalseEditor(IPropertyEditorFactory propertyEditorFactory)
            : base(propertyEditorFactory)
        { }
    }
}
