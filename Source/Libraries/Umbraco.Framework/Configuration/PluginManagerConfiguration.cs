using System.Configuration;

namespace Umbraco.Framework.Configuration
{
    /// <summary>
    /// Configuration 
    /// </summary>
    public class PluginManagerConfiguration : ConfigurationSection
    {
        public const string ConfigXmlKey = "umbraco/pluginManager";

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns></returns>
        public static PluginManagerConfiguration GetSettings()
        {
            var mgr = (PluginManagerConfiguration)ConfigurationManager.GetSection(ConfigXmlKey);
            return mgr;
        }
        
        /// <summary>
        /// The Pluginmanager xml configuration key
        /// </summary>
        public const string PluginManagerXmlKey = "pluginManager";
        private const string ShadowCopyFolderXmlKey = "shadowCopyFolder";
        private const string PluginsFolderXmlKey = "pluginsFolder";

        /// <summary>
        /// Gets or sets the plugins path.
        /// </summary>
        /// <value>
        /// The plugins path.
        /// </value>
        [ConfigurationProperty(PluginsFolderXmlKey, DefaultValue = "~/App_Plugins")]
        public string PluginsPath
        {
            get { return this[PluginsFolderXmlKey].ToString(); }
            set
            {
                this[PluginsFolderXmlKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the shadow copy path.
        /// </summary>
        /// <value>
        /// The shadow copy path.
        /// </value>
        [ConfigurationProperty(ShadowCopyFolderXmlKey, DefaultValue = "~/App_Plugins/bin")]
        public string ShadowCopyPath
        {
            get { return this[ShadowCopyFolderXmlKey].ToString(); }
            set
            {
                this[ShadowCopyFolderXmlKey] = value;
            }
        }
    }
}