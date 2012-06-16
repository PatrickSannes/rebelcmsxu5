using System;


namespace Umbraco.Framework
{

    /// <summary>
    /// A cache provider used for caching data in the current scope. For example, in a web application the current scope is the current request.
    /// </summary>
    public abstract class AbstractScopedCache : DisposableObject
    {
        public abstract void AddOrChange(string key, Func<string, object> factory);

        public abstract object GetOrCreate(string key, Func<object> callback);

        public abstract void ScopeComplete();

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns>Returns the number of items removed that matched the pattern</returns>
        public abstract int InvalidateItems(string pattern);

        protected override void DisposeResources()
        {           
            ScopeComplete();
        }
    }

}