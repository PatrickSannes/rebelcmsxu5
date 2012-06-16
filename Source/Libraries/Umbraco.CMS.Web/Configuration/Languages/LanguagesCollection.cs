using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.Configuration.Languages
{
    public class LanguagesCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Returns a Language object by ISO Code
        /// </summary>
        /// <param name="isoCode"></param>
        /// <returns></returns>
        public new LanguageElement this[string isoCode]
        {
            get
            {
                return (LanguageElement)BaseGet(isoCode);
            }
        }

        /// <summary>
        /// Creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new LanguageElement();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LanguageElement)element).IsoCode;
        }
    }
}
