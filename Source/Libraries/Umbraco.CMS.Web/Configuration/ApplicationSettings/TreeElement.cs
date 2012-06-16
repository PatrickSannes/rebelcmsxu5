using System;
using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public class TreeElement : ConfigurationElement, ITree
    {

        [ConfigurationProperty("controllerType", IsRequired = true)]
        public string ControllerTypeName
        {
            get
            {
                return (string)this["controllerType"];
            }
            set
            {
                this["controllerType"] = value;
            }
        }

        public Type ControllerType
        {
            get { return Type.GetType(ControllerTypeName); }
        }

        [ConfigurationProperty("application", IsRequired = true)]
        public string ApplicationAlias
        {
            get
            {
                return (string)this["application"];
            }
            set
            {
                this["application"] = value;
            }
        }
    }
}
