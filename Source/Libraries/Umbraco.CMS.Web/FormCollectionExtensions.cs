using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web
{
    public static class FormCollectionExtensions
    {

        /// <summary>
        /// Converts a dictionary object to a query string representation such as:
        /// firstname=shannon&lastname=deminick
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static string ToQueryString(this FormCollection items)
        {
            if (items == null) return "";
            if (items.Count == 0) return "";

            var builder = new StringBuilder();
            foreach (var i in items.AllKeys)
            {
                builder.Append(string.Format("{0}={1}&", i, items[i]));
            }
            return builder.ToString().TrimEnd('&');
        }

        /// <summary>
        /// Converts the FormCollection to a dictionary
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this FormCollection items)
        {
            return items.AllKeys.ToDictionary<string, string, object>(i => i, i => items[i]);
        }

        ///// <summary>
        ///// Returns the value of an item in the collection based on the key, if not found returns an empty string
        ///// </summary>
        ///// <param name="items"></param>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public static string GetString(this FormCollection items, string key)
        //{
        //    if (string.IsNullOrEmpty(items[key]))
        //        return string.Empty;
        //    return items[key];
        //}

        /// <summary>
        /// Checks if the collection contains the key
        /// </summary>
        /// <param name="items"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasKey(this FormCollection items, string key)
        {
            return !string.IsNullOrEmpty(items[key]);
        }

        /// <summary>
        /// Returns the object based in the collection based on it's key. This does this with a conversion so if it doesn't convert a null object is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetValue<T>(this FormCollection items, string key)
        {
            var val = items.Get(key);
            if (string.IsNullOrEmpty(val)) return default(T);

            var converter = TypeDescriptor.GetConverter(typeof (T));
            if (converter == null) return default(T);
            try
            {
                var converted = (T)converter.ConvertFrom(val);
                return converted;
            }
            catch (NotSupportedException)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Returns the value of a mandatory item in the FormCollection
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static string GetRequiredString(this FormCollection items, string key)
        {
            if (string.IsNullOrEmpty(items[key]))
                throw new ArgumentNullException("The " + key + " query string parameter was not found but is required");
            return items[key];
        }     

    }
}
