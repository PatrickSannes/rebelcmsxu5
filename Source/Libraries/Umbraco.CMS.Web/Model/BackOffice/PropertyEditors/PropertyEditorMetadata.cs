using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.PropertyEditors
{
    public class PropertyEditorMetadata : PluginMetadataComposition
    {
        public PropertyEditorMetadata(IDictionary<string, object> obj)
        : base(obj)
        {
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; set; }

        /// <summary>
        /// Whether or not this is a built-in Umbraco editor
        /// </summary>
        public bool IsInternalUmbracoEditor { get; set; }

        /// <summary>
        /// Flag determining if this property editor is used to edit content
        /// </summary>
        public bool IsContentPropertyEditor { get; set; }

        /// <summary>
        /// Flag determining if this property editor is used to edit parameters
        /// </summary>
        public bool IsParameterEditor { get; set; }
    }
}