using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

using Umbraco.Framework.Configuration;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Framework.Persistence.NHibernate.Config
{
    public class ProviderConfigurationSection : AbstractProviderConfigurationSection
    {
        private const string XNameApplicationTierAlias = "connectionStringKey";
        private const string XNameNhDriver = "driver";
        private const string XCreateSchema = "autoCreateSchema";
        private const string XSessionContext = "sessionContext";
        private const string XOutputNhMappings = "outputNhMappings";
        private const string XUseNhProfiler = "useNhProf";

        [ConfigurationProperty(XUseNhProfiler, DefaultValue = false)]
        public bool UseNhProf
        {
            get { return (bool)this[XUseNhProfiler]; }

            set { this[XUseNhProfiler] = value; }
        }

        [ConfigurationProperty(XNameApplicationTierAlias)]
        public string ConnectionStringKey
        {
            get { return (string)this[XNameApplicationTierAlias]; }

            set { this[XNameApplicationTierAlias] = value; }
        }

        [ConfigurationProperty(XOutputNhMappings, DefaultValue = false)]
        public bool OutputNhMappings
        {
            get { return (bool)this[XOutputNhMappings]; }

            set { this[XOutputNhMappings] = value; }
        }

        [ConfigurationProperty(XSessionContext, DefaultValue = "web")]
        public string SessionContext
        {
            get { return (string)this[XSessionContext]; }

            set { this[XSessionContext] = value; }
        }

        //[ConfigurationProperty(XCreateSchema, DefaultValue = false)]
        //public bool AutoCreateSchema
        //{
        //    get { return (bool)this[XCreateSchema]; }

        //    set { this[XCreateSchema] = value; }
        //}

        [ConfigurationProperty(XNameNhDriver, IsRequired = true)]
        public SupportedNHDrivers Driver
        {
            get
            {
                SupportedNHDrivers parsed = SupportedNHDrivers.Unknown;
                Enum.TryParse(this[XNameNhDriver].ToString(), true, out parsed);

                return parsed;
            }

            set { this[XNameNhDriver] = value.ToString(); }
        }

        /// <summary>
        /// Gets the driver as string. TODO: This method is included for pre-CTP support in the installer and will be removed.
        /// </summary>
        /// <remarks></remarks>
        public string DriverAsString
        {
            get { return this[XNameNhDriver].ToString(); }
            set { this[XNameNhDriver] = value; }
        }

        public string GetConnectionString()
        {
            if (!string.IsNullOrWhiteSpace(ConnectionStringKey))
            {
                //var connString = ConfigurationManager.ConnectionStrings[ConnectionStringKey];
                var connString = DeepConfigManager.Default.GetConnectionStrings("~/App_Data/Umbraco")
                    .Where(x => x.Name == ConnectionStringKey)
                    .SingleOrDefault();

                if (connString != null)
                    return connString.ConnectionString;
            }

            throw new ConfigurationErrorsException(
                string.Format("Connection string '{0}' not found in application configuration.", ConnectionStringKey));
        }

     
    }
}
