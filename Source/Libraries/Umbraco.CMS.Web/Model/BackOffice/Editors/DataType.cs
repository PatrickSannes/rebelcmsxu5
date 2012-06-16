using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;
using PropertyEditorModel = Umbraco.Cms.Web.Model.BackOffice.PropertyEditors.EditorModel;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents a data type which refrences a PropertyEditor
    /// </summary>
    /// <remarks>
    /// This does not represent the DataType editor model
    /// </remarks>
    public class DataType : EntityUIModel
    {
        /// <summary>
        /// Creates a data type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="alias"></param>
        /// <param name="propertyEditor">The PropertyEditor associated with the DataType</param>
        /// <param name="preValues">The pre values stored in the repository for the DataType</param>
        public DataType(HiveId id, string name, string alias, dynamic propertyEditor, string preValues)
        {
            Mandate.ParameterNotEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNullOrEmpty(alias, "alias");
            //Mandate.ParameterNotNull(propertyEditor, "propertyEditor");

            Name = name;
            Alias = alias;
            InternalPropertyEditor = propertyEditor;
            Prevalues = preValues;
        }

        /// <summary>
        /// Creates a new data type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="alias"></param>
        /// <param name="propertyEditor">The PropertyEditor associated with the DataType</param>
        public DataType(string name, string alias, dynamic propertyEditor)
        {
            Name = name;
            Alias = alias;
            InternalPropertyEditor = propertyEditor;
        }
        
        /// <summary>
        /// Returns the PropertyEditor associated with this DataType as a <code>dynamic</code> object
        /// </summary>
        public dynamic PropertyEditor { get { return InternalPropertyEditor; } }

        /// <summary>
        /// Alias gets mapped from the persistence models, alias is required as a unique identifier for each DataType however
        /// this is a readonly property since it gets generated automatically from the Name property.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Returns the PropertyEditor associated with this DataType
        /// </summary>
        protected internal dynamic InternalPropertyEditor { get; set; }
        
        /// <summary>
        /// The serialized pre values associated with this data type
        /// </summary>
        protected internal string Prevalues { get; set; }
       
        /// <summary>
        /// Returns the PropertyEditor model used to render out the content editor
        /// for this data type and ensures that the prevalues stored in the repository against both the 
        /// data type and document type property are passed into the editor so it can be configured.
        /// </summary>
        /// <returns></returns>
        public PropertyEditorModel GetEditorModel()
        {
            var preValueModel = GetPreValueModel();            

            //return the editor model with the merged pre-values
            return InternalPropertyEditor != null ? InternalPropertyEditor.CreateEditorModel((dynamic)preValueModel) : null;
        }

        /// <summary>
        /// Represents the pre-value model used to render the pre value editor for this DataType
        /// </summary>
        public PreValueModel GetPreValueModel()
        {
            if (InternalPropertyEditor != null)
            {
                var preValModel = InternalPropertyEditor.CreatePreValueEditorModel();
                preValModel.SetModelValues(Prevalues);
                return preValModel;
            }
            return null;
        }

    }
}
