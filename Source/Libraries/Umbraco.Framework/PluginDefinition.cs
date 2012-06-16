using System.IO;
using System.Reflection;

namespace Umbraco.Framework
{

    public static class PluginDefinitionExtensions
    {
        
        /// <summary>
        /// Returns true if the plugin definition belongs to a package that is not a core package
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static bool HasRoutablePackageArea(this PluginDefinition def)
        {
            if (def == null)
                return false;
            if (def.IsCorePlugin)
                return false;
            if (def.PackageName.IsNullOrWhiteSpace())
                return false;

            return true;
        }

    }

    /// <summary>
    /// Defines a plugin
    /// </summary>
    public class PluginDefinition
    {
        public PluginDefinition(
            FileInfo originalAssembly, 
            string packageFolderPath, 
            Assembly shadowCopied,
            bool isCorePlugin)
        {
            ReferencedAssembly = shadowCopied;
            IsCorePlugin = isCorePlugin;
            OriginalAssemblyFile = originalAssembly;
            PackageFolderPath = packageFolderPath;
        }

        /// <summary>
        /// The assembly that has been shadow copied that is active in the application
        /// </summary>
        public Assembly ReferencedAssembly { get; internal set; }

        /// <summary>
        /// True if the plugin is a core plugin
        /// </summary>
        public bool IsCorePlugin { get; private set; }

        /// <summary>
        /// The package folder name
        /// </summary>
        public string PackageName
        {
            get
            {
                return Path.GetFileName(PackageFolderPath);
            }
        }

        /// <summary>
        /// The original assembly file that a shadow copy was made from it
        /// </summary>
        public FileInfo OriginalAssemblyFile { get; private set; }

        public string PackageFolderPath { get; private set; }

    }
}