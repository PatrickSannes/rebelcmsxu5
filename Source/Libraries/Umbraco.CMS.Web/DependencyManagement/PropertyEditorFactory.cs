using System;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.DependencyManagement
{
    /// <summary>
    /// Used to resolve a PropertyEditor
    /// </summary>
    public class PropertyEditorFactory : IPropertyEditorFactory
    {
        public Lazy<PropertyEditor, PropertyEditorMetadata> GetPropertyEditor(Guid id)
        {
            //NOTE: Yes, we are using the resolver here, we need to be able to resolve new instances of property editors registered in the container
            var editors = DependencyResolver.Current.GetServices<Lazy<PropertyEditor, PropertyEditorMetadata>>();
            
            var editorMeta = editors.GetPropertyEditor(id);

            return editorMeta;
        }
    }
}