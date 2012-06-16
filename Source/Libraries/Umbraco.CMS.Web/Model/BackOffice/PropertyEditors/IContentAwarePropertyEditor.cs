using Umbraco.Cms.Web.Model.BackOffice.Editors;

namespace Umbraco.Cms.Web.Model.BackOffice.PropertyEditors
{
    /// <summary>
    /// Interface specifically used for type checking   
    /// </summary>
    internal interface IContentAwarePropertyEditor
    {
        void SetContentProperty(ContentProperty property);
        void SetContentItem(BasicContentEditorModel contentItem);
        void SetDocumentType(DocumentTypeEditorModel docType);
        bool IsContentPropertyAvailable { get; }
        bool IsDocumentTypeAvailable { get; }
        bool IsContentModelAvailable { get; }
    }
}