using System;
using System.Configuration;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;

namespace Umbraco.Hive.Configuration
{

    public class PersistenceTypeLoaderElement : ConfigurationElement
    {
        private const string XKeyKey = "key";
        private const string XTypeKey = "type";
        private const string XConfigSectionKey = "providerConfig";


        /// <summary>
        /// This is the unique provider key which is unique to readers and writers. It is used for identifiying a reader or writer
        /// and is used in setting up the providers DemandBuilder
        /// </summary>
        [ConfigurationProperty(XKeyKey, IsKey = true)]
        public string Key
        {
            get { return (string)this[XKeyKey]; }
            set
            {
                this[XKeyKey] = value;                
            }
        }

        /// <summary>
        /// This is the providers Alias which is common between the providers readers and writers. It is used for creating <see cref="HiveId"/> objects
        /// and for creating configuration sections for providers
        /// </summary>
        public string Alias
        {
            get { return Key.GetProviderAliasFromProviderKey(); }
        }

        [ConfigurationProperty(XTypeKey, IsRequired = true)]
        public string Type
        {
            get { return (string)this[XTypeKey]; }

            set { this[XTypeKey] = value; }
        }

        public string InternalKey { get; set; }

        [ConfigurationProperty(XConfigSectionKey)]
        public string ConfigSectionKey
        {
            get
            {
                return this[XConfigSectionKey] as string;
            }

            set { this[XConfigSectionKey] = value; }
        }

        public ConfigurationSection GetLocalProviderConfig()
        {
            //TODO: Have ConfigurationManager as an injected service
            if (!String.IsNullOrWhiteSpace(ConfigSectionKey))
            {
                var section = ConfigurationManager.GetSection(ConfigSectionKey);
                if (section != null)
                    return section as ConfigurationSection;
            }
            return null;
        }
    }
}