using System.Collections.Generic;

namespace Umbraco.Framework.Security
{
    public class PermissionMetadata : PluginMetadataComposition
    {
        public PermissionMetadata(IDictionary<string, object> obj)
            : base(obj)
        { }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
    }
}
