using System;

namespace Umbraco.Framework
{
    /// <summary>
    /// A cache provider used for caching data in the current application which persists across scope changes.
    /// </summary>
    public abstract class AbstractApplicationCache : DisposableObject
    {
        /// <summary>
        /// Gets or Creates the cache item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback">Callback to create the cache item if it doesn't exist</param>
        /// <returns></returns>
        public abstract T GetOrCreate<T>(string key, Func<HttpRuntimeCacheParameters<T>> callback);

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns>Returns the number of items removed that matched the pattern</returns>
        public abstract int InvalidateItems(string pattern);
        
        public abstract void ScopeComplete();

        protected override void DisposeResources()
        {
            ScopeComplete();
        }
    }
}