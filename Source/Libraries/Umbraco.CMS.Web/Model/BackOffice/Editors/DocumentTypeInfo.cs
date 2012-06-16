using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents some basic presentation information for a document type, mostly used in drop down lists, etc...
    /// </summary>
    public class DocumentTypeInfo : IdentityModel
    {
        public string Thumbnail { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }

    }
}