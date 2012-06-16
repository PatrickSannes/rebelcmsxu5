using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents the view model used to render a media type editor
    /// </summary>
    public class MediaTypeEditorModel : AbstractSchemaEditorModel
    {
        public MediaTypeEditorModel()
        {
            
        }

        public MediaTypeEditorModel(HiveId id)
            : base(id)
        {
        }
    }
}