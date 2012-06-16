using System;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// A Factory used to resolve PropertyEditors
    /// </summary>
    public interface IPropertyEditorFactory
    {
        Lazy<PropertyEditor, PropertyEditorMetadata> GetPropertyEditor(Guid id);
    }
}