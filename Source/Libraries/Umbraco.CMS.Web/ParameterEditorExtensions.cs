using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web
{
    public static class ParameterEditorExtensions
    {
        /// <summary>
        /// Returns a parameter editor definition based on a parameter editor id
        /// </summary>
        /// <param name="parameterEditors"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Lazy<AbstractParameterEditor, ParameterEditorMetadata> GetParameterEditor(this IEnumerable<Lazy<AbstractParameterEditor, ParameterEditorMetadata>> parameterEditors, Guid id)
        {
            return parameterEditors.Where(x => x.Metadata.Id == id).SingleOrDefault();
        }
    }
}
