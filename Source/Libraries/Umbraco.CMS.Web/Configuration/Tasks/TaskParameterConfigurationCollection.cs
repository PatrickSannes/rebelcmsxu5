using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.Tasks
{
    public class TaskParameterConfigurationCollection : ConfigurationElementCollection
    {

        #region Overridden methods to define collection


        protected override ConfigurationElement CreateNewElement()
        {
            return new TaskParameterElement();
        }
        
        /// <summary>
        /// Returns the key for this collection which is a combination of the controller type and the application alias
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TaskParameterElement) element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return "Parameter";
            }
        }

        #endregion

    }
}