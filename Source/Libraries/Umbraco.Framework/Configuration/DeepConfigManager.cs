using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Configuration;
using System.Xml.Linq;
using Umbraco.Framework.Diagnostics;

namespace Umbraco.Framework.Configuration
{
    using System.Threading;

    //public class ConfigInfo<T>
    //{
    //    public DirectoryInfo ConfigFileLocation { get; private set; }
    //}

    /// <summary>
    /// A helper class for returning configuration values by first checking with configuration stored in a subfolder, typically a plugins folder,
    /// and then falling back to the primary application config.
    /// </summary>
    public class DeepConfigManager
    {
        //private readonly HttpContextBase _context;
        private readonly Func<string, string> _mapPathDelegate;

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        private DeepConfigManager(HttpContextBase context)
        {
            Mandate.ParameterNotNull(context, "context");

            _mapPathDelegate = path => context.Server.MapPath(path);
        }

        public DeepConfigManager(Func<string, string> mapPathDelegate)
        {
            Mandate.ParameterNotNull(mapPathDelegate, "mapPathDelegate");

            _mapPathDelegate = mapPathDelegate;
        }

        private static Func<DeepConfigManager> _valueFactory = () => new DeepConfigManager(new HttpContextWrapper(HttpContext.Current));

        private static DeepConfigManager _default = null;
        private static readonly ReaderWriterLockSlim DefaultLocker = new ReaderWriterLockSlim();

        /// <summary>
        /// Gets an instance of the <see cref="DeepConfigManager"/> using the current HttpContext.
        /// </summary>
        public static DeepConfigManager Default
        {
            get
            {
                using (new WriteLockDisposable(DefaultLocker))
                {
                    if (_default == null)
                        _default = DefaultFactory.Invoke();
                }
                return _default;
            }
        }

        public static Func<DeepConfigManager> DefaultFactory
        {
            get
            {
                return _valueFactory;
            }
            set
            {
                _valueFactory = value;
                _default = null;
            }
        }

        ///<summary>
        /// Returns a collection of all connection strings
        ///</summary>
        ///<param name="pluginBasePath"></param>
        ///<returns></returns>
        public IEnumerable<ConnectionStringSettings> GetConnectionStrings(string pluginBasePath)
        {
            //we cannot actually do this:
            // GetWebSettings<ConnectionStringsSection, ConnectionStringSettings>
            // because the child element of ConnectionStringSettings isn't IEnumerable<ConnectionStringSettings> it is actually 
            // ConnectionStringSettingsCollection and we can't return IEnumerable<ConnectionStringSettingsCollections> 
            // so we actually return a collection of ALL ConnectionStringsSection found in the site hierarchy and cast manually

            return GetWebSettings<ConnectionStringsSection, ConnectionStringsSection>("connectionStrings",
                                        x => x, pluginBasePath)
                                        .SelectMany(x => x.ConnectionStrings.Cast<ConnectionStringSettings>());
        }

        /// <summary>
        /// Gets the first configuration setting matching <paramref name="sectionName"/> by calling <see cref="GetWebSettings{TSection,TOut}(string,Func{TSection,TOut},string,string,string)"/>.
        /// See that method for an example. The relative plugin base path in <paramref name="pluginBasePath"/> is mapped to a physical path using the <see cref="System.Web.HttpContextWrapper"/> supplied to this instance
        /// in the constructor.
        /// </summary>
        /// <typeparam name="TSection">The type of the section.</typeparam>
        /// <typeparam name="TOut">The type outputted by the expression in <paramref name="deferred"/>.</typeparam>
        /// <param name="sectionName">Name of the section in the configuration files.</param>
        /// <param name="deferred">The deffered expression.</param>
        /// <param name="pluginBasePath">The relative plugin root path.</param>
        /// <returns></returns>
        public TOut GetFirstWebSetting<TSection, TOut>(string sectionName, Expression<Func<TSection, TOut>> deferred, string pluginBasePath)
            where TSection : ConfigurationSection
            where TOut : class
        {
            Mandate.ParameterNotNullOrEmpty(sectionName, "sectionName");
            Mandate.ParameterNotNull(deferred, "deferred");
            Mandate.ParameterNotNullOrEmpty(pluginBasePath, "pluginBasePath");
            Mandate.ParameterCondition(!Path.IsPathRooted(pluginBasePath), "pluginBasePath");

            var absolutePluginRoot = _mapPathDelegate(pluginBasePath);
            var absoluteRoot = _mapPathDelegate("~/");
            return GetWebSettings(sectionName, deferred, absoluteRoot, absolutePluginRoot, pluginBasePath).FirstOrDefault();
        }

        /// <summary>
        /// Creates a new config file at the specified file path with the basics - appSettings and connectionStrings
        /// </summary>
        /// <param name="toCreate"></param>
        /// <param name="overwriteIfExists"></param>
        /// <returns></returns>
        /// <remarks>
        /// It would be way more awesome to use 'real' configuration but Medium trust is not our friend in this circumstance
        /// </remarks> 
        public static XDocument CreateNewConfigFile(FileInfo toCreate, bool overwriteIfExists)
        {
            Mandate.ParameterNotNull(toCreate, "toCreate");
            Mandate.ParameterCondition(toCreate.Extension == ".config", "toCreate");

            if (overwriteIfExists && toCreate.Exists)
            {
                toCreate.Delete();
            }

            var xml = new XDocument(
                new XElement("configuration",
                    new XElement("configSections"),
                    new XElement("connectionStrings"),
                    new XElement("appSettings")));

            xml.Save(toCreate.FullName);

            return xml;
        }

        /// <summary>
        /// Serializes the provider config section and returns the XElement to which the operation was performed.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="section">The section.</param>
        /// <param name="sectionPath">The section path.</param>
        /// <param name="writeSectionDeclaration">if set to <c>true</c> [write section declaration].</param>
        /// <returns></returns>
        public static XElement SerializeProviderConfigSection(
            XDocument config,
            ConfigurationElement section,
            string sectionPath,
            bool writeSectionDeclaration)
        {
            Mandate.ParameterNotNull(config, "config");
            Mandate.ParameterNotNull(section, "section");
            Mandate.ParameterNotNull(sectionPath, "sectionPath");
            Mandate.ParameterCondition(config.Root != null, "config");
            Mandate.ParameterCondition(config.Root.Elements().Any(x => x.Name == "configSections"), "config");

            var sectionParts = sectionPath.Split('/').ToList();

            if (writeSectionDeclaration)
            {
                //ensure the section group declarations's exist            
                var configSectionsDeclaration = config.Root.Elements("configSections").Single();
                var currGroupDeclaration = configSectionsDeclaration;
                foreach (var part in sectionParts)
                {
                    //if this is the last part, we need to write a section, otherwise write a sectionGroup
                    if (part == sectionParts.Last())
                    {
                        var newSection = new XElement("section",
                                                      new XAttribute("name", part),
                                                      new XAttribute("type", section.GetType().AssemblyQualifiedName),
                                                      new XAttribute("requirePermission", false));
                        currGroupDeclaration.Add(newSection);
                    }
                    else
                    {
                        //if the section group doesn't exist, then create it
                        if (!currGroupDeclaration.Elements("sectionGroup").Where(x => (string)x.Attribute("name") == part).Any())
                        {
                            var newGroup = new XElement("sectionGroup",
                                                        new XAttribute("name", part));
                            currGroupDeclaration.Add(newGroup);
                            currGroupDeclaration = newGroup;
                        }
                        else
                        {
                            currGroupDeclaration = currGroupDeclaration.Elements("sectionGroup").Where(x => (string)x.Attribute("name") == part).First();
                        }
                    }
                }
            }

            //now create the real section
            var currParent = config.Root;
            foreach (var part in sectionParts)
            {
                if (!currParent.Elements(part).Any())
                {
                    var newSection = new XElement(part);
                    currParent.Add(newSection);
                    currParent = newSection;
                }
                else
                {
                    currParent = currParent.Element(part);
                }
            }

            AddPropertiesToElement(section, currParent);

            return currParent;
        }

        public static void AddPropertiesToElement(ConfigurationElement section, XElement currParent)
        {
            foreach (PropertyInformation k in section.ElementInformation.Properties)
            {
                if (k.Name != "")
                {
                    currParent.Add(new XAttribute(k.Name, k.Value));
                }
            }
        }

        /// <summary>
        /// Gets all the configuration settings matching <paramref name="sectionName"/> by calling <see cref="GetWebSettings{TSection,TOut}(string,Func{TSection,TOut},string,string,string)"/>.
        /// See that method for an example.
        /// </summary>
        /// <typeparam name="TSection">The type of the section.</typeparam>
        /// <typeparam name="TOut">The type outputted by the expression in <paramref name="deferred"/>.</typeparam>
        /// <param name="sectionName">Name of the section in the configuration files.</param>
        /// <param name="deferred">The deffered expression.</param>
        /// <param name="pluginBasePath">The relative plugin root path.</param>
        /// <returns></returns>
        public IEnumerable<TOut> GetWebSettings<TSection, TOut>(string sectionName, Expression<Func<TSection, TOut>> deferred, string pluginBasePath) where TSection : ConfigurationSection
        {
            Mandate.ParameterNotNullOrEmpty(sectionName, "sectionName");
            Mandate.ParameterNotNull(deferred, "deferred");
            Mandate.ParameterNotNullOrEmpty(pluginBasePath, "pluginBasePath");
            Mandate.ParameterCondition(!Path.IsPathRooted(pluginBasePath), "pluginBasePath");

            var absolutePluginRoot = _mapPathDelegate(pluginBasePath);
            var absoluteRoot = _mapPathDelegate("~/");
            return GetWebSettings(sectionName, deferred, absoluteRoot, absolutePluginRoot, pluginBasePath);
        }

        /// <summary>
        /// Gets the first discoverable value from the 'appSettings' section of configuration, by calling <see cref="GetWebAppSettings{TOut}(string,string)"/>.
        /// </summary>
        /// <typeparam name="TOut">The type of the value returned.</typeparam>
        /// <param name="key">The key of the setting.</param>
        /// <param name="pluginBasePath">The relative plugin root path.</param>
        /// <returns></returns>
        public TOut GetSingleWebAppSetting<TOut>(string key, string pluginBasePath) where TOut : class
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");
            Mandate.ParameterNotNullOrEmpty(pluginBasePath, "pluginBasePath");
            Mandate.ParameterCondition(!Path.IsPathRooted(pluginBasePath), "pluginBasePath");

            return GetWebAppSettings<TOut>(key, pluginBasePath).FirstOrDefault();
        }

        /// <summary>
        /// Gets all discoverable values from the 'appSettings' section of configuration, by first searching for all consumable configuration files contained in
        /// <paramref name="pluginBasePath"/>, having mapped it to a physical path using the <see cref="System.Web.HttpContextWrapper"/> supplied to this instance
        /// in the constructor.
        /// </summary>
        /// <typeparam name="TOut">The type of the value returned.</typeparam>
        /// <param name="key">The key of the setting.</param>
        /// <param name="pluginBasePath">The relative plugin root path.</param>
        /// <returns></returns>
        public IEnumerable<TOut> GetWebAppSettings<TOut>(string key, string pluginBasePath) where TOut : class
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");
            Mandate.ParameterNotNullOrEmpty(pluginBasePath, "pluginBasePath");
            Mandate.ParameterCondition(!Path.IsPathRooted(pluginBasePath), "pluginBasePath");

            var absolutePluginRoot = _mapPathDelegate(pluginBasePath);
            var absoluteRoot = _mapPathDelegate("~/");
            return GetWebAppSettings<TOut>(key, absoluteRoot, absolutePluginRoot, pluginBasePath);
        }

        /// <summary>
        /// Gets a value from the 'appSettings' section of configuration, by first searching for all consumable configuration files contained in
        /// <paramref name="absolutePluginRoot"/>.
        /// </summary>
        /// <typeparam name="TOut">The type of the value returned.</typeparam>
        /// <param name="key">The key of the setting.</param>
        /// <param name="applicationRoot">The application root path.</param>
        /// <param name="absolutePluginRoot">The absolute plugin root path.</param>
        /// <param name="pluginBasePath">The relative plugin root path.</param>
        /// <returns></returns>
        public IEnumerable<TOut> GetWebAppSettings<TOut>(string key, string applicationRoot, string absolutePluginRoot, string pluginBasePath) where TOut : class
        {
            Mandate.ParameterNotNullOrEmpty(key, "key");
            Mandate.ParameterNotNullOrEmpty(applicationRoot, "applicationRoot");
            Mandate.ParameterNotNullOrEmpty(absolutePluginRoot, "absolutePluginRoot");
            Mandate.ParameterNotNullOrEmpty(pluginBasePath, "pluginBasePath");
            Mandate.ParameterCondition(!Path.IsPathRooted(pluginBasePath), "pluginBasePath");
            Mandate.ParameterCondition(Path.IsPathRooted(applicationRoot), "applicationRoot");
            Mandate.ParameterCondition(Path.IsPathRooted(absolutePluginRoot), "absolutePluginRoot");

            foreach (var searchPath in GetSearchPaths(pluginBasePath, absolutePluginRoot))
            {
                ConfigurationSection section = null;
                NameValueCollection coll = null;

                try
                {
                    // ApplicationSettingsSection has some weirdness where it won't case from the result of GetSection,
                    // the real returntype is KeyValueInternalCollection which is an internal type, however its base
                    // is a public type of NameValueCollection.
                    // We need the section itself for accessing the ElementInformation, but we also need it casted as the collection
                    // for grabbing the values.
                    section = WebConfigurationManager.GetSection("appSettings", searchPath) as ConfigurationSection;
                    coll = (object)section as NameValueCollection;
                }
                catch (Exception)
                {
                    continue;
                }


                if (section != null && coll != null)
                {
                    // By default ASP.NET inherits settings down the config chain.
                    // Since this configuration is purely for overriding in the opposite direction, we specifically don't want to include settings that are 
                    // inherited, since in this loop through sub-config locations we may later move to a setting file which contains a specific, genuine overriding value.
                    if (IsRootConfig(searchPath, section.ElementInformation.Source, applicationRoot))
                    {
                        var inferredAppSettings = coll[key];
                        if (inferredAppSettings != null)
                        {
                            var tryCast = inferredAppSettings as TOut;
                            if (tryCast != null)
                            {
                                yield return tryCast;
                            }
                        }
                    }
                }
            }

            var mainValue = WebConfigurationManager.AppSettings[key] as TOut;
            if (mainValue != null)
                yield return mainValue;
        }

        /// <summary>
        /// Gets all the configuration settings matching <paramref name="sectionName"/> by first searching for all consumable configuration files contained in
        /// <paramref name="absolutePluginRoot"/>. For every configuration found which has a matching section name, yields back the output specified in the provided
        /// delegate <paramref name="deferred"/>.
        /// </summary>
        /// <example>
        /// If you have a custom <see cref="ConfigurationSection"/> called <see cref="FoundationConfigurationSection"/>, you can ask for a specific setting like so:
        /// <code>
        /// var myValue = DeepConfigManager.Default.GetFirstWebSetting&lt;FoundationConfigurationSection, string&gt;("umbraco.foundation", x => x.FoundationSettings.ApplicationTierAlias, "~/App_Plugins");
        /// </code>
        /// This will first scan the ~/App_Plugins folder recursively to find configuraiton settings called 'umbraco.foundation' that are readable as <see cref="FoundationConfigurationSection"/>,
        /// and for all those it finds (including the main app config) it will invoke the delegate provided in <paramref name="deferred"/>.
        /// </example>
        /// <typeparam name="TSection">The type of the section.</typeparam>
        /// <typeparam name="TOut">The type outputted by the expression in <paramref name="deferred"/>.</typeparam>
        /// <param name="sectionName">Name of the section in the configuration files.</param>
        /// <param name="deferred">The deffered expression.</param>
        /// <param name="applicationRoot">The application root path.</param>
        /// <param name="absolutePluginRoot">The absolute plugin root path.</param>
        /// <param name="pluginBasePath">The relative plugin root path.</param>
        /// <returns></returns>
        public IEnumerable<TOut> GetWebSettings<TSection, TOut>(string sectionName, Expression<Func<TSection, TOut>> deferred, string applicationRoot, string absolutePluginRoot, string pluginBasePath) where TSection : ConfigurationSection
        {
            Mandate.ParameterNotNullOrEmpty(sectionName, "sectionName");
            Mandate.ParameterNotNull(deferred, "deferred");
            Mandate.ParameterNotNullOrEmpty(applicationRoot, "applicationRoot");
            Mandate.ParameterNotNullOrEmpty(absolutePluginRoot, "absolutePluginRoot");
            Mandate.ParameterNotNullOrEmpty(pluginBasePath, "pluginBasePath");
            Mandate.ParameterCondition(!Path.IsPathRooted(pluginBasePath), "pluginBasePath");
            Mandate.ParameterCondition(Path.IsPathRooted(applicationRoot), "applicationRoot");
            Mandate.ParameterCondition(Path.IsPathRooted(absolutePluginRoot), "absolutePluginRoot");

            var accessorDelegate = deferred.Compile();

            var isHosted = System.Web.Hosting.HostingEnvironment.IsHosted;
            if (!isHosted)
            {
                LogHelper.Warn<DeepConfigManager>("DeepConfigManager is designed for web applications, outside of a web context all config elements must follow normal configuraiton rules");
            }

            foreach (var searchPath in GetSearchPaths(pluginBasePath, absolutePluginRoot))
            {
                TSection localSection = null;
                try
                {
                    if (isHosted)
                        localSection = WebConfigurationManager.GetSection(sectionName, searchPath) as TSection;
                    else 
                        localSection = ConfigurationManager.GetSection(sectionName) as TSection;
                }
                catch (Exception e)
                {
                    LogHelper.Error<DeepConfigManager>("Error parsing config section in path " + searchPath + ". This config is being skipped.", e);
                    continue;
                }

                // By default ASP.NET inherits settings down the config chain.
                // Since this configuration is purely for overriding in the opposite direction, we specifically don't want to include settings that are 
                // inherited, since in this loop through sub-config locations we may later move to a setting file which contains a specific, genuine overriding value.
                if (localSection != null && !IsRootConfig(searchPath, localSection.ElementInformation.Source, applicationRoot))
                {
                    var val = GetConfigValue(deferred, accessorDelegate, localSection);
                    yield return val;
                }
            }

            // Finally add the app default to the enumerable at the end
            var mainSection = WebConfigurationManager.GetSection(sectionName) as TSection;
            if (mainSection != null)
                yield return accessorDelegate.Invoke(mainSection);
        }

        private static TOut GetConfigValue<TSection, TOut>(Expression<Func<TSection, TOut>> deferred, Func<TSection, TOut> accessorDelegate, TSection localSection)
            where TSection : ConfigurationSection
        {
            var configValue = default(TOut);
            try
            {
                configValue = accessorDelegate.Invoke(localSection);
            }
            catch (NullReferenceException)
            {
                var prop = localSection.GetPropertyInfo(deferred);
                //check if there is a default value                            
                var config = prop.GetCustomAttributes(typeof(ConfigurationPropertyAttribute), false).FirstOrDefault();
                if (config != null)
                {
                    var attr = (ConfigurationPropertyAttribute)config;
                    if (attr.DefaultValue != null)
                    {
                        configValue = (TOut)attr.DefaultValue;
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            return configValue;
        }

        /// <summary>
        /// Checks if the 
        /// </summary>
        /// <param name="searchPath"></param>
        /// <param name="sourceDirectory"></param>
        /// <param name="applicationRoot"></param>
        /// <returns></returns>
        private static bool IsRootConfig(string searchPath, string sourceDirectory, string applicationRoot)
        {
            Mandate.ParameterNotNullOrEmpty(searchPath, "searchPath");
            Mandate.ParameterNotNullOrEmpty(sourceDirectory, "sourceDirectory");
            Mandate.ParameterCondition(Path.IsPathRooted(sourceDirectory), "sourceDirectory");
            Mandate.ParameterCondition(Path.IsPathRooted(applicationRoot), "applicationRoot");

            var directoryName = Path.GetDirectoryName(sourceDirectory);
            if (directoryName.Length < applicationRoot.Length) return false;
            var pathWithRootRemoved = "~/" + NormaliseRelativeRoot(directoryName.Substring(applicationRoot.Length - 1));

            if (pathWithRootRemoved.StartsWith(NormaliseRelativeRoot(searchPath), StringComparison.InvariantCultureIgnoreCase))
            {
                //this is a sub folder of our search path, so we'll assume that it is a local config
                return false;
            }
            else
            {
                //this is not a sub folder or the same folder of our search path so we'll assume that it is a global/root config
                return true;
            }
        }

        private static string NormaliseRelativeRoot(string path)
        {
            Mandate.ParameterNotNullOrEmpty(path, "path");
            path = path.Trim('\\');
            Mandate.ParameterCondition(!Path.IsPathRooted(path), "path");

            return path.Replace('\\', '/').Trim('/');
        }

        /// <summary>
        /// Gets the search paths. Both the relative and the absolute (mapped) plugin roots are required in order to iterate the file system whilst returning a relative path.
        /// </summary>
        /// <remarks>The reason why the params are not "absolute root" and "relative plugin path", for combination in side this method, is in case the plugin path is a virtual directory
        /// that IIS maps to a different physical location via Server.MapPath.</remarks>
        /// <param name="relativePluginRoot">The relative plugin root.</param>
        /// <param name="absolutePluginRoot">The absolute plugin root.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetSearchPaths(string relativePluginRoot, string absolutePluginRoot)
        {
            Mandate.ParameterNotNullOrEmpty(relativePluginRoot, "relativePluginRoot");
            Mandate.ParameterNotNullOrEmpty(absolutePluginRoot, "absolutePluginRoot");
            Mandate.ParameterCondition(Path.IsPathRooted(absolutePluginRoot), "absolutePluginRoot");

            if (!Directory.Exists(absolutePluginRoot))
                yield break;
            else
            {
                foreach (var enumerateDirectory in Directory.EnumerateFiles(absolutePluginRoot, "web.config", SearchOption.AllDirectories))
                {
                    yield return relativePluginRoot + Path.GetDirectoryName(enumerateDirectory).Replace(absolutePluginRoot, "").Replace('\\', '/') + "/";
                }
            }
        }
    }
}
