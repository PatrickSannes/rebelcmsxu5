using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using Umbraco.Framework;


namespace Umbraco.Cms.Web.EmbeddedViewEngine
{
    /// <summary>
    /// helper for embedded compiled view paths
    /// </summary>
    public static class EmbeddedViewPath
    {
        /// <summary>
        /// Dictionary to store view paths and their hashes
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> ViewPaths = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, Assembly> ViewAssemblies = new ConcurrentDictionary<string, Assembly>();
        
        /// <summary>
        /// Returns a compiled view path based on the FQN of the view (embedded resource) 
        /// </summary>
        /// <returns></returns>
        public static string Create(string resourcePathWithAssembly)
        {
            Mandate.ParameterNotNullOrEmpty(resourcePathWithAssembly, "resourcePathWithAssembly");
            Mandate.That<FormatException>(resourcePathWithAssembly.Contains(','));

            var parts = resourcePathWithAssembly.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            return Create(parts[0], parts[1]);
        }

        /// <summary>
        /// Returns a compiled view path based on the FQN of the view (embedded resource) 
        /// </summary>
        /// <param name="resourcePath">
        /// The path to the resource without the assembly name
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly where the resource is found
        /// </param>
        /// <returns></returns>
        public static string Create(string resourcePath, string assemblyName)
        {
            Mandate.ParameterNotNullOrEmpty(resourcePath, "resourcePath");
            Mandate.ParameterNotNullOrEmpty(assemblyName, "assemblyName");
            
            //first check if the resource exists in the values already and return the key
            if (ViewPaths.Values.Contains(resourcePath))
            {
                return ViewPaths
                    .Where(x => x.Value == resourcePath)
                    .Single()
                    .Key;
            }
            else 
            {
                Assembly assembly;
                if (ViewResourceExists(resourcePath, assemblyName, out assembly)) 
                {
                    var md5 = CreateResourcePathHashKey(resourcePath);
                    if (!ViewPaths.ContainsKey(md5))
                    {                      
                        //add the mapping
                        ViewPaths.TryAdd(md5, resourcePath);
                        ViewAssemblies.TryAdd(resourcePath, assembly);                      
                    }
                    return md5;
                }
                
            }
            
            throw new TypeLoadException("Could not find resource with resourcePath: " + resourcePath);
            
        }

        /// <summary>
        /// Returns the Full Virtual file path for the embedded view path
        /// </summary>
        /// <param name="embeddedViewPath">The embedded view path that was created with the Create method</param>
        /// <returns></returns>
        public static string GetFullPath(string embeddedViewPath)
        {
            if (!embeddedViewPath.StartsWith(NamePrefix))
            {
                throw new NotSupportedException("The embeddedViewPath referenced is an invalid format");
            }
            return string.Format("~" + PathPrefix + "{0}.cshtml", embeddedViewPath);
        }

        /// <summary>
        /// Prefix of the MD5 hashed view name so that we can determine that it is in fact a view name made from this class
        /// </summary>
        /// <remarks>
        /// This is used to differentiate between a normal view and a compiled view, therefore it just needs to be a string
        /// that is unique enough that nobody will use naturally when naming a view.
        /// </remarks>
        internal const string NamePrefix = "_EV_";
        internal const string PathPrefix = "/EV.axd/";

        //compiled regex is faster than normal string contains
        private static readonly Regex RegexPrefix = new Regex(PathPrefix, RegexOptions.Compiled);
        private static readonly Regex RegexName = new Regex(NamePrefix, RegexOptions.Compiled);

        /// <summary>
        /// determines if the virtual path is in fact an embedded resource
        /// </summary>
        /// <returns></returns>
        internal static bool IsEmbeddedView(string viewPath)
        {
            Mandate.ParameterNotNullOrEmpty(viewPath, "viewPath");

            if (ViewPaths.ContainsKey(viewPath))
            {
                return true;
            }

            if (RegexPrefix.IsMatch(viewPath) && RegexName.IsMatch(viewPath))
            {
                var hash = GetHashFromPath(viewPath);
                if (!string.IsNullOrEmpty(hash))
                    return ViewPaths.ContainsKey(hash);
                return false;
            }

            return false;
        }

        /// <summary>
        /// /Returns the resource/type FQN of the view resourace based on an md5 hash id
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        /// <remarks>
        /// The only way that a resource will exist based on a hash is if it was created with the Create method.
        /// </remarks>
        internal static string GetViewResourceType(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentNullException("hash");

            return ViewPaths.ContainsKey(hash) ? ViewPaths[hash] : string.Empty;
        }

        internal static string GetViewContent(string viewResourceType)
        {
            if (string.IsNullOrEmpty(viewResourceType))
                throw new ArgumentNullException("viewResourceType");

            string output;
            using (var reader = new StreamReader(Open(viewResourceType)))
                output = reader.ReadToEnd();

            return output;
        }

        /// <summary>
        /// Checks if the resource exists by the path specified
        /// </summary>
        /// <returns></returns>
        private static bool ViewResourceExists(string resourcePath, string assemblyName, out Assembly assembly)
        {            
            assembly = Assembly.Load(assemblyName);
            return assembly.GetManifestResourceInfo(resourcePath) != null;            
        }


        /// <summary>
        /// Returns the MD5 hash of the resource if found in the path
        /// </summary>
        /// <param name="viewPath"></param>
        /// <returns></returns>
        /// <remarks>
        /// need to run some regex because the path will come through as all paths that are registerd with the ViewEngine such as:
        ///     /Areas/Umbraco/Views/Editors/ContentEditor/{_EV_MD5-HASH-OF-EMBEDDED-VIEW}(NAME OF FILE).cshtml
        /// and we need to get the _EV_MD5_HASH
        /// </remarks>
        internal static string GetHashFromPath(string viewPath)
        {
            Mandate.ParameterNotNullOrEmpty(viewPath, "viewPath");

            var match = Regex.Match(viewPath, @"/EV\.axd.*/(" + NamePrefix + @"\w+\(.*\))\.cshtml", RegexOptions.Compiled);
            if (match.Success && match.Groups.Count == 2)
                return match.Groups[1].Value;

            return string.Empty;
        }

        /// <summary>
        /// Opens the resource as as Stream
        /// </summary>
        /// <returns></returns>
        internal static Stream Open(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
                throw new ArgumentNullException("resourcePath");

            var parts = resourcePath.Split(',');
            var assembly = ViewAssemblies[resourcePath];
            return assembly.GetManifestResourceStream(parts[0]);
        }

        /// <summary>
        /// Returns a unique has for the resource path
        /// </summary>
        /// <param name="resPath"></param>
        /// <returns></returns>
        private static string CreateResourcePathHashKey(string resPath)
        {
            // step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(resPath);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            //We'll prefix with _EV_ so we can differentiate a resource path that which contains the key as opposed to a normal URL.
            //it's random enought that nobody is gonna have that :/

            //so that it's readable, we want to suffix the path with the file name part of the resPath which is it's last 2 parts when split by a '.'
            var parts = resPath.Split('.');
            var fileNamePart = "";
            if (parts.Length >= 2 && parts.Last().ToUpper() == "CSHTML")
            {
                fileNamePart = parts[parts.Length - 2];
            }

            return NamePrefix + sb + "(" + fileNamePart + ")";
        }

    }
}
