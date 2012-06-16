using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Respresents the view model used to render the document type editor
    /// </summary>
    [Bind(Exclude = "AllowedTemplateIds,SelectedAllowedTemplates,AvailableTemplates")]
    public class DocumentTypeEditorModel : AbstractSchemaEditorModel
    {
        /// <summary>
        /// initialize collections so they are never null
        /// </summary>
        public DocumentTypeEditorModel(HiveId id)
            : base(id)
        {
            AllowedTemplates = Enumerable.Empty<SelectListItem>();
            AllowedTemplateIds = new HashSet<HiveId>();
        }

        /// <summary>
        /// initialize collections so they are never null
        /// </summary>
        public DocumentTypeEditorModel()
        {
            AllowedTemplates = Enumerable.Empty<SelectListItem>();
            AllowedTemplateIds = new HashSet<HiveId>();
        }

        /// <summary>
        /// The default template to use for this doc type
        /// </summary>
        [DisplayName("Default Template")]
        public HiveId? DefaultTemplateId { get; set; }

        /// <summary>
        /// List of all of the selected allowed template Ids
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public HashSet<HiveId> AllowedTemplateIds { get; set; }

        /// <summary>
        /// A list of allowed templates check box list
        /// </summary>
        [DisplayName("Allowed Templates")]
        public IEnumerable<SelectListItem> AllowedTemplates { get; set; }

    }
}
