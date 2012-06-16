using System;

namespace Umbraco.Framework
{
    /// <summary>
    /// Represents an interface used for all plugins
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="id"></param>
        public PluginAttribute(string id)
        {            
            Id = Guid.Parse(id);
        }

        public Guid Id { get; private set; }
    }
}
