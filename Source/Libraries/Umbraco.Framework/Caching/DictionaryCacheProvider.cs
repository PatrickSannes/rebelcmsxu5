using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Caching
{
    using System.Collections.Concurrent;

    public class DictionaryCacheProvider : AbstractCacheProvider
    {
        private readonly ConcurrentDictionary<CacheKey, object> _cacheStore = new ConcurrentDictionary<CacheKey, object>();

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
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

        #region Overrides of AbstractCacheProvider

        public override CacheModificationResult AddOrChange<T>(CacheKey key, CacheValueOf<T> cacheObject)
        {
            CacheModificationResult toReturn = null;
            _cacheStore.AddOrUpdate(
                key,
                keyForAdding =>
                    {
                        toReturn = new CacheModificationResult(false, true);
                        return cacheObject;
                    },
                (keyForUpdating, existing) =>
                    {
                        toReturn = new CacheModificationResult(true, false);
                        return cacheObject;
                    });

            return toReturn;
        }

        public override void Clear()
        {
            _cacheStore.Clear();
        }

        public override bool Remove(CacheKey key)
        {
            object original = null;
            return _cacheStore.TryRemove(key, out original);
        }

        public override IEnumerable<CacheKey<T>> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            foreach (var key in _cacheStore
                .Where(x => x.Key is CacheKey<T>)
                .Select(x => (CacheKey<T>)x.Key)
                .Where(x => x != default(CacheKey<T>) && predicate.Invoke(x.Original)))
            {
                yield return key;
            }
        }

        protected override CacheEntry<T> PerformGet<T>(CacheKey key)
        {
            var item = _cacheStore.ContainsKey(key) ? _cacheStore[key] : null;
            var casted = item as ICacheValueOf<T>;
            return casted != null ? new CacheEntry<T>(key, casted) : null;
        }

        #endregion
    }
}
