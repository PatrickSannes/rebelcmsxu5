namespace Umbraco.Cms.Web.Model.BackOffice.ParameterEditors
{
    public abstract class ParameterEditor<TParamEditorModel> : AbstractParameterEditor
        where TParamEditorModel : EditorModel, new()
    {
        protected ParameterEditor(IPropertyEditorFactory propertyEditorFactory) 
            : base(propertyEditorFactory)
        { }

        public virtual TParamEditorModel CreateEditorModel()
        {
            return new TParamEditorModel {PropertyEditorModel = PropertyEditorModel};
        }
    }

    public abstract class ParameterEditor : ParameterEditor<EditorModel>
    {
        protected ParameterEditor(IPropertyEditorFactory propertyEditorFactory)
            : base(propertyEditorFactory)
        { }
    }
}
