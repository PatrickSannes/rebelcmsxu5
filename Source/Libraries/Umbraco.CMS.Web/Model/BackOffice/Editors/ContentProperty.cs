using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents a proeprty of a content/media model
    /// </summary>
    [Bind(Exclude = "Name,SortOrder,DocTypeProperty,TabId,Alias,DocTypePropertyId")]
    [DebuggerDisplay("{Alias} with DocTypeProperty.DataTypeId {DocTypeProperty.DataTypeId}")]
    public class ContentProperty : TimestampedModel, IMetadataAware
    {
        private readonly IDictionary<string, object> _propertyValue;
        private dynamic _propertyEditorModel;

        /// <summary>
        /// constructor for creating a property that has a multi-value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="docTypeProperty">The DocumentTypeProperty associated with this content property</param>
        /// <param name="propertyValue">The value of the property, used to set the values of the editor model</param>
        public ContentProperty(HiveId id,
            DocumentTypeProperty docTypeProperty,
            IDictionary<string, object> propertyValue)
        {            
            Mandate.ParameterNotEmpty(id, "id");
            Mandate.ParameterNotNull(docTypeProperty, "docTypeProperty");

            _propertyValue = propertyValue;
            Id = id;
            DocTypeProperty = docTypeProperty;
            DocTypePropertyId = docTypeProperty.Id;
        }


        /// <summary>
        /// Constructor for creating a property that has a single string value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="docTypeProperty"></param>
        /// <param name="propertyValue">The string value of the property</param>
        /// <remarks>
        /// This will create a new Dictionary object to put the value in with a key of "Value"
        /// </remarks>
        public ContentProperty(HiveId id,
            DocumentTypeProperty docTypeProperty,
            string propertyValue)
            : this(id, docTypeProperty, new Dictionary<string, object> { { "Value", propertyValue } })
        {
        }

        /// <summary>
        /// Constructor for creating a property that has a single int value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="docTypeProperty"></param>
        /// <param name="propertyValue">The int value of the property</param>
        /// <remarks>
        /// This will create a new Dictionary object to put the value in with a key of "Value"
        /// </remarks>
        public ContentProperty(HiveId id,
            DocumentTypeProperty docTypeProperty,
            int propertyValue)
            : this(id, docTypeProperty, new Dictionary<string, object> { { "Value", propertyValue } })
        {
        }

        /// <summary>
        /// Constructor for creating a property that has a single bool value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="docTypeProperty"></param>
        /// <param name="propertyValue">The value of the property</param>
        /// <remarks>
        /// This will create a new Dictionary object to put the value in with a key of "Value"
        /// </remarks>
        public ContentProperty(HiveId id,
            DocumentTypeProperty docTypeProperty,
            bool propertyValue)
            : this(id, docTypeProperty, new Dictionary<string, object> { { "Value", propertyValue } })
        {
        }

        /// <summary>
        /// Constructor for creating a property that has a single decimal value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="docTypeProperty"></param>
        /// <param name="propertyValue">The value of the property</param>
        /// <remarks>
        /// This will create a new Dictionary object to put the value in with a key of "Value"
        /// </remarks>
        public ContentProperty(HiveId id,
            DocumentTypeProperty docTypeProperty,
            decimal propertyValue)
            : this(id, docTypeProperty, new Dictionary<string, object> { { "Value", propertyValue } })
        {
        }
        
        /// <summary>
        /// Constructor for creating a property that has a single double value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="docTypeProperty"></param>
        /// <param name="propertyValue">The value of the property</param>
        /// <remarks>
        /// This will create a new Dictionary object to put the value in with a key of "Value"
        /// </remarks>
        public ContentProperty(HiveId id,
            DocumentTypeProperty docTypeProperty,
            double propertyValue)
            : this(id, docTypeProperty, new Dictionary<string, object> { { "Value", propertyValue } })
        {
        }

        /// <summary>
        /// Override name to be declared readonly
        /// </summary>
        [ReadOnly(true)]
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        [HiddenInput(DisplayValue = false)]
        [ReadOnly(true)]
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the tab id.
        /// </summary>
        /// <value>
        /// The tab id.
        /// </value>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public HiveId TabId { get; set; }

        /// <summary>
        /// Gets or sets the tab id.
        /// </summary>
        /// <value>
        /// The tab id.
        /// </value>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public string TabAlias { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        [HiddenInput(DisplayValue = false)]
        [ReadOnly(true)]
        public string Alias { get; set; }
        
        /// <summary>
        /// Gets the doc type property.
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public DocumentTypeProperty DocTypeProperty { get; private set; }

        /// <summary>
        /// Gets the doc type property id.
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        [ReadOnly(true)]
        public HiveId DocTypePropertyId { get; private set; }

        /// <summary>
        /// Returns the property editor model used to render the editor for this property
        /// </summary>
        /// <remarks>
        /// The property editor model is returned from the <see cref="EditorModel"/> GetEditorModel method
        /// </remarks>
        public dynamic PropertyEditorModel
        {
            get
            {
                if (_propertyEditorModel == null)
                {
                    var model = DocTypeProperty.GetEditorModel();
                    if (model != null) model.SetModelValues(_propertyValue);
                    _propertyEditorModel = model;    
                }                
                return _propertyEditorModel;
            }
        }


        /// <summary>
        /// When implemented in a class, provides metadata to the model metadata creation process.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.DisplayName = Name;
        }
    }
}
