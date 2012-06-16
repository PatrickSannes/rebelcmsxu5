using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Macros
{
    public class MacroEngineMetadata : PluginMetadataComposition
    {
        public MacroEngineMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }


        /// <summary>
        /// Whether or not this is an built-in Umbraco macro engine
        /// </summary>
        public bool IsInternalUmbracoEngine { get; set; }

        /// <summary>
        /// Gets the name of the MacroEngine
        /// </summary>
        public string EngineName { get; set; }

    }
}
