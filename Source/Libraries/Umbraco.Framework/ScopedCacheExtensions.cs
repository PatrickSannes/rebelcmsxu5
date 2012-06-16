using System;

namespace Umbraco.Framework
{
    public static class ScopedCacheExtensions
    {
        public static T GetOrCreateTyped<T>(this AbstractScopedCache cache, string key, Func<T> callback) 
        {
            var obj = cache.GetOrCreate(key, () => callback.Invoke());
            return (T)obj;
        }
    }
}