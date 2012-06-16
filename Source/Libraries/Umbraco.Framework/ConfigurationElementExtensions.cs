using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Framework
{
    public static class ConfigurationElementExtensions
    {
        /// <summary>
        /// Used for DeepConfigManager sections to ensure that only sections are returned that belong
        /// to the config file found and does not include 'inherited' sections from the application declared
        /// web.config.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <returns></returns>
        public static IEnumerable<T> OnlyLocalConfig<T>(this ConfigurationElementCollection c)
        {
            return c.Cast<ConfigurationElement>()
                .Where(x => x.ElementInformation.Source != null)
                .Cast<T>();
        } 

    }
}
