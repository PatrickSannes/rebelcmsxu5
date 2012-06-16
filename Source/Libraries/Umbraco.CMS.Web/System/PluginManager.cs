using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Web.Compilation;
using System.Reflection;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.System;

using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;

// SEE THIS POST for full details of what this does
//http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust.aspx

[assembly: PreApplicationStartMethod(typeof(PluginManager), "Initialize")]

namespace Umbraco.Cms.Web.System
{
    /// <summary>
    /// Sets the application up for the plugin referencing
    /// </summary>
    public class PluginManager
    {
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static DirectoryInfo _shadowCopyFolder;

        public const string PackagesFolderName = "Packages";

        /// <summary>
        /// Returns a collection of all referenced plugin assemblies that have been shadow copied
        /// </summary>
        public static IEnumerable<PluginDefinition> ReferencedPlugins { get; private set; }

        public static void Initialize()
        {
            using (new WriteLockDisposable(Locker))
            {
                using (DisposableTimer.TraceDuration<PluginManager>("Start Initialise", "End Initialise"))
                {
                    var settings = UmbracoSettings.GetSettings();

                    // TODO: Add verbose exception handling / raising here since this is happening on app startup and could
                    // prevent app from starting altogether
                    var pluginFolder = new DirectoryInfo(HostingEnvironment.MapPath(settings.PluginConfig.PluginsPath));
                    _shadowCopyFolder = new DirectoryInfo(HostingEnvironment.MapPath(settings.PluginConfig.ShadowCopyPath));

                    var referencedPlugins = new List<PluginDefinition>();

                    var pluginFiles = Enumerable.Empty<FileInfo>();

                    try
                    {
                        LogHelper.TraceIfEnabled<PluginManager>("Creating shadow copy folder and querying for dlls");

                        //ensure folders are created
                        Directory.CreateDirectory(pluginFolder.FullName);
                        Directory.CreateDirectory(_shadowCopyFolder.FullName);
                        Directory.CreateDirectory(Path.Combine(pluginFolder.FullName, PackagesFolderName));
                        
                        //get list of all DLLs in bin
                        var binFiles = _shadowCopyFolder.GetFiles("*.dll", SearchOption.AllDirectories);

                        //get list of all DLLs in plugins (not in bin!)
                        //this will match the plugin folder pattern
                        pluginFiles = pluginFolder.GetFiles("*.dll", SearchOption.AllDirectories)
                            //just make sure we're not registering shadow copied plugins
                            .Where(x => !binFiles.Select(q => q.FullName).Contains(x.FullName))
                            .Where(x => x.Directory.Parent != null && (IsPackagePluginBinFolder(x.Directory)))
                            .ToList();

                        //clear out shadow copied plugins
                        foreach (var f in binFiles)
                        {
                            LogHelper.TraceIfEnabled<PluginManager>("Deleting {0}", () => f.Name);

                            File.Delete(f.FullName);
                        }

                    }
                    catch (Exception ex)
                    {
                        var fail = new ApplicationException("Could not initialise plugin folder", ex);
                        LogHelper.Error<PluginManager>(fail.Message, fail);
                        //throw fail;
                    }

                    try
                    {
                        LogHelper.TraceIfEnabled<PluginManager>("Shadow copying assemblies");

                        //shadow copy files
                        referencedPlugins
                            .AddRange(pluginFiles.Select(plug => 
                                new PluginDefinition(plug, 
                                    GetPackageFolderFromPluginDll(plug),                                    
                                    PerformFileDeploy(plug),
                                    plug.Directory.Parent.Name == "Core")));
                    }
                    catch (Exception ex)
                    {
                        var fail = new ApplicationException("Could not initialise plugin folder", ex);
                        LogHelper.Error<PluginManager>(fail.Message, fail);
                        //throw fail;

                    }

                    ReferencedPlugins = referencedPlugins;
                }
            }          
        }

        private static Assembly PerformFileDeploy(FileInfo plug)
        {
            if (plug.Directory.Parent == null)
                throw new InvalidOperationException("The plugin directory for the " + plug.Name +
                                                    " file exists in a folder outside of the allowed Umbraco folder heirarchy");

            FileInfo shadowCopiedPlug;
            
            if (SystemUtilities.GetCurrentTrustLevel() != AspNetHostingPermissionLevel.Unrestricted)
            {
                //all plugins will need to be copied to ~/App_Plugins/bin/Packages
                //this is aboslutely required because all of this relies on probingPaths being set statically in the web.config
                var shadowCopyPlugFolderName = PackagesFolderName;

                LogHelper.TraceIfEnabled<PluginManager>(plug.FullName + " to " + shadowCopyPlugFolderName);

                //were running in med trust, so copy to custom bin folder
                var shadowCopyPlugFolder = Directory.CreateDirectory(Path.Combine(_shadowCopyFolder.FullName, shadowCopyPlugFolderName));
                shadowCopiedPlug = InitializeMediumTrust(plug, shadowCopyPlugFolder);
            }
            else
            {
                LogHelper.TraceIfEnabled<PluginManager>(plug.FullName + " to " + AppDomain.CurrentDomain.DynamicDirectory);
                //were running in full trust so copy to standard dynamic folder
                shadowCopiedPlug = InitializeFullTrust(plug, new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory));
            }

            //we can now register the plugin definition
            var shadowCopiedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(shadowCopiedPlug.FullName));
            
            //add the reference to the build manager
            LogHelper.TraceIfEnabled<PluginManager>("Adding to BuildManager: '{0}'", () => shadowCopiedAssembly.FullName);
            BuildManager.AddReferencedAssembly(shadowCopiedAssembly);

            return shadowCopiedAssembly;
        }

        /// <summary>
        /// Used to initialize plugins when running in Full Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <returns></returns>
        private static FileInfo InitializeFullTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));
            try
            {
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            catch (IOException)
            {
                LogHelper.TraceIfEnabled<PluginManager>(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                //this occurs when the files are locked,
                //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                try
                {
                    var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                    File.Move(shadowCopiedPlug.FullName, oldFile);
                }
                catch (IOException)
                {
                    LogHelper.TraceIfEnabled<PluginManager>(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin");    
                    throw;
                }
                //ok, we've made it this far, now retry the shadow copy
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            return shadowCopiedPlug;
        }

        /// <summary>
        /// Used to initialize plugins when running in Medium Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <returns></returns>
        private static FileInfo InitializeMediumTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shouldCopy = true;
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));

            //check if a shadow copied file already exists and if it does, check if its updated, if not don't copy
            if (shadowCopiedPlug.Exists)
            {
                if (shadowCopiedPlug.CreationTimeUtc.Ticks == plug.CreationTimeUtc.Ticks)
                {
                    LogHelper.TraceIfEnabled<PluginManager>("Not copying; files appear identical: '{0}'", () => shadowCopiedPlug.Name);
                    shouldCopy = false;
                }
            }

            if (shouldCopy)
            {
                try
                {
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
                catch (IOException)
                {
                    LogHelper.TraceIfEnabled<PluginManager>(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                    //this occurs when the files are locked,
                    //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                    //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                    try
                    {
                        var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                        File.Move(shadowCopiedPlug.FullName, oldFile);
                    }
                    catch (IOException)
                    {
                        LogHelper.TraceIfEnabled<PluginManager>(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin");
                        throw;
                    }
                    //ok, we've made it this far, now retry the shadow copy
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
            }

            return shadowCopiedPlug;
        }

        /// <summary>
        /// Returns the package folder for the plugin DLL passed in
        /// </summary>
        /// <param name="pluginDll"></param>
        /// <returns>An empty string if the plugin was not found in a pckage folder</returns>
        private static string GetPackageFolderFromPluginDll(FileInfo pluginDll)
        {
            if (!IsPackagePluginBinFolder(pluginDll.Directory))
            {
                throw new DirectoryNotFoundException("The file specified does not exist in the lib folder for a package");
            }
            //we know this folder structure is correct now so return the directory. parent  \bin\..\{PackageName}
            return pluginDll.Directory.Parent.FullName;
        }

        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        internal static bool IsPackagePluginFolder(DirectoryInfo folder)
        {
            if (folder.Parent == null) return false;
            if (folder.Parent.Name != PackagesFolderName) return false;
            return true;
            //return PackageFolderNameHasId(folder.Parent.Name);
        }

        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package or for the core package
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static bool IsPackagePluginBinFolder(DirectoryInfo folder)
        {
            if (folder.Name != "lib") return false;
            if (folder.Parent == null) return false;
            return IsPackagePluginFolder(folder.Parent) || (folder.Parent != null && folder.Parent.Name == "Core");
        }

        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        internal static bool IsPackagePluginBinFolder(string folderPath)
        {
            return IsPackagePluginBinFolder(new DirectoryInfo(folderPath));
        }

    }
}
