using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Caching
{
    using System.Collections.Concurrent;
    using System.Runtime.Caching;
    using System.Threading;
    using Umbraco.Framework.Diagnostics;

    public class RuntimeCacheProvider : AbstractCacheProvider
    {
        public RuntimeCacheProvider()
        {
            _memoryCache = new MemoryCache("in-memory");
        }

        public RuntimeCacheProvider(ObjectCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        private static readonly ReaderWriterLockSlim StaticLocker = new ReaderWriterLockSlim();
        private static RuntimeCacheProvider _singleInstance;
        public static RuntimeCacheProvider Default
        {
            get
            {
                using (new WriteLockDisposable(StaticLocker))
                {
                    if (_singleInstance == null) _singleInstance = new RuntimeCacheProvider();
                }
                return _singleInstance;
            }
        }

        private ObjectCache _memoryCache;
        private ConcurrentDictionary<string, CacheKey> _keyTracker = new ConcurrentDictionary<string, CacheKey>();

        public override CacheModificationResult AddOrChange<T>(CacheKey key, CacheValueOf<T> cacheObject)
        {
            using (new WriteLockDisposable(ThreadLocker))
            {
                var exists = _memoryCache.GetCacheItem(key) != null;
                var cacheItemPolicy = cacheObject.Policy.ToCacheItemPolicy();

                var entryDate = DateTime.Now;
                var policyExpiry = cacheItemPolicy.AbsoluteExpiration.Subtract(entryDate);
                cacheItemPolicy.RemovedCallback +=
                    arguments =>
                    LogHelper.TraceIfEnabled(
                        GetType(),
                        "Item was removed from cache ({0}). Policy had {1}s to run when entered at {2}. Key: {3}",
                        () => arguments.RemovedReason.ToString(),
                        () => policyExpiry.TotalSeconds.ToString(),
                        () => entryDate.ToString(),
                        () => key);

                _keyTracker.AddOrUpdate(key, key, (existingKey, existingValue) => key);
                if (exists)
                {
                    LogHelper.TraceIfEnabled(GetType(), "Updating item with {0} left to run", () => policyExpiry.TotalSeconds.ToString());
                    _memoryCache.Set(key, cacheObject, cacheItemPolicy);
                    return new CacheModificationResult(true, false);
                }
                var diff = cacheObject.Policy.GetExpiryDate().Subtract(DateTimeOffset.Now);
                LogHelper.TraceIfEnabled(GetType(), "Adding item with {0} left to run", () => policyExpiry.TotalSeconds.ToString());
                _memoryCache.Add(key, cacheObject, cacheItemPolicy);
            }
            return new CacheModificationResult(false, true);
        }

        public override void Clear()
        {
            _memoryCache.Select(x => x.Key).ToArray().ForEach(x => _memoryCache.Remove(x));
            _memoryCache.DisposeIfDisposable();
            _memoryCache = new MemoryCache("in-memory");
        }

        public override bool Remove(CacheKey key)
        {
            CacheKey throwaway = null;
            var keyBeSure = _keyTracker.TryGetValue(key, out throwaway);
            object itemRemoved = _memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out throwaway);

            return itemRemoved != null;
        }

        protected override CacheEntry<T> PerformGet<T>(CacheKey key)
        {
            var item = _memoryCache.Get(key);
            var casted = item as ICacheValueOf<T>;
            return casted != null ? new CacheEntry<T>(key, casted) : null;
        }

        public override IEnumerable<CacheKey<T>> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            return _keyTracker.Keys.Select(key =>
                {
                    var originalString = key;
                    var convertKey = (CacheKey<T>)originalString;
                    return new { Key = originalString, Converted = convertKey };
                })
                    .Where(x => x.Converted != default(CacheKey<T>) && x.Converted.Original != null && predicate.Invoke(x.Converted.Original))
                    .Select(key => key.Converted);
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _memoryCache.DisposeIfDisposable();
        }

        #endregion
    }
}
