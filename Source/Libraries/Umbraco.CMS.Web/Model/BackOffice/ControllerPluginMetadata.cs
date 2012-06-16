using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// Represents a base class for controller plugin meta data classes to inherit from
    /// </summary>
    public abstract class ControllerPluginMetadata : PluginMetadataComposition
    {
        protected ControllerPluginMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }

        /// <summary>
        /// The controller name of the editor (without the 'Controller' suffix)
        /// </summary>
        public string ControllerName { get; set; }


    }
}