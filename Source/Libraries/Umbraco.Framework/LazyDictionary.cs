using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Umbraco.Framework
{
    /// <summary>
    /// Represents a dictionary of items supporting lazy-loading of the values.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LazyDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TValue : class
    {
        private readonly Dictionary<TKey, Lazy<TValue>> _inner = new Dictionary<TKey, Lazy<TValue>>();
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        /// <summary>
        /// Exposes the inner dictionary
        /// </summary>
        protected virtual IDictionary<TKey, Lazy<TValue>> Inner
        {
            get { return _inner; }
        } 

        /// <summary>
        /// Gets or sets the lazy load factory which is responsible for returning elements which should populate this dictionary when
        /// this dictionary is first enumerated.
        /// </summary>
        /// <value>The lazy load factory.</value>
        /// <remarks></remarks>
        public Func<IEnumerable<KeyValuePair<TKey, Lazy<TValue>>>> LazyLoadFactory { get; set; }

        private bool _lazyLoaderCalled = false;
        protected void EnsureLazyLoaderCalled()
        {
            if (_lazyLoaderCalled || LazyLoadFactory == null) return;
            try
            {
                var items = LazyLoadFactory.Invoke();
                items.ForEach(x => this.AddOrUpdate(x.Key, x.Value, (y, z) => x.Value));
            }
            finally
            {
                _lazyLoaderCalled = true;
            }
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection. The delegates provided in order to lazily load the item values
        /// will be invoked.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            EnsureLazyLoaderCalled();
            return Inner.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<TKey,TValue>>

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            EnsureLazyLoaderCalled();
            if (Inner.ContainsKey(item.Key))
            {
                throw new ArgumentException("An element with the same key already exists");
            }
            else
            {
                Inner.Add(item.Key, new Lazy<TValue>(() => item.Value));
            }            
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public virtual void Clear()
        {
            Inner.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            EnsureLazyLoaderCalled();
            return GetLoadedCollection().Contains(item);
        }

        private IDictionary<TKey, TValue> GetLoadedCollection()
        {
            EnsureLazyLoaderCalled();
            return Inner.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).ToDictionary(x => x.Key,
                                                                                                        x => x.Value);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            EnsureLazyLoaderCalled();
            GetLoadedCollection().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get
            {
                EnsureLazyLoaderCalled();
                return Inner.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly { get { return false; } }

        #endregion

        #region Implementation of IDictionary<TKey,TValue>

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(TKey key)
        {
            EnsureLazyLoaderCalled();
            return Inner.ContainsKey(key);
        }

        /// <summary>
        /// Adds an element with the provided key and value loader to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueLoader">The value loader.</param>
        /// <remarks></remarks>
        public virtual void Add(TKey key, Lazy<TValue> valueLoader)
        {
            if (Inner.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists");
            }
            else
            {
                Inner.Add(key, valueLoader);
            }       
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            Add(key, new Lazy<TValue>(() => value));
        }

        /// <summary>
        /// Adds an item or updates it if an item with that key already exists in the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="updator">The updator.</param>
        /// <remarks></remarks>
        public virtual void AddOrUpdate(TKey key, Lazy<TValue> value, Func<TKey, Lazy<TValue>, Lazy<TValue>> updator)
        {
            if (Inner.ContainsKey(key))
            {
                Inner[key] = updator(key, Inner[key]);
            }
            else
            {
                Inner.Add(key, value);
            }   
        }

        /// <summary>
        ///  Adds an item or updates it if an item with that key already exists in the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void AddOrUpdate(TKey key, TValue value)
        {
            AddOrUpdate(key,
                        new Lazy<TValue>(() => value),
                        (k,i) => new Lazy<TValue>(() => value));
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public virtual bool Remove(TKey key)
        {
            return Inner.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            EnsureLazyLoaderCalled();
            Lazy<TValue> foundItem;
            var success = Inner.TryGetValue(key, out foundItem);
            value = foundItem != null ? foundItem.Value : null;
            return success;
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                using (new WriteLockDisposable(_locker))
                {
                    EnsureLazyLoaderCalled();
                    var foundItem = Inner[key];
                    if (foundItem != null)
                        return foundItem.Value;
                }
                return default(TValue);
            }
            set
            {
                EnsureLazyLoaderCalled();
                Inner[key] = new Lazy<TValue>(() => value);
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TKey> Keys { get { return Inner.Keys; } }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TValue> Values { get { return Inner.Values.Select(x => x.Value).ToList(); } }

        #endregion
    }
}