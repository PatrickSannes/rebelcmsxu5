using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Mvc;

namespace Umbraco.Cms.Web
{
    public static class ValueProviderExtensions
    {

     

        /// <summary>
        /// Converts a dictionary to an IValueProvider
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static IValueProvider ToValueProvider(this IDictionary<string, string> dictionary)
        {
            var collection = new NameValueCollection();
            foreach (var f in dictionary)
            {
                collection.Add(f.Key, f.Value);
            }
            var valProvider = new NameValueCollectionValueProvider(collection, CultureInfo.InvariantCulture);
            return valProvider;  
        }
    }
}