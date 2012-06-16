using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Umbraco.Framework
{
    public static class EnumHelper
    {

        /// <summary>
        /// Returns an array of string names from the enum
        /// </summary>
        /// <returns></returns>
        public static string[] GetNames<T>()
        {
            return GetNames(typeof (T));
        }

        public static string[] GetNames(Type enumType)
        {
            if (!(typeof(Enum).IsAssignableFrom(enumType)))
                throw new InvalidOperationException("T can only be of type Enum");

            return Enum.GetValues(enumType).Cast<Enum>().Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// Returns an array of string names from the enum which takes into account any Display attributes assigned to the enum fields
        /// </summary>
        /// <returns></returns>
        public static string[] GetDisplayNames<T>()
        {
            return GetDisplayNames(typeof (T));
        }

        public static string[] GetDisplayNames(Type enumType)
        {
            if (!(typeof(Enum).IsAssignableFrom(enumType)))
                throw new InvalidOperationException("T can only be of type Enum");

            var labels = new List<string>();
            foreach (var item in Enum.GetValues(enumType).Cast<Enum>())
            {
                var label = GetStringValue(item);
                labels.Add(label ?? item.ToString());
            }
            return labels.ToArray();
        }

        /// <summary>
        /// Returns a list of a Tuple representing the Enum: Value (int), Name, Display Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Tuple<long, string, string>> GetDisplayNameValueCollection<T>()
        {
            return GetDisplayNameValueCollection(typeof (T));
        }

        public static IEnumerable<Tuple<long, string, string>> GetDisplayNameValueCollection(Type enumType)
        {
            if (!(typeof(Enum).IsAssignableFrom(enumType)))
                throw new InvalidOperationException("T can only be of type Enum");

            var nameValues = new List<Tuple<long, string, string>>();
            foreach (var item in Enum.GetValues(enumType).Cast<Enum>())
            {
                var label = GetStringValue(item);
                nameValues.Add(new Tuple<long, string, string>(Convert.ToInt64(item), item.ToString(), label ?? item.ToString()));
            }
            return nameValues.ToArray();
        }

        private static string GetStringValue(this Enum value)
        {
            // Get the type
            var type = value.GetType();

            // Get fieldinfo for this type
            var fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            var attribs = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            
            if (attribs == null)
                return null;

            // Return the first if there was a match.
            return attribs.Any() ? attribs.First().Name : null;
        }
    }
}
