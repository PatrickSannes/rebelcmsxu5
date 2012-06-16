using NuGet;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    public class PackageModel
    {
        public IPackageMetadata Metadata { get; set; }

        /// <summary>
        /// true if this package id is installed, regardless of version
        /// </summary>
        public bool IsPackageInstalled { get; set; }
        
        /// <summary>
        /// If this package is the latest available version
        /// </summary>
        public bool IsLatestVersion { get; set; }

        /// <summary>
        /// true if the current version of the package is installed
        /// </summary>
        public bool IsVersionInstalled { get; set; }
    }
}
