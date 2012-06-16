using System;
using System.Linq;
using System.ComponentModel;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// The model representing a data type
    /// </summary>
    /// <remarks>
    /// We exclude the PreValueEditorModel from being bound because we manually bind it
    /// </remarks>
    [Bind(Exclude = "Alias,PreValueEditorModel")]
    public class DataTypeEditorModel : EditorModel, IModelBindAware
    {
        public DataTypeEditorModel(HiveId id)
            : this()
        {
            Mandate.ParameterNotEmpty(id, "id");
            Id = id;
        }

        public DataTypeEditorModel() 
        {
            PopulateUIElements();
        }

        /// <summary>
        /// Alias gets mapped from the persistence models, alias is required as a unique identifier for each DataType however
        /// this is a readonly property since it gets generated automatically from the Name property.
        /// </summary>
        [ReadOnly(true)]
        public string Alias { get { return _alias ?? (_alias = Name.ToUmbracoAlias()); } set { _alias = value; } }

        private string _alias;

        private bool? _hasEditableProps;
        /// <summary>
        /// A helper methods use to check if the pre value model has editable properties
        /// </summary>
        /// <returns></returns>
        public bool HasEditableProperties()
        {           
            if (!_hasEditableProps.HasValue)
            {                    
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => PreValueEditorModel, PreValueEditorModel.GetType());
                _hasEditableProps = metadata.Properties.Where(x => x.ShowForEdit).Any();                    
            }
            return _hasEditableProps.Value;            
        }

        /// <summary>
        /// The model representing the editor for the pre values
        /// </summary>
        [ReadOnly(true)]
        public PreValueModel PreValueEditorModel { get; set; }

        /// <summary>
        /// The Id of the selected PropertyEditor for this DataType
        /// </summary>          
        [Required(ErrorMessage = "A property editor must be selected")]
        [ScaffoldColumn(false)]
        [UIHint("PropertyEditorDropDown")]
        public Guid PropertyEditorId { get; set; }

        /// <summary>
        /// Bind the model
        /// </summary>
        /// <param name="modelUpdator"></param>
        public void BindModel(IModelUpdator modelUpdator)
        {
            Mandate.ParameterNotNull(modelUpdator, "updator");

            modelUpdator.BindModel(PreValueEditorModel, "PreValueEditorModel");
            modelUpdator.BindModel(this, string.Empty);
        }

        protected void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}
