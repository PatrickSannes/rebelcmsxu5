using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Umbraco.Framework.EntityGraph.Domain.Entity;

namespace Umbraco.Framework.EntityGraph.Domain.EntityAdaptors.ObjectModel
{
    /// <summary>
    /// Represents a simple 1-dimensional keyed collection of entities
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class TypedEntityAdaptorCollection<TEntity> : KeyedCollection<IMappedIdentifier, TEntity>, IEntityCollection<TEntity> where TEntity : ITypedEntity
    {
        #region Overrides of KeyedCollection<IMappedIdentifier,TAllowedType>

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <returns>
        /// The key for the specified element.
        /// </returns>
        /// <param name="item">The element from which to extract the key.</param>
        protected override IMappedIdentifier GetKeyForItem(TEntity item)
        {
            return item.Id;
        }

        #endregion

        #region Implementation of IEnumerable<out KeyValuePair<IMappedIdentifier,TAllowedType>>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="TAllowedType:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<IMappedIdentifier, TEntity>> GetEnumerator()
        {
            return base.Dictionary.GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<IMappedIdentifier,TAllowedType>>

        /// <summary>
        /// Adds an item to the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>.</param><exception cref="TAllowedType:System.NotSupportedException">The <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(KeyValuePair<IMappedIdentifier, TEntity> item)
        {
            base.Add(item.Value);
        }

        /// <summary>
        /// Determines whether the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(KeyValuePair<IMappedIdentifier, TEntity> item)
        {
            return base.Dictionary.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> to an <see cref="TAllowedType:System.Array"/>, starting at a particular <see cref="TAllowedType:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="TAllowedType:System.Array"/> that is the destination of the elements copied from <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>. The <see cref="TAllowedType:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="TAllowedType:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="TAllowedType:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="TAllowedType:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="TAllowedType"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<IMappedIdentifier, TEntity>[] array, int arrayIndex)
        {
            base.CopyTo(array.Select(a => a.Value).ToArray(), arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/>.</param><exception cref="TAllowedType:System.NotSupportedException">The <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(KeyValuePair<IMappedIdentifier, TEntity> item)
        {
            return base.Remove(item.Key);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly { get; private set; }

        #endregion

        #region Implementation of IDictionary<IMappedIdentifier,TAllowedType>

        /// <summary>
        /// Determines whether the <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="TAllowedType:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(IMappedIdentifier key)
        {
            return base.Contains(key);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="TAllowedType:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="TAllowedType:System.ArgumentException">An element with the same key already exists in the <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="TAllowedType:System.NotSupportedException">The <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(IMappedIdentifier key, TEntity value)
        {
            base.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="TAllowedType:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(IMappedIdentifier key, out TEntity value)
        {
            return base.Dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param><exception cref="TAllowedType:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="TAllowedType:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception><exception cref="TAllowedType:System.NotSupportedException">The property is set and the <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public TEntity this[IMappedIdentifier key]
        {
            get { return base[key]; }
            set { base.Dictionary[key] = value; }
        }

        /// <summary>
        /// Gets an <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<IMappedIdentifier> Keys
        {
            get { return base.Dictionary.Keys; }
            private set { return; }
        }

        /// <summary>
        /// Gets an <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="TAllowedType:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="TAllowedType:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TEntity> Values
        {
            get { return base.Dictionary.Values; }
            private set { return; }
        }

        #endregion
    }
}