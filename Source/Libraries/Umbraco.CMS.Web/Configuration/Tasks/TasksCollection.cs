using System;
using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.Tasks
{
    public class TasksCollection : ConfigurationElementCollection
    {
        
        #region Overridden methods to define collection

        protected override ConfigurationElement CreateNewElement()
        {
            return new TaskElement();
        }

        /// <summary>
        /// Returns the key for this collection which is a combination of the controller type and the application alias
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            //allow multiple elements, just means you can't reference by a key
            return Guid.NewGuid();
        }

        protected override string ElementName
        {
            get
            {
                return "Task";
            }
        }

        #endregion
    }
}