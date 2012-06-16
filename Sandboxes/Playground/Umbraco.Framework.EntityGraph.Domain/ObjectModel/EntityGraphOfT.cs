using System;
using System.Collections;
using System.Collections.Generic;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;

namespace Umbraco.Framework.EntityGraph.Domain.ObjectModel
{
    /// <summary>
    /// Represents a simple 1-dimensional keyed collection of entities
    /// </summary>
    /// <typeparam name="TAllowedType"></typeparam>
    public class EntityGraph<T> : IEntityGraph<T> where T : IEntityVertex
    {
        internal LazyMappedIdentifierCollection<T> collection;

        public EntityGraph()
        {
            collection = new LazyMappedIdentifierCollection<T>();
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in collection)
                yield return item.Value.Value;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IEntityGraph<T> Members

        public void Add(IMappedIdentifier identifier, Func<T> item)
        {
            Add(identifier, new Lazy<T>(item));
        }

        public void Add(IMappedIdentifier identifier, T Item)
        {
            Add(identifier, new Lazy<T>(() => Item));
        }

        public void Add(IMappedIdentifier identifier, Lazy<T> item)
        {
            collection.Add(identifier, item);
        }

        public void Clear()
        {
            collection.Clear();
        }

        public int Count
        {
            get { return collection.Count; }
        }

        #endregion

    }
}