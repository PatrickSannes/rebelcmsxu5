using System;
using System.Configuration;

namespace Umbraco.Hive.Configuration
{
    public class UriMatchElement : ConfigurationElement
    {
        private const string XUriKey = "uri";

        [ConfigurationProperty(XUriKey, IsKey = true)]
        public string UriPattern
        {
            get { return (string)this[XUriKey]; }

            set { this[XUriKey] = value; }
        }
    }
}