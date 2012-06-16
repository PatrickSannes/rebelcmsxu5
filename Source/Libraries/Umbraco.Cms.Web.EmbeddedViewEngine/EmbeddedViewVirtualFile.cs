using System.IO;
using System.Web.Hosting;

namespace Umbraco.Cms.Web.EmbeddedViewEngine
{
    /// <summary>
    /// Represents a Virtual file for an embedded View resource
    /// </summary>
    internal class EmbeddedViewVirtualFile : VirtualFile
    {

        internal EmbeddedViewVirtualFile(string resourcePath, string virtualPath)
            : base(virtualPath)
        {
            _resourcePath = resourcePath;
        }

        private readonly string _resourcePath;

        /// <summary>
        /// Returns the md5 name of the file
        /// </summary>
        public override string Name
        {
            get
            {

                return EmbeddedViewPath.GetHashFromPath(VirtualPath);
            }
        }

        /// <summary>
        /// Opens the resource as as Stream
        /// </summary>
        /// <returns></returns>
        public override Stream Open()
        {
            return EmbeddedViewPath.Open(_resourcePath);
        }

        /// <summary>
        /// Return the string representation of the file
        /// </summary>
        /// <returns></returns>
        public string GetContents()
        {
            return EmbeddedViewPath.GetViewContent(_resourcePath);
        }

    }
}
