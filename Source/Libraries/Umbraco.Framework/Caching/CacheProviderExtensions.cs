namespace Umbraco.Framework.Caching
{
    using System;

    public static class CacheProviderExtensions
    {
        public static CacheCreationResult<T> GetOrCreate<T>(this AbstractCacheProvider provider, CacheKey key, Func<T> callback)
        {
            return provider.GetOrCreate(key, () => new CacheValueOf<T>(callback.Invoke()));
        }

        //public static object GetWithTypedKey<T>(this AbstractCacheProvider provider, T key)
        //{
        //    return provider.GetValue(new CacheKey<T>(key));
        //}

        //public static T GetTyped<T>(this AbstractCacheProvider provider, CacheKey key)
        //{
        //    var item = provider.Get(key);
        //    if (item != null && item is T)
        //    {
        //        return (T)item;
        //    }
        //    return default(T);
        //}
    }
}