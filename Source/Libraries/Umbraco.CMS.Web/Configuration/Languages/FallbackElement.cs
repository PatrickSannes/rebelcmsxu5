using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.Configuration.Languages
{
    public class FallbackElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the iso code.
        /// </summary>
        /// <value>
        /// The iso code.
        /// </value>
        [ConfigurationProperty("isoCode", IsRequired = true, IsKey = true)]
        public string IsoCode
        {
            get
            {
                return (string)this["isoCode"];
            }
            set
            {
                this["isoCode"] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
