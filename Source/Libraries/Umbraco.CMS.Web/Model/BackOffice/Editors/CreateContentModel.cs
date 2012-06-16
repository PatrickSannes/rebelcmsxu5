using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Mvc.Validation;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Used to render the create content dialog
    /// </summary>
    [Bind(Exclude = "NoticeBoard")]
    public class CreateContentModel
    {

        public CreateContentModel()
        {
            //By default will be under the content root
            ParentId = FixedHiveIds.ContentVirtualRoot;
            NoticeBoard = new List<NotificationMessage>();
        }

        /// <summary>
        /// The parent node to create content under
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public HiveId ParentId { get; set; }

        /// <summary>
        /// the name of the new content item
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The selected document  type
        /// </summary>
        [HiveIdRequired]
        [UIHint("DocumentTypeDropDown")]
        public virtual HiveId SelectedDocumentTypeId { get; set; }

        /// <summary>
        /// Used to display important messages about the node
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn((false))]
        [UIHint("NoticeBoard")]
        public IList<NotificationMessage> NoticeBoard { get; private set; }
    }
}