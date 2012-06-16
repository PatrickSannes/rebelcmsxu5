namespace Umbraco.Framework.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading;
    using Umbraco.Framework.Diagnostics;

    /// <summary>
    /// A cache provider used for caching data in the current scope. For example, in a web application the current scope is the current request.
    /// </summary>
    public abstract class AbstractCacheProvider : DisposableObject
    {
        protected static readonly ReaderWriterLockSlim ThreadLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public virtual CacheModificationResult AddOrChangeValue(CacheKey key, object cacheObject)
        {
            var value = new CacheValueOf<object>(cacheObject);
            return AddOrChange(key, value);
        }

        public virtual CacheModificationResult AddOrChange(CacheKey key, CacheValueOf<object> cacheObject)
        {
            return AddOrChange<object>(key, cacheObject);
        }

        public abstract CacheModificationResult AddOrChange<T>(CacheKey key, CacheValueOf<T> cacheObject);

        public virtual CacheCreationResult<object> GetOrCreate(CacheKey key, Func<CacheValueOf<object>> callback)
        {
            return GetOrCreate<object>(key, callback);
        }

        public abstract void Clear();

        public abstract bool Remove(CacheKey key);

        public virtual int RemoveWhereKeyMatches<T>(Func<T, bool> matches)
        {
            var removed = 0;
            GetKeysMatching(matches)
                .ToArray() // Ensure sequence is executed once to avoid collection modified errors in inheriting class
                .ForEach(x =>
                {
                    LogHelper.TraceIfEnabled(GetType(), "Removing item due to key delegate, key: {0}", () => x);
                    Remove(x);
                    removed++;
                });
            return removed;
        }

        protected virtual ICacheValueOf<T> EnsureItemExpired<T>(CacheEntry<T> entry)
        {
            if (entry == null) return null;
            var dateTimeOffset = entry.Value.Policy.GetExpiryDate();
            var now = DateTimeOffset.Now;
            if (dateTimeOffset < now)
            {
                LogHelper.TraceIfEnabled(GetType(), "Removing item due to expiry, key: {0}", () => entry.Key);
                Remove(entry.Key);
                return null;
            }
            return entry.Value;
        }

        public abstract IEnumerable<CacheKey<T>> GetKeysMatching<T>(Func<T, bool> predicate);

        protected abstract CacheEntry<T> PerformGet<T>(CacheKey key);

        public ICacheValueOf<object> Get(CacheKey key)
        {
            return Get<object>(key);
        }

        public virtual ICacheValueOf<T> Get<T>(CacheKey key)
        {
            return EnsureItemExpired(PerformGet<T>(key));
        }

        public object GetValue(CacheKey key)
        {
            var value = Get<object>(key);
            return value != null ? value.Item : null;
        }

        public T GetValue<T>(CacheKey key)
        {
            return GetValue(key).SafeCast<T>();
        }

        public virtual CacheCreationResult<T> GetOrCreate<T>(CacheKey key, Func<CacheValueOf<T>> callback)
        {

            var inheritorType = GetType();
            LogHelper.TraceIfEnabled(inheritorType, "In GetOrCreate for {0}", key.ToString);
            using (DisposableTimer.TraceDuration(inheritorType, "In GetOrCreate for " + key.ToString(), "End GetOrCreate"))
            {
                var existing = Get<T>(key);
                if (existing != null)
                {
                    LogHelper.TraceIfEnabled<AbstractCacheProvider>("Item existed");
                    var existingCast = existing as CacheValueOf<T>;
                    if (existingCast != null)
                    {
                        return new CacheCreationResult<T>(false, false, true, existingCast);
                    }
                    return new CacheCreationResult<T>(false, false, true, default(CacheValueOf<T>), true);
                }

                using (new WriteLockDisposable(ThreadLocker))
                {
                    LogHelper.TraceIfEnabled(inheritorType, "Item did not exist");
                    var newValue = callback.Invoke();
                    var result = AddOrChange(key, newValue);
                    return new CacheCreationResult<T>(result.WasUpdated, result.WasInserted, false, newValue);
                }
            }
        }
    }
}