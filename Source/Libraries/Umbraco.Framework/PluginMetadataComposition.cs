using System;
using System.Collections.Generic;


namespace Umbraco.Framework
{
    /// <summary>
    /// Abstract meta data class defining a metadata for a Plugin
    /// </summary>
    public abstract class PluginMetadataComposition : MetadataComposition
    {
        protected PluginMetadataComposition(IDictionary<string, object> obj)
            : base(obj)
        {
        }

        /// <summary>
        /// The definition of the plugin that contains this editor
        /// </summary>
        public PluginDefinition PluginDefinition { get; set; }
        
        ///<summary>
        /// The Type of the plugin
        ///</summary>
        public Type ComponentType { get; set; }

        /// <summary>
        /// The Id of the plugin
        /// </summary>
        public Guid Id { get; set; }
    }
}