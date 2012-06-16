using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Mvc.Validation;
using Umbraco.Framework;
using System.Web.Mvc;
using PropertyEditorModel = Umbraco.Cms.Web.Model.BackOffice.PropertyEditors.EditorModel;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{

    /// <summary>
    /// Defines a Document Type property
    /// </summary>
    [Bind(Exclude = "DataType, SchemaId, TabAlias")]
    public class DocumentTypeProperty : TimestampedModel
    {

        private dynamic _preValueModel;

        /// <summary>
        /// Constructor to setup an existing DocumentTypeProperty from a DataType
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataType"></param>
        public DocumentTypeProperty(HiveId id, DataType dataType)
            : this(dataType)
        {
            Mandate.ParameterNotEmpty(id, "id");
            Id = id;            
        }

        /// <summary>
        /// Constructor to setup an existing DocumentTypeProperty from a DataType
        /// </summary>
        /// <param name="dataType"></param>
        public DocumentTypeProperty(DataType dataType)
        {
            Mandate.ParameterNotNull(dataType, "dataType");
            DataType = dataType;
            DataTypeId = DataType.Id;
        }

        /// <summary>
        /// Constructor to create a new document type property
        /// </summary>
        public DocumentTypeProperty()
        {
            
        }

        /// <summary>
        /// Returns the editor model for the content node based on this Doc Type property which may contain
        /// overridden Pre-Values that were specified on the Data Type
        /// </summary>
        /// <returns></returns>
        public PropertyEditorModel GetEditorModel()
        {
            var model = DataType.GetEditorModel();
            return model;
        }

        /// <summary>
        /// Gets or sets the schema id.
        /// </summary>
        [ReadOnly(true), ScaffoldColumn(false)]
        public HiveId SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        [ReadOnly(true), ScaffoldColumn(false)]
        public string SchemaName { get; set; }

        /// <summary>
        /// The underlying DataType object used to create this model
        /// </summary>
        /// <remarks>
        /// Settings this property will re-initialize the OverriddenPreVaules, DataTypeId and the PreValueEditor
        /// </remarks>
        [ReadOnly(true), ScaffoldColumn(false)]
        public DataType DataType { get; internal set; }

        /// <summary>
        /// The alias for the doc type property
        /// </summary>
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// The DataType Id for the doc type property
        /// </summary>
        /// <remarks>
        /// This property is marked settable for use with model binding, generally this property shouldn't be set manually otherwise,
        /// This property will be set by specifying a DataType.
        /// </remarks>
        [HiveIdRequired]
        public HiveId DataTypeId { get; set; }

        /// <summary>
        /// The tab id that the doc type property will exist on
        /// </summary>
        public HiveId TabId { get; set; }

        /// <summary>
        /// The tab alias that the doc type property will exist on
        /// </summary>
        [ReadOnly(true), ScaffoldColumn(false)]
        public string TabAlias { get; set; }

        /// <summary>
        /// The description of the doc type property
        /// </summary>
        [DataType(global::System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// The order in which the properties appear
        /// </summary>
        [RegularExpression(@"\d+")]
        public int SortOrder { get; set; }

        /// <summary>
        /// The prevalue model associated with the Property editor used to display it's properties that are attributed with AllowDocumentTypePropertyOverrideAttribute
        /// </summary>
        /// <remarks>
        /// <para>
        /// An example of this usage is that the text box Property Editor contains 2 pre-values: a regular expression to validate the input and a boolean of whether or not the vaue is required.
        /// </para>
        /// <para>
        /// Since an Umbraco administrator may want to use the same Data Type for different properties of a Document Type, it would be nicer to modify if this property is required or not instead of having
        /// to create multipe Data Types to support this.
        /// </para>
        /// </remarks>
        public dynamic PreValueEditor
        {
            get
            {
                if (_preValueModel== null && DataType != null)
                {
                    _preValueModel = DataType.GetPreValueModel();
                }
                return _preValueModel;
            }
        }

    }
}
