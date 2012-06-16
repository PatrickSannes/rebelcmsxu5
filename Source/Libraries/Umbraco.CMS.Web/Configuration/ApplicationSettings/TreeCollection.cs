using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public class TreeCollection : ConfigurationElementCollection
    {


        #region Overridden methods to define collection

        protected override ConfigurationElement CreateNewElement()
        {
            return new TreeElement();
        }

        /// <summary>
        /// Returns the key for this collection which is a combination of the controller type and the application alias
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return new {Controller = ((TreeElement) element).ControllerTypeName, Application = ((TreeElement) element).ApplicationAlias};
        }

        protected override string ElementName
        {
            get
            {
                return "Tree";
            }
        }

        #endregion
    }
}
