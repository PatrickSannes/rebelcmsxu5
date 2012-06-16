using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    using global::System.Linq;
    using global::System.Web;

    /// <summary>
    /// The model to render the content editor
    /// </summary>
    [Bind(Exclude = "DocumentLinks")]
    public class ContentEditorModel : StandardContentEditorModel
    {
        public const string PreviewQuerystringKey = "umbPreview";

        public ContentEditorModel(HiveId id)
            : this()
        {
            Mandate.ParameterNotEmpty(id, "id");
            Id = id;
        }

        public ContentEditorModel()
        {
            //initialize properties
            DocumentLinks = new string[] { };
        }

        public string GeneratePreviewUrl()
        {
            var first = DocumentLinks.FirstOrDefault();
            if (first == null || !Uri.IsWellFormedUriString(first, UriKind.Relative)) return string.Empty;

            //return first.TrimEnd('/') + "/?revisionStatusType=draft";
            return first.TrimEnd('/') + "/?" + PreviewQuerystringKey + "=true";
        }


        /// <summary>
        /// A list of 'NiceUrls' to display
        /// </summary>
        /// <remarks>
        /// This allows for multiple URLs based on domain
        /// </remarks>
        [UIHint("DocumentLinks")]
        public string[] DocumentLinks { get; set; }
    }
}
