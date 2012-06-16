using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public class ApplicationsCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Returns an Application object by alias
        /// </summary>
        /// <param name="appAlias"></param>
        /// <returns></returns>
        public new ApplicationElement this[string appAlias]
        {
            get
            {
                return (ApplicationElement)BaseGet(appAlias);
            }
        }

        #region Overridden methods to define collection
        
        protected override ConfigurationElement CreateNewElement()
        {
            return new ApplicationElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ApplicationElement)element).Alias;
        }

        #endregion


    }
}
