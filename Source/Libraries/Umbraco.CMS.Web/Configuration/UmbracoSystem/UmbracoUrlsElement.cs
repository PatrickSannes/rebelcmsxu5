using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.Configuration.UmbracoSystem
{
    public class UmbracoUrlsElement : ConfigurationElement
    {
        const string RemoveDoubleDashesKey = "removeDoubleDashes";
        [ConfigurationProperty(RemoveDoubleDashesKey, IsRequired = false, DefaultValue = true)]
        public bool RemoveDoubleDashes
        {
            get { return (bool)this[RemoveDoubleDashesKey]; }
            set
            {
                this[RemoveDoubleDashesKey] = value;
            }
        }

        const string StripNonAsciiKey = "stripNonAscii";
        [ConfigurationProperty(StripNonAsciiKey, IsRequired = false, DefaultValue = true)]
        public bool StripNonAscii
        {
            get { return (bool)this[StripNonAsciiKey]; }
            set
            {
                this[StripNonAsciiKey] = value;
            }
        }

        const string UrlEncodeKey = "urlEncode";
        [ConfigurationProperty(UrlEncodeKey, IsRequired = false, DefaultValue = false)]
        public bool UrlEncode
        {
            get { return (bool)this[UrlEncodeKey]; }
            set
            {
                this[UrlEncodeKey] = value;
            }
        }

        [ConfigurationProperty(CharReplacementElementCollection.CollectionXmlKey, IsRequired = true)]
        public CharReplacementElementCollection CharReplacements
        {
            get { return this[CharReplacementElementCollection.CollectionXmlKey] as CharReplacementElementCollection; }
            set { this[CharReplacementElementCollection.CollectionXmlKey] = value; }
        }
    }
}
