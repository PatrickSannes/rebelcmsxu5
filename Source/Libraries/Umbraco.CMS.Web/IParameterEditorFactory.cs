using System;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// A Factory used to resolve ParameterEditors
    /// </summary>
    public interface IParameterEditorFactory
    {
        Lazy<AbstractParameterEditor, ParameterEditorMetadata> GetParameterEditor(Guid id);
    }
}
