using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Configuration.UmbracoSystem
{
    public class CharReplacementElement : ConfigurationElement
    {
        private const string CharKey = "char";
        private const string ValueKey = "value";

        [ConfigurationProperty(CharKey, IsKey = true, IsRequired = true)]
        public string Char { get { return (string)this[CharKey]; } set { this[CharKey] = value; } }

        [ConfigurationProperty(ValueKey, IsKey = true, IsRequired = true)]
        public string Value { get { return (string)this[ValueKey]; } set { this[ValueKey] = value; } }
    }
}
