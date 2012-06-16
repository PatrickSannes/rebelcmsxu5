using System;
using System.Collections;
using System.Collections.Generic;
using Umbraco.Framework.EntityGraph.Domain.Entity;

namespace Umbraco.Framework.EntityGraph.Domain.ObjectModel
{
    /// <summary>
    /// Represents a simple 1-dimensional keyed collection of entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityCollection<T> : IEntityCollection<T> where T : IEntity
    {
        //TODO: Az - This isn't really lazy, since IEntityCollection can't be passed Lazy<T>, so should Lazy be at the IEntityCollection level?
        private LazyMappedIdentifierCollection<T> col;

        public EntityCollection()
        {
            col = new LazyMappedIdentifierCollection<T>();
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            col.Add(item.Id, new Lazy<T>(() => item));
        }

        public void Clear()
        {
            col.Clear();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return col.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}