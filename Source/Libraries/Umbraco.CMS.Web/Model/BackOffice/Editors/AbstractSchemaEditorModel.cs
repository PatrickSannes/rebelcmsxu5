using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents the view model used to render a document type editor
    /// </summary>
    [Bind(Exclude = "AvailableThumbnails,AvailableIcons,AvailableDataTypes,AvailableTabs,AllowedChildIds,InheritFromIds,IconsBaseUrl,ThumbnailsBaseUrl,SpriteFileUrls,IsReadOnly")]
    public abstract class AbstractSchemaEditorModel : EditorModel, IModelBindAware
    {
        /// <summary>
        /// Creates a new model with an existing id
        /// </summary>
        /// <param name="id"></param>
        protected AbstractSchemaEditorModel(HiveId id)
            : this()
        {
            Mandate.ParameterNotEmpty(id, "id");
            Id = id;
        }

        /// <summary>
        /// initialize collections so they are never null
        /// </summary>
        protected AbstractSchemaEditorModel()
        {
            NewProperty = new DocumentTypeProperty();
            ActiveTabIndex = 0;
            Properties = new HashSet<DocumentTypeProperty>();

            AllowedChildren = Enumerable.Empty<SelectListItem>();
            AllowedChildIds = Enumerable.Empty<HiveId>();
            InheritFrom = Enumerable.Empty<HierarchicalSelectListItem>();
            InheritFromIds = Enumerable.Empty<HiveId>();
            AvailableIcons = Enumerable.Empty<SelectListItem>();
            AvailableThumbnails = Enumerable.Empty<SelectListItem>();
            AvailableDataTypes = Enumerable.Empty<SelectListItem>();
            AvailableTabs = Enumerable.Empty<SelectListItem>();

            //ensure theres always a general tab
            DefinedTabs = new HashSet<Tab>();
            var generalGroup = FixedGroupDefinitions.GeneralGroup;
            DefinedTabs.Add(new Tab { Alias = generalGroup.Alias, Name = generalGroup.Name, SortOrder = generalGroup.Ordinal });

            InheritedTabs = new HashSet<Tab>();
            InheritedProperties = new HashSet<DocumentTypeProperty>();

            IsReadOnly = false;
            IsAbstract = false;

            Icon = "tree-folder";
            Thumbnail = "doc.png";

            PopulateUIElements();
        }

        /// <summary>
        /// The tab to display
        /// </summary>
        [HiddenInput]
        public int ActiveTabIndex { get; set; }

        /// <summary>
        /// The alias used to reference the doc type
        /// </summary>
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// The icon to be displayed in the tree for a content node of this doc type
        /// </summary>
        [Required]
        public string Icon { get; set; }

        /// <summary>
        /// The thumbnail to be displayed in the create model for a content node of this doc type
        /// </summary>
        [Required]
        public string Thumbnail { get; set; }

        /// <summary>
        /// The description of this doc type
        /// </summary>
        [DataType(global::System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// Setting for whether doc type is only to be used in inheritance, and not available for content entry
        /// </summary>
        [DisplayName("Inheritable Only")]
        public bool IsAbstract { get; set; }

        /// <summary>
        /// A list of allowed children types
        /// </summary>
        [DisplayName("Allowed Children")]
        public IEnumerable<SelectListItem> AllowedChildren { get; set; }

        /// <summary>
        /// A list of types to inherit from
        /// </summary>
        [DisplayName("Inherit From")]
        public IEnumerable<HierarchicalSelectListItem> InheritFrom { get; set; }

        /// <summary>
        /// The defined tabs for the content editor
        /// </summary>
        [DisplayName("Tabs")]
        public HashSet<Tab> DefinedTabs { get; set; }

        /// <summary>
        /// The properites of this doc type
        /// </summary>
        public HashSet<DocumentTypeProperty> Properties { get; set; }

        /// <summary>
        /// Model for creating a new property for the Document Type
        /// </summary>
        public DocumentTypeProperty NewProperty { get; set; }

        /// <summary>
        /// Whether or not the user is creating a new property
        /// </summary>
        /// <remarks>
        /// This is used to determine how validation works for the NewProperty property of this model, 
        /// it is also used to determine if the NewProperty UI box should be expanded by default.
        /// </remarks>
        public bool IsCreatingNewProperty { get; set; }

        /// <summary>
        /// Name of the new tab to be created
        /// </summary>
        [DisplayName("New tab")]
        public string NewTabName { get; set; }

        /// <summary>
        /// list of the allowed child doc type ids
        /// </summary>
        [ScaffoldColumn(false)]
        public IEnumerable<HiveId> AllowedChildIds { get; set; }

        /// <summary>
        /// list of the doc type ids to inherit from
        /// </summary>
        [ScaffoldColumn(false)]
        public IEnumerable<HiveId> InheritFromIds { get; set; }

        /// <summary>
        /// list of the available icons
        /// </summary>
        [ScaffoldColumn(false)]
        public IEnumerable<SelectListItem> AvailableIcons { get; set; }

        /// <summary>
        /// list of the available thumbnails
        /// </summary>
        [ScaffoldColumn(false)]
        public IEnumerable<SelectListItem> AvailableThumbnails { get; set; }

        /// <summary>
        /// List of defined data types
        /// </summary>
        [ScaffoldColumn(false)]
        public IEnumerable<SelectListItem> AvailableDataTypes { get; set; }

        /// <summary>
        /// List of tabs to choose from
        /// </summary>
        [ScaffoldColumn(false)]
        public IEnumerable<SelectListItem> AvailableTabs { get; set; }

        /// <summary>
        /// the base URL to the icon files
        /// </summary>
        [ScaffoldColumn(false)]
        public string IconsBaseUrl { get; set; }

        /// <summary>
        /// the base URL to the icon files
        /// </summary>
        [ScaffoldColumn(false)]
        public string ThumbnailsBaseUrl { get; set; }

        /// <summary>
        /// a list of sprite file urls to reference
        /// </summary>
        [ScaffoldColumn(false)]
        public IEnumerable<string> SpriteFileUrls { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        [ScaffoldColumn(false)]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the inherited tabs.
        /// </summary>
        public HashSet<Tab> InheritedTabs { get; set; }

        /// <summary>
        /// Gets or sets the inherited properties.
        /// </summary>
        public HashSet<DocumentTypeProperty> InheritedProperties { get; set; }

        public void BindModel(IModelUpdator modelUpdator)
        {
            Mandate.ParameterNotNull(modelUpdator, "updator");

            //bind this model, this will also update the NewProperty model
            //since the prefixes are aligned.
            modelUpdator.BindModel(this, string.Empty);
            //bind custom properties
            foreach (var p in Properties)
            {
                modelUpdator.BindModel(p, p.Id.GetHtmlId());

                //bind the override pre-values
                if (p.PreValueEditor != null)
                {
                    modelUpdator.BindModel(p.PreValueEditor, string.Concat(p.Id.GetHtmlId(), ".PreValueEditor"));
                }

            }

            //bind the tabs
            foreach (var t in DefinedTabs)
            {
                if (!t.Id.IsNullValueOrEmpty())
                {
                    modelUpdator.BindModel(t, t.Id.GetHtmlId());
                }
            }

        }

        protected void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}