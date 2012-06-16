using System;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public class ApplicationElement : ConfigurationElement, IApplication
    {

        #region IApplication Members
       
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("alias", IsRequired = true, IsKey = true)]
        public string Alias
        {
            get
            {
                return (string)this["alias"];
            }
            set
            {
                //need to validate
                if (!Regex.IsMatch(value, "^[a-zA-Z0-9]*$"))
                {
                    throw new FormatException("Alias can only contain alphanumeric characters");
                }
                this["alias"] = value;
            }
        }

        [ConfigurationProperty("icon", IsRequired = true)]
        public string Icon
        {
            get
            {
                return (string)this["icon"];
            }
            set
            {
                this["icon"] = value;
            }
        }

        [ConfigurationProperty("ordinal", DefaultValue = 0, IsRequired = false)]
        public int Ordinal
        {
            get
            {
                return (int)this["ordinal"];
            }
            set
            {
                this["ordinal"] = value;
            }
        }
        
        #endregion      
    }
}
