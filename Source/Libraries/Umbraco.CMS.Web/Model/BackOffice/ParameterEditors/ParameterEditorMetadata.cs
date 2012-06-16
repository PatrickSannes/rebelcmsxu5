using System;
using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.ParameterEditors
{
    public class ParameterEditorMetadata : PluginMetadataComposition
    {
        public ParameterEditorMetadata(IDictionary<string, object> obj)
            : base(obj)
        { }

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
        /// Gets or sets the property editor id.
        /// </summary>
        public Guid PropertyEditorId { get; set; }
    }
}
