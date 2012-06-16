using System;
using System.ComponentModel.DataAnnotations;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public abstract class StandardContentEditorModel : BasicContentEditorModel
    {
        /// <summary>
        /// Gets or sets the revision id
        /// </summary>
        public HiveId RevisionId { get; set; }

        /// <summary>
        /// The last published date, if null then the document is not published
        /// </summary>
        [UIHint("PublicationStatus")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? UtcPublishedDate { get; set; }

        /// <summary>
        /// The date to publish at
        /// </summary>
        [UIHint("DateTimePicker")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? UtcPublishScheduled { get; set; }

        /// <summary>
        /// The date to unpublish at
        /// </summary>
        [UIHint("DateTimePicker")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? UtcUnpublishScheduled { get; set; }
    }
}