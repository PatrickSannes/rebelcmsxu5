using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace UmbracoFramework.EntityGraph.Domain.Entity.Graph
{
    public class EntityAdaptorGraph<T> : IEntityAdaptorGraph<T> where T : ITypedEntity
    {
        private readonly IList<IEntityAdaptorVertex<T>> innerList = new List<IEntityAdaptorVertex<T>>();

        public EntityAdaptorGraph()
        {
        }

        public EntityAdaptorGraph(IEnumerable<IEntityAdaptorVertex<T>> incoming)
        {
            Contract.Requires(incoming != null);
            innerList = incoming.ToList();
        }

        #region Implementation of ICollection<IEntityAdaptorVertex<T>>

        /// <summary>
        ///   Adds an item to the <see cref = "T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name = "item">The object to add to the <see cref = "T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Add(IEntityAdaptorVertex<T> item)
        {
            innerList.Add(item);
        }

        /// <summary>
        ///   Removes all items from the <see cref = "T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
        public void Clear()
        {
            innerList.Clear();
        }

        /// <summary>
        ///   Determines whether the <see cref = "T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>
        ///   true if <paramref name = "item" /> is found in the <see cref = "T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <param name = "item">The object to locate in the <see cref = "T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(IEntityAdaptorVertex<T> item)
        {
            return innerList.Contains(item);
        }

        /// <summary>
        ///   Copies the elements of the <see cref = "T:System.Collections.Generic.ICollection`1" /> to an <see cref = "T:System.Array" />, starting at a particular <see cref = "T:System.Array" /> index.
        /// </summary>
        /// <param name = "array">The one-dimensional <see cref = "T:System.Array" /> that is the destination of the elements copied from <see cref = "T:System.Collections.Generic.ICollection`1" />. The <see cref = "T:System.Array" /> must have zero-based indexing.</param>
        /// <param name = "arrayIndex">The zero-based index in <paramref name = "array" /> at which copying begins.</param>
        /// <exception cref = "T:System.ArgumentNullException"><paramref name = "array" /> is null.</exception>
        /// <exception cref = "T:System.ArgumentOutOfRangeException"><paramref name = "arrayIndex" /> is less than 0.</exception>
        /// <exception cref = "T:System.ArgumentException"><paramref name = "array" /> is multidimensional.-or-The number of elements in the source <see cref = "T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name = "arrayIndex" /> to the end of the destination <paramref name = "array" />.-or-Type <paramref name = "T" /> cannot be cast automatically to the type of the destination <paramref name = "array" />.</exception>
        public void CopyTo(IEntityAdaptorVertex<T>[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///   Removes the first occurrence of a specific object from the <see cref = "T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///   true if <paramref name = "item" /> was successfully removed from the <see cref = "T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name = "item" /> is not found in the original <see cref = "T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <param name = "item">The object to remove from the <see cref = "T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(IEntityAdaptorVertex<T> item)
        {
            return innerList.Remove(item);
        }

        /// <summary>
        ///   Gets the number of elements contained in the <see cref = "T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///   The number of elements contained in the <see cref = "T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public int Count
        {
            get { return innerList.Count; }
        }

        /// <summary>
        ///   Gets a value indicating whether the <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        ///   true if the <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return innerList.IsReadOnly; }
        }

        #endregion

        #region Implementation of IList<IEntityAdaptorVertex<T>>

        /// <summary>
        ///   Determines the index of a specific item in the <see cref = "T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <returns>
        ///   The index of <paramref name = "item" /> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name = "item">The object to locate in the <see cref = "T:System.Collections.Generic.IList`1" />.</param>
        public int IndexOf(IEntityAdaptorVertex<T> item)
        {
            return innerList.IndexOf(item);
        }

        /// <summary>
        ///   Inserts an item to the <see cref = "T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name = "index">The zero-based index at which <paramref name = "item" /> should be inserted.</param>
        /// <param name = "item">The object to insert into the <see cref = "T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref = "T:System.ArgumentOutOfRangeException"><paramref name = "index" /> is not a valid index in the <see cref = "T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public void Insert(int index, IEntityAdaptorVertex<T> item)
        {
            innerList.Insert(index, item);
        }

        /// <summary>
        ///   Removes the <see cref = "T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name = "index">The zero-based index of the item to remove.</param>
        /// <exception cref = "T:System.ArgumentOutOfRangeException"><paramref name = "index" /> is not a valid index in the <see cref = "T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public void RemoveAt(int index)
        {
            innerList.RemoveAt(index);
        }

        /// <summary>
        ///   Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        ///   The element at the specified index.
        /// </returns>
        /// <param name = "index">The zero-based index of the element to get or set.</param>
        /// <exception cref = "T:System.ArgumentOutOfRangeException"><paramref name = "index" /> is not a valid index in the <see cref = "T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref = "T:System.NotSupportedException">The property is set and the <see cref = "T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public IEntityAdaptorVertex<T> this[int index]
        {
            get { return innerList[index]; }
            set { innerList[index] = value; }
        }

        #endregion

        #region Implementation of IEnumerable<out IEntityAdaptorVertex<T>>

        /// <summary>
        ///   Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///   A <see cref = "T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<IEntityAdaptorVertex<T>> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///   An <see cref = "T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion

        [ContractInvariantMethod]
        private void SpecificObjectInvariant()
        {
            Contract.Invariant(innerList != null);
        }
    }
}