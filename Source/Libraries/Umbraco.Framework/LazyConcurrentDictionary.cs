using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Umbraco.Framework
{
    /// <summary>
    /// Represents a concurrent threadsafe dictionary of items supporting lazy-loading of the values.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <remarks></remarks>
    public class LazyConcurrentDictionary<TKey, TValue> : LazyDictionary<TKey, TValue> where TValue : class 
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _inner = new ConcurrentDictionary<TKey, Lazy<TValue>>();

        protected override IDictionary<TKey, Lazy<TValue>> Inner
        {
            get { return _inner; }
        }


        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public override void Add(KeyValuePair<TKey, TValue> item)
        {
            EnsureLazyLoaderCalled();
            _inner.AddOrUpdate(item.Key, new Lazy<TValue>(() => item.Value), (x, y) => new Lazy<TValue>(() => item.Value));
        }

        /// <summary>
        /// Adds an element with the provided key and value loader to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueLoader">The value loader.</param>
        /// <remarks></remarks>
        public override void Add(TKey key, Lazy<TValue> valueLoader)
        {
            _inner.AddOrUpdate(key, valueLoader, (x, y) => valueLoader);
        }

        /// <summary>
        /// Adds an item or updates it if an item with that key already exists in the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="updator">The updator.</param>
        /// <remarks></remarks>
        public override void AddOrUpdate(TKey key, Lazy<TValue> value, Func<TKey, Lazy<TValue>, Lazy<TValue>> updator)
        {
            _inner.AddOrUpdate(key, value, updator);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public override bool Remove(TKey key)
        {
            Lazy<TValue> foundItem;
            return _inner.TryRemove(key, out foundItem);
        }

    }
}