using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;


namespace Umbraco.Framework.Persistence.Model
{
    /// <summary>
    /// Represents a simple 1-dimensional keyed collection of entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityCollection<T> : KeyedCollection<HiveId?, T>, INotifyCollectionChanged
        where T : AbstractEntity, IReferenceByAlias
    {
        public Action OnAdd;
        public Action<T> OnRemove;
        private readonly ReaderWriterLockSlim _addLocker = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _removeLocker = new ReaderWriterLockSlim();

        public EntityCollection()
        {
        }

        public EntityCollection(IEnumerable<T> collection)
        {
            AddRange(collection);
        }

        public T this[string alias]
        {
            get { return this.FirstOrDefault(x => x.Alias.InvariantEquals(alias)); }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) return;
            collection.WhereNotNull().ForEach(Add);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="EntityCollection{T}"/> OR replaces an item if one with the same key already exists.
        /// </summary>
        /// <param name="item">The object to be added to the end of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>. The value can be null for reference types.</param>
        /// <remarks></remarks>
        public new void Add(T item)
        {
            using (new WriteLockDisposable(_addLocker))
            {
                var key = GetKeyForItem(item);
                if (key != null)
                {
                    var exists = this.Contains(key);
                    if (exists)
                    {
                        SetItem(IndexOfKey(key.Value), item);
                        return;
                    }
                }
                base.Add(item);
                OnAdd.IfNotNull(x => x.Invoke());

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
        }

        public new void Remove(HiveId id)
        {
            using (new WriteLockDisposable(_removeLocker))
            {
                var exists = Contains(id);
                if (!exists) return;
                var item = this[id];
                if (base.Remove(id))
                {
                    OnRemove.IfNotNull(x => x.Invoke(item));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
            }
        }

        public new void Remove(T item)
        {
            using (new WriteLockDisposable(_removeLocker))
            {
                if (base.Remove(item))
                {
                    OnRemove.IfNotNull(x => x.Invoke(item));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
            }
        }

        public int IndexOfKey(HiveId key)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].Id == key)
                {
                    return i;
                }
            }
            return -1;
        }

        #region Overrides of KeyedCollection<HiveId,T>

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <returns>
        /// The key for the specified element.
        /// </returns>
        /// <param name="item">The element from which to extract the key.</param>
        protected override HiveId? GetKeyForItem(T item)
        {
            return item.Id.NullIfEmpty();
        }

        #endregion

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }
    }
}