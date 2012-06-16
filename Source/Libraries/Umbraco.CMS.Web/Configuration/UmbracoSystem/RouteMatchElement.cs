using System;
using System.Configuration;
using System.Text.RegularExpressions;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Configuration.UmbracoSystem
{
    public class RouteMatchElement : ConfigurationElement
    {
        private WildcardRegex _regex = null;
        private const string PathXmlKey = "route";
        private const string TypeXmlKey = "type";

        private WildcardRegex Regex
        {
            get
            {
                if (_regex == null && !string.IsNullOrWhiteSpace(Path))
                    _regex = new WildcardRegex(Path, RegexOptions.IgnoreCase);
                return _regex;
            }
        }

        [ConfigurationProperty(PathXmlKey, IsKey = true, IsRequired = true)]
        public string Path { get { return (string)this[PathXmlKey]; } set { this[PathXmlKey] = value; } }

        [ConfigurationProperty(TypeXmlKey, IsKey = true, IsRequired = true)]
        public RouteMatchTypes Type
        {
            get
            {
                RouteMatchTypes type = RouteMatchTypes.Include;
                Enum.TryParse(this[TypeXmlKey].ToString(), true, out type);
                return type;
            }
            set { this[TypeXmlKey] = value.ToString(); }
        }

        /// <summary>
        /// Determines whether the specified absolute path is a match for the path filter on this element.
        /// </summary>
        /// <param name="absolutePath">The absolute path.</param>
        /// <returns>
        /// 	<c>true</c> if the specified absolute path is match; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMatch(string absolutePath)
        {
            return Regex.IsMatch(absolutePath);
        }
    }
}