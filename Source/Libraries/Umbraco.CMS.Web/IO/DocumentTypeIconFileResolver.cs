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
    /// <summary>
    /// Looks up the document type icons and returns a list of Icon objects
    /// </summary>
    /// <remarks>
    /// This will lookup all icon files and then search for any files with an associated CSS file which is named with 
    /// the same name as the icon. This indicates that the the image file is a sprite file and the CSS file defines the sprites.
    /// If this is found, we will remove the sprite icon file from the list, then parse it's sprite names from the CSS file and add their
    /// names to the collection.
    /// </remarks>
    public class DocumentTypeIconFileResolver : SpriteIconFileResolver
    {
        public const string SpriteNamePrefixValue = "tree-";

        public DocumentTypeIconFileResolver(HttpServerUtilityBase server, UmbracoSettings settings, UrlHelper url)
            : base(new DirectoryInfo(server.MapPath(settings.UmbracoFolders.DocTypeIconFolder)), url)
        {
        }
        
        public override string SpriteNamePrefix
        {
            get { return SpriteNamePrefixValue; }
        }

    }
}