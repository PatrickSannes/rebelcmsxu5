using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration;

namespace Umbraco.Cms.Web.IO
{
    public class ApplicationIconFileResolver : SpriteIconFileResolver
    {
        public const string SpriteNamePrefixValue = "app-";

        public ApplicationIconFileResolver(HttpServerUtilityBase server, UmbracoSettings settings, UrlHelper url)
            : base(new DirectoryInfo(server.MapPath(settings.UmbracoFolders.ApplicationIconFolder)), url)
        {
        }

        public override string SpriteNamePrefix
        {
            get { return SpriteNamePrefixValue; }
        }

    }
}