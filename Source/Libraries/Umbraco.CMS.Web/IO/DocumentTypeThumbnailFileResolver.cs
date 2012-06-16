using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration;

using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.IO
{
    public class DocumentTypeThumbnailFileResolver : SpriteIconFileResolver
    {
        public const string SpriteNamePrefixValue = "thumb-";

        public DocumentTypeThumbnailFileResolver(HttpServerUtilityBase server, UmbracoSettings settings, UrlHelper url)
            : base(new DirectoryInfo(server.MapPath(settings.UmbracoFolders.DocTypeThumbnailFolder)), url)
        {
        }

        public override string SpriteNamePrefix
        {
            get { return SpriteNamePrefixValue; }
        }
    }
}