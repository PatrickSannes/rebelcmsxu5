using System;
using System.Web;
using NuGet;
using Umbraco.Cms.Web.Configuration;

namespace Umbraco.Cms.Web.Packaging
{
    public class DefaultPackageContext : IPackageContext
    {
        private readonly UmbracoSettings _settings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="mapPath">A delegate method used to perform a Server.MapPath operation</param>
        public DefaultPackageContext(UmbracoSettings settings, Func<string, string> mapPath)
        {
            _settings = settings;

            _pluginInstallFolderPath = mapPath(_settings.PluginConfig.PluginsPath + "/Packages");
            _localPackageRepoFolderPath = mapPath(_settings.UmbracoFolders.LocalPackageRepositoryFolder);

            //create lazy instances of each
            _localPackageRepository = new Lazy<IPackageRepository>(
                () =>
                    {
                        //create a new path resolver with false as 'useSideBySidePaths' so that it doesn't install with version numbers.
                        var packageFileSys = new PhysicalFileSystem(_localPackageRepoFolderPath);
                        var packagePathResolver = new DefaultPackagePathResolver(packageFileSys, false);
                        return new LocalPackageRepository(packagePathResolver, packageFileSys, true);
                    });

            _localPackageManager = new Lazy<IPackageManager>(
                () =>
                    {
                        //create a new path resolver with false as 'useSideBySidePaths' so that it doesn't install with version numbers.
                        var packageFileSys = new PhysicalFileSystem(_pluginInstallFolderPath);
                        var packagePathResolver = new DefaultPackagePathResolver(packageFileSys, false);
                        return new PackageManager(_localPackageRepository.Value, packagePathResolver, packageFileSys);
                    });
            
            _publicPackageRepository = new Lazy<IPackageRepository>(
                () => PackageRepositoryFactory.Default.CreateRepository(_settings.PublicPackageRepository.RepositoryAddress));
            
            _publicPackageManager = new Lazy<IPackageManager>(
                () => new PackageManager(_publicPackageRepository.Value, mapPath(_settings.PluginConfig.PluginsPath + "/Packages")));
        }

        private readonly string _localPackageRepoFolderPath;
        private readonly string _pluginInstallFolderPath;
        private readonly Lazy<IPackageManager> _localPackageManager;
        private readonly Lazy<IPackageRepository> _localPackageRepository;
        private readonly Lazy<IPackageManager> _publicPackageManager;
        private readonly Lazy<IPackageRepository> _publicPackageRepository;
        
        /// <summary>
        /// Gets the local path resolver.
        /// </summary>
        public IPackagePathResolver LocalPathResolver
        {
            get { return ((PackageManager) LocalPackageManager).PathResolver; }
        }

        /// <summary>
        /// Gets the local package manager.
        /// </summary>
        public IPackageManager LocalPackageManager
        {
            get { return _localPackageManager.Value; }
        }

        /// <summary>
        /// Gets the public package manager.
        /// </summary>
        public IPackageManager PublicPackageManager
        {
            get { return _publicPackageManager.Value; }
        }

       

    }
}
