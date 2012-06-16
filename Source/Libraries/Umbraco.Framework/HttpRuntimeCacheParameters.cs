using System;
using System.Web.Caching;

namespace Umbraco.Framework
{
    /// <summary>
    /// Parameters used with the AbstractApplicationCache
    /// </summary>
    public class HttpRuntimeCacheParameters<T>
    {
        /// <summary>
        /// Creates a default HttpRuntimeCacheParameters object with no dependencies, callbacks and with sliding expiration of 30 mins
        /// </summary>
        /// <param name="value"></param>
        public HttpRuntimeCacheParameters(T value)
        {
            Value = value;
            Dependencies = null;
            AbsoluteExpiration = Cache.NoAbsoluteExpiration;
            SlidingExpiration = new TimeSpan(0, 0, 30, 0);
            CacheItemPriority = CacheItemPriority.Default;
            OnRemoved = null;
        }

        public T Value { get; private set; }
        public CacheDependency Dependencies { get; set; }

        /// <summary>
        /// If you are using SlidingExpiration this should be set to Cache.NoAbsoluteExpiration
        /// </summary>
        public DateTime AbsoluteExpiration { get; set; }

        /// <summary>
        /// If you are using AbsoluteExpiration this should be set to Cache.NoSlidingExpiration
        /// </summary>
        public TimeSpan SlidingExpiration { get; set; }
        public CacheItemPriority CacheItemPriority { get; set; }
        public CacheItemRemovedCallback OnRemoved { get; set; }

    }
}