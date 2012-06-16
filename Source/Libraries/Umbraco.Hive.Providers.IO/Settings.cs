using System.IO;
using Umbraco.Framework;

namespace Umbraco.Hive.Providers.IO
{
    public sealed class Settings
    {
        public string SupportedExtensions { get; private set; }
        public string AbsoluteRootedPath { get; set; }
        public string ApplicationRelativeRoot { get; private set; }
        public string ExcludedExtensions { get; private set; }
        public string ExcludedDirectories { get; private set; }
        public string RootPublicDomain { get; private set; }

        public string RelationsStoragePath { get { return Path.Combine(AbsoluteRootedPath, "Relations"); } }

        public Settings(string supportedExtensions, 
            string absoluteRootedPath, 
            string applicationRelativeRoot,
            string excludedExtensions,
            string excludedDirectories,
            string rootPublicDomain)
        {
            // The one thing which cannot be blank is the ApplicationRelativeRoot as that gives us the path relative to the application
            Mandate.ParameterNotNullOrEmpty(applicationRelativeRoot, "applicationRelativeRoot");
            ApplicationRelativeRoot = applicationRelativeRoot;

            // The absolute rooted path may be set outside of the constructor e.g. once the Server object is actually available, so we allow it to be blank
            AbsoluteRootedPath = absoluteRootedPath ?? string.Empty;

            SupportedExtensions = supportedExtensions ?? string.Empty;
            ExcludedExtensions = excludedExtensions ?? string.Empty;
            ExcludedDirectories = excludedDirectories ?? string.Empty;
            RootPublicDomain = rootPublicDomain ?? string.Empty;
        }
    }
}