using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice;
using System;

namespace Umbraco.Cms.Web.IO
{
    /// <summary>
    /// Abstract resolver to find icons/images and return a list of Icon objects
    /// </summary>
    public abstract class AbstractIconFileResolver : FileResolver<Icon>
    {
        private readonly DirectoryInfo _rootFolder;

        protected AbstractIconFileResolver(DirectoryInfo folderPath, UrlHelper url)
        {
            _rootFolder = folderPath;
            Url = url;
        }

        protected override DirectoryInfo RootFolder
        {
            get { return _rootFolder; }
        }

        protected override IEnumerable<string> AllowedExtensions
        {
            get { return new[] { "*.png","*.jpg", "*.gif", "*.css" }; }
        }

        protected string FolderPath { get; set; }
        protected UrlHelper Url { get; set; }

        protected override Icon GetItem(FileInfo file)
        {
            var name = file.Name.Substring(0, file.Name.LastIndexOf(Path.GetExtension(file.Name)));
            var subdir = 
                file.DirectoryName != RootFolder.FullName && file.DirectoryName.Contains(RootFolder.FullName) ?
                file.DirectoryName.Replace(RootFolder.FullName, string.Empty).Replace("\\","/") : null;

            var icon = new Icon
            {
                Name = name,
                Url = Url.Content(subdir + "/" + file.Name),
                IconType = IconType.Image
            };
            return icon;
        }
    }
}