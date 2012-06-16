using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Umbraco.Framework.Persistence.Model.Associations._Revised
{
    /// <summary>
    /// An enumerable sequence of <see cref="RelationProxy"/> objects. Supports enlisting manual relation proxies which should then be 
    /// considered for persistence if the <see cref="IRelatableEntity"/> which "owns" this collection is subsequently saved to a repository.
    /// Does not support removing or changing relations; these actions are to be done on the repository service.
    /// </summary>
    public class RelationProxyCollection : IEnumerable<RelationProxy>
    {
        private readonly IRelatableEntity _collectionOwner;

        private readonly HashSet<RelationProxy> _innerParents = new HashSet<RelationProxy>();
        private readonly HashSet<RelationProxy> _innerChildren = new HashSet<RelationProxy>();

        private bool _lazyLoadDelegateCalled = false;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        /// <summary>
        /// Gets or sets the lazy load delegate.
        /// </summary>
        /// <value>The lazy load delegate.</value>
        public Func<HiveId, RelationProxyBucket> LazyLoadDelegate { get; set; } 

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationProxyCollection"/> class.
        /// </summary>
        /// <param name="collectionOwner">The collection owner.</param>
        public RelationProxyCollection(IRelatableEntity collectionOwner)
        {
            _collectionOwner = collectionOwner;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is connected to its datasource and can load relations on-demand
        /// using the <see cref="LazyLoadDelegate"/>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get { return LazyLoadDelegate != null; }
        }

        /// <summary>
        /// Ensures the lazy loader is called, and does nothing if it already has been.
        /// </summary>
        protected void EnsureLazyLoaderCalled()
        {
            if (_lazyLoadDelegateCalled || !IsConnected) return;

            try
            {
                var items = LazyLoadDelegate.Invoke(_collectionOwner.Id);

                AddToParentsWithLock(items.Parents.Select(x =>
                    {
                        var relation = new Relation(x.Type, x.SourceId, x.DestinationId, x.Ordinal, x.MetaData.ToArray());
                        return new RelationProxy(relation, RelationProxyStatus.AutoLoaded);
                    }));

                AddToChildrenWithLock(items.Children.Select(x =>
                    {
                        var relation = new Relation(x.Type, x.SourceId, x.DestinationId, x.Ordinal, x.MetaData.ToArray());
                        return new RelationProxy(relation, RelationProxyStatus.AutoLoaded);
                    }));
            }
            finally
            {
                _lazyLoadDelegateCalled = true;
            }
        }

        private void AddToParentsWithLock(RelationProxy relationProxy)
        {
            AddToParentsWithLock(new[] {relationProxy});
        }

        private void AddToChildrenWithLock(RelationProxy relationProxy)
        {
            AddToChildrenWithLock(new[] { relationProxy });
        }

        /// <summary>
        /// Adds the range of <see cref="RelationProxy"/> items, first obtaining a lock.
        /// </summary>
        /// <param name="relationProxy">The relation proxy.</param>
        private void AddToParentsWithLock(IEnumerable<RelationProxy> relationProxy)
        {
            using (new WriteLockDisposable(_locker))
            {
                relationProxy.ForEach(newItem =>
                    {
                        CheckCyclicRelation(newItem);
                        AddRangeWithoutLock(newItem, _innerParents);
                    });
            }
        }

        /// <summary>
        /// Adds the range of <see cref="RelationProxy"/> items, first obtaining a lock.
        /// </summary>
        /// <param name="relationProxy">The relation proxy.</param>
        private void AddToChildrenWithLock(IEnumerable<RelationProxy> relationProxy)
        {
            using (new WriteLockDisposable(_locker))
            {
                relationProxy.ForEach(newItem =>
                    {
                        CheckCyclicRelation(newItem);
                        AddRangeWithoutLock(newItem, _innerChildren);
                    });
            }
        }

        private void CheckCyclicRelation(RelationProxy newItem)
        {
            if (WouldCauseCyclicRelation(newItem, _innerChildren) || WouldCauseCyclicRelation(newItem, _innerParents))
            {
                throw new InvalidOperationException(
                    "Adding item {0} > {1} > {2} would cause a cyclic relation"
                        .InvariantFormat(newItem.Item.SourceId, newItem.Item.Type.RelationName, newItem.Item.DestinationId));
            }
        }

        private static void AddRangeWithoutLock(RelationProxy newItem, ISet<RelationProxy> relationProxies)
        {
            var existing = relationProxies.Where(y => y.Item.Equals(newItem.Item)).FirstOrDefault();
            if (existing != null)
            {
                if (newItem.Status == RelationProxyStatus.AutoLoaded
                    && existing.Status == RelationProxyStatus.ManuallyAdded)
                {
                    // If we're being told to add because the item was already
                    // available for autoloading, replace existing
                    relationProxies.Remove(existing);
                }
                else return;
            }
            if (newItem != existing) relationProxies.Add(newItem);
        }

        private static bool WouldCauseCyclicRelation(RelationProxy incomingProxy, IEnumerable<RelationProxy> relationProxies)
        {
            return relationProxies.Any(existing => CompareOppositeOfRelation(incomingProxy.Item, existing.Item));
        }

        private static bool CompareOppositeOfRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> left, IReadonlyRelation<IRelatableEntity, IRelatableEntity> right)
        {
            return
                ((right.SourceId == left.DestinationId &&
                  !right.SourceId.IsNullValueOrEmpty()) ||
                 right.Source != null && right.Source == left.Destination)
                &&
                ((right.DestinationId == left.SourceId &&
                  !right.DestinationId.IsNullValueOrEmpty()) ||
                 right.Destination != null && right.Destination == left.Source)
                && right.Type == left.Type;
        }

        /// <summary>
        /// Enlists an <see cref="IRelatableEntity"/> in a <see cref="RelationProxy"/> as a parent of the <see cref="IRelatableEntity"/> which owns this collection.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="type">The type.</param>
        /// <param name="metaData">The meta data.</param>
        public void EnlistParent(IRelatableEntity parent, AbstractRelationType type, params RelationMetaDatum[] metaData)
        {
            EnlistParent(parent, type, 0, metaData);
        }

        /// <summary>
        /// Enlists an <see cref="IRelatableEntity"/> in a <see cref="RelationProxy"/> as a parent of the <see cref="IRelatableEntity"/> which owns this collection.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="type">The type.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="metaData">The meta data.</param>
        public void EnlistParent(IRelatableEntity parent, AbstractRelationType type, int ordinal, params RelationMetaDatum[] metaData)
        {
            var newRelation = new Relation(type, parent, _collectionOwner, ordinal, metaData);
            AddToParentsWithLock(new RelationProxy(newRelation, RelationProxyStatus.ManuallyAdded));
        }

        /// <summary>
        /// Enlists the id of an <see cref="IRelatableEntity"/> in a <see cref="RelationProxy"/> as a parent of the <see cref="IRelatableEntity"/> which owns this collection.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="type">The type.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="metaData">The meta data.</param>
        public void EnlistParentById(HiveId parentId, AbstractRelationType type, int ordinal = 0, params RelationMetaDatum[] metaData)
        {
            Mandate.ParameterNotEmpty(parentId, "parentId");
            var newRelation = new Relation(type, parentId, _collectionOwner, ordinal, metaData);
            AddToParentsWithLock(new RelationProxy(newRelation, RelationProxyStatus.ManuallyAdded));
        }

        /// <summary>
        /// Enlists an <see cref="IRelatableEntity"/> in a <see cref="RelationProxy"/> as a child of the <see cref="IRelatableEntity"/> which owns this collection.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <param name="type">The type.</param>
        /// <param name="metaData">The meta data.</param>
        public void EnlistChild(IRelatableEntity child, AbstractRelationType type, params RelationMetaDatum[] metaData)
        {
            EnlistChild(child, type, 0, metaData);
        }

        /// <summary>
        /// Enlists an <see cref="IRelatableEntity"/> in a <see cref="RelationProxy"/> as a child of the <see cref="IRelatableEntity"/> which owns this collection.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <param name="type">The type.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="metaData">The meta data.</param>
        public void EnlistChild(IRelatableEntity child, AbstractRelationType type, int ordinal, params RelationMetaDatum[] metaData)
        {
            var newRelation = new Relation(type, _collectionOwner, child, ordinal, metaData);
            AddToChildrenWithLock(new RelationProxy(newRelation, RelationProxyStatus.ManuallyAdded));
        }

        /// <summary>
        /// Enlists the id of an <see cref="IRelatableEntity"/> in a <see cref="RelationProxy"/> as a child of the <see cref="IRelatableEntity"/> which owns this collection.
        /// </summary>
        /// <param name="childId">The child id.</param>
        /// <param name="type">The type.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="metaData">The meta data.</param>
        public void EnlistChildById(HiveId childId, AbstractRelationType type, int ordinal, params RelationMetaDatum[] metaData)
        {
            Mandate.ParameterNotEmpty(childId, "childId");
            var newRelation = new Relation(type, _collectionOwner, childId, ordinal, metaData);
            AddToChildrenWithLock(new RelationProxy(newRelation, RelationProxyStatus.ManuallyAdded));
        }

        /// <summary>
        /// Gets the <see cref="RelationProxy"/> items that have been manually enlisted in this collection, and are therefore to be considered for saving
        /// to a datastore.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RelationProxy> GetManualProxies()
        {
            Func<RelationProxy, bool> predicate = x => x.Status == RelationProxyStatus.ManuallyAdded;
            return _innerParents.Where(predicate).Concat(_innerChildren.Where(predicate));
        }

        public IOrderedEnumerable<RelationProxy> AllChildRelations(AbstractRelationType relationType = null)
        {
            EnsureLazyLoaderCalled();
            return _innerChildren.OrderBy(x => x.Item.Ordinal);
        }

        public IOrderedEnumerable<RelationProxy> AllParentRelations(AbstractRelationType relationType = null)
        {
            EnsureLazyLoaderCalled();
            return _innerParents.OrderBy(x => x.Item.Ordinal);
        }

        public IOrderedEnumerable<RelationProxy> GetChildRelations(AbstractRelationType relationType)
        {
            EnsureLazyLoaderCalled();
            return AllChildRelations().Where(x => x.Item.Type.RelationName == relationType.RelationName).OrderBy(x => x.Item.Ordinal);
        }

        public IOrderedEnumerable<RelationProxy> GetParentRelations(AbstractRelationType relationType)
        {
            EnsureLazyLoaderCalled();
            return AllParentRelations().Where(x => x.Item.Type.RelationName == relationType.RelationName).OrderBy(x => x.Item.Ordinal);
        }

        public IEnumerator<RelationProxy> GetEnumerator()
        {
            EnsureLazyLoaderCalled();
            return AllParentRelations().Concat(AllChildRelations()).Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
