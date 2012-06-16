using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Umbraco.Framework
{
    /// <summary>
    /// A Scoped Cache provider which stores items in the HttpContext.Items cache.
    /// </summary>
    public class HttpRequestScopedCache : AbstractScopedCache
    {

        private const string ContextKey = "UmbHttpCtx-";

        #region Overrides of AbstractScopedCache

        public override void AddOrChange(string key, Func<string, object> factory)
        {
            DoAddOrChange(key, factory);
        }

        private object DoAddOrChange(string key, Func<string, object> factory)
        {
            var realKey = ContextKey + key;
            var output = GetFromContext(realKey);
            if (output == null)
            {
                output = factory.Invoke(key);
                if (output != null)
                {
                    HttpContext.Current.Items[realKey] = output;
                    return output;
                }
                return null;
            }
            return output;
        }

        public override object GetOrCreate(string key, Func<object> callback)
        {
            return DoAddOrChange(key, k => callback.Invoke());
        }

        private static object GetFromContext(string key)
        {
            return HttpContext.Current.Items[key];
        }

        public override void ScopeComplete()
        {
            var keysToRemove = new List<string>();
            foreach (var k in HttpContext.Current.Items.Keys.OfType<string>())
            {
                if (k.StartsWith(ContextKey))
                {
                    //dispose and remove it
                    var item = HttpContext.Current.Items[k];
                    if (item != null && item is IDisposable)
                    {
                        ((IDisposable)item).Dispose();
                    }
                    keysToRemove.Add(k);
                }                  
            }
            foreach (var key in keysToRemove)
            {
                HttpContext.Current.Items.Remove(key);
            }
        }

        /// <summary>
        /// Removes any item from the cache that match the regex pattern
        /// </summary>
        /// <param name="pattern"></param>
        public override int InvalidateItems(string pattern)
        {
            var toRemove = (from DictionaryEntry i in HttpContext.Current.Items
                            where i.Key.ToString().StartsWith(ContextKey)
                            let key = i.Key.ToString()
                            where Regex.IsMatch(key.Substring(ContextKey.Length, key.Length - ContextKey.Length), pattern)
                            select i.Key.ToString()).ToList();

            foreach (var i in toRemove)
            {
                HttpContext.Current.Items.Remove(i);
            }
            return toRemove.Count;
        }

        #endregion
    }
}