namespace Umbraco.Cms.Web.Model.BackOffice.PropertyEditors
{
    /// <summary>
    /// Abstract class that all Property editors should inherit from
    /// </summary>
    /// <typeparam name="TEditorModel"></typeparam>
    /// <typeparam name="TPreValueModel"></typeparam>
    public abstract class PropertyEditor<TEditorModel, TPreValueModel> : PropertyEditor
        where TEditorModel : EditorModel
        where TPreValueModel : PreValueModel        
    {

        /// <summary>
        /// Returns the editor model to be used for the property editor
        /// </summary>
        /// <returns></returns>
        public abstract TEditorModel CreateEditorModel(TPreValueModel preValues);

        /// <summary>
        /// Returns the editor model to be used for the prevalue editor
        /// </summary>
        /// <returns></returns>
        public abstract TPreValueModel CreatePreValueEditorModel();

    }

    /// <summary>
    /// Abstract class that Property editors should inherit from that don't require a pre-value editor
    /// </summary>
    /// <typeparam name="TEditorModel"></typeparam>
    public abstract class PropertyEditor<TEditorModel> : PropertyEditor<TEditorModel, BlankPreValueModel>
        where TEditorModel : EditorModel
    {
        public override BlankPreValueModel CreatePreValueEditorModel()
        {
            return new BlankPreValueModel();
        }

        public sealed override TEditorModel CreateEditorModel(BlankPreValueModel preValues)
        {
            return CreateEditorModel();
        }

        public abstract TEditorModel CreateEditorModel();
    }
}
