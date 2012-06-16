using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;

namespace Umbraco.Framework
{
    public class DictionaryScopedCache : AbstractScopedCache
    {
        private readonly ConcurrentDictionary<string, object> _cacheStore = new ConcurrentDictionary<string, object>();

        #region Overrides of AbstractScopedCache

        public override void AddOrChange(string key, Func<string, object> factory)
        {
            Mandate.ParameterNotNull(factory, "factory");

            _cacheStore.AddOrUpdate(key, factory, (e, k) => factory(e));
        }

        public override object GetOrCreate(string key, Func<object> callback)
        {
            Mandate.ParameterNotNull(callback, "callback");

            var toReturnOuter = _cacheStore.GetOrAdd(key, x =>
                {
                    var toReturn = callback.Invoke();
                    return toReturn;
                });
            return toReturnOuter;
        }

        /// <summary>
        /// Removes any item from the cache that match the regex pattern
        /// </summary>
        /// <param name="pattern"></param>
        public override int InvalidateItems(string pattern)
        {
            var toRemove = (from i in _cacheStore
                            let key = i.Key
                            where Regex.IsMatch(key, pattern)
                            select i.Key).ToList();

            foreach (var i in toRemove)
            {
                object output;
                _cacheStore.TryRemove(i, out output);
            }
            return toRemove.Count;
        }

        public override void ScopeComplete()
        {
            foreach (var item in _cacheStore)
            {
                if (item.Value != null && item.Value is IDisposable)
                {
                    ((IDisposable)item.Value).Dispose();
                }
            }
            _cacheStore.Clear();
        }

        #endregion
    }
}