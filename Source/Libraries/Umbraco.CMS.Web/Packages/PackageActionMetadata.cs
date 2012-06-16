using System.Collections.Generic;
using Umbraco.Cms.Domain.BackOffice;
using Umbraco.Foundation;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Packages
{

    public class PackageActionMetadata : PluginMetadataComposition
    {
        public PackageActionMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}
