namespace Umbraco.Hive.InMemoryProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading;
    using Umbraco.Framework;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Associations;
    using Umbraco.Framework.Persistence.Model.Versioning;

    public class CacheHelper : DisposableObject
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public CacheHelper(ObjectCache cacheReference)
        {
            Cache = cacheReference;
        }

        public ObjectCache Cache { get; private set; }

        public ISet<IRelationById> GetRelationsTable()
        {
            using (new WriteLockDisposable(_locker))
            {
                var table = (HashSet<IRelationById>)Cache["relations"] ?? new HashSet<IRelationById>();
                Cache.Add("relations", table, new CacheItemPolicy() { Priority = CacheItemPriority.NotRemovable });
                return table;
            }
        }

        public IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] id)
            where T : IReferenceByHiveId
        {
            var getValuesFromCache = Cache.Select(x => x.Value);
            var filteredValues = getValuesFromCache.OfType<T>().Where(x => id.Contains(x.Id, new HiveIdComparer(true)));
            return filteredValues;
        }

        public Revision<TEntity> PerformGet<TEntity>(HiveId entityId, HiveId revisionId)
            where TEntity : class, IVersionableEntity
        {
            var getValuesFromCache = Cache.Select(x => x.Value);
            var filteredValues = getValuesFromCache
                .OfType<Revision<TEntity>>().Where(
                    x =>
                    x.Item.Id.EqualsIgnoringProviderId(entityId) && x.MetaData.Id.EqualsIgnoringProviderId(revisionId))
                .FirstOrDefault();
            return filteredValues;
        }

        public IEnumerable<Revision<TEntity>> PerformGetAllRevisions<TEntity>()
            where TEntity : class, IVersionableEntity
        {
            return Cache.Select(x => x.Value).OfType<Revision<TEntity>>();
        }

        public IEnumerable<T> PerformGetAll<T>()
        {
            var enumerable = Cache.Select(x => x.Value).ToArray();
            return enumerable.OfType<T>();
        }

        public void PerformAdd<TEntity>(Revision<TEntity> revision)
            where TEntity : class, IVersionableEntity
        {
            Mandate.ParameterNotNull(revision, "revision");

            if (!revision.MetaData.Id.IsNullValueOrEmpty())
            {
                AddToCache(revision);
            }

            if (!revision.Item.Id.IsNullValueOrEmpty())
            {
                // Also add the item to the cache without its revision
                PerformAddOrUpdate(revision.Item);
            }
        }

        public IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            return
                GetRelationsTable().Where(
                    x =>
                    x.SourceId.EqualsIgnoringProviderId(parentId) &&
                    (relationType == null || x.Type.RelationName == relationType.RelationName))
                    .Distinct();
        }

        public IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return
                GetRelationsTable().Where(
                    x =>
                    x.DestinationId.EqualsIgnoringProviderId(childId) &&
                    (relationType == null || x.Type.RelationName == relationType.RelationName))
                    .Distinct();
        }

        public bool Exists<TEntity>(HiveId id) where TEntity : IReferenceByHiveId
        {
            return Cache.Select(x => x.Value).OfType<TEntity>().Any(x => x.Id == id);
        }

        public void PerformAddOrUpdate(IReferenceByHiveId entity)
        {
            Mandate.ParameterNotNull(entity, "entity");
            if (!entity.Id.IsNullValueOrEmpty())
                AddToCache(entity);
        }

        public void PerformDelete<T>(HiveId id)
        {
            // Delete any relations to this id
            GetRelationsTable().RemoveAll(x => x.SourceId.EqualsIgnoringProviderId(id) || x.DestinationId.EqualsIgnoringProviderId(id));
            // Delete any revisions of this id
            var revisions = PerformGetAllRevisions<TypedEntity>()
                .Where(x => x.Item.Id.EqualsIgnoringProviderId(id));
            foreach (var revision in revisions)
            {
                Cache.Remove(revision.MetaData.Id.ToString());
            }
            Cache.Remove(id.ToString());
        }

        public IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return GetRelationsTable().Where(
                x => x.SourceId.EqualsIgnoringProviderId(sourceId) &&
                     x.DestinationId.EqualsIgnoringProviderId(destinationId) &&
                     x.Type.RelationName == relationType.RelationName).FirstOrDefault();
        }

        public IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            return
                PerformGetParentRelations(descendentId, relationType)
                    .SelectRecursive(x => PerformGetParentRelations(x.DestinationId, relationType));
        }

        public IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            var childRelations = PerformGetChildRelations(ancestorId, relationType).ToArray();
            return childRelations.SelectRecursive(x =>
            {
                var childRelationsSub = PerformGetChildRelations(x.DestinationId, relationType).ToArray();
                return childRelationsSub;
            });
        }

        public void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            GetRelationsTable().Add(item);
        }

        public void RemoveRelation(IRelationById item)
        {
            var relationByIds = GetRelationsTable();
            var countBefore = relationByIds.Count;
            relationByIds.RemoveAll(item.Equals);
            var success = countBefore != relationByIds.Count;
        }

        public void AddToCache<T>(T persistedEntity)
            where T : IReferenceByHiveId
        {
            using (new WriteLockDisposable(_locker))
            {
                string cacheKey = persistedEntity.Id.ToString();

                if (Cache.Contains(cacheKey))
                {
                    Cache.Remove(cacheKey);
                }
                Cache.Add(cacheKey, persistedEntity, new CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }
        }

        public void AddToCache<T>(Revision<T> persistedEntity)
            where T : class, IVersionableEntity
        {
            using (new WriteLockDisposable(_locker))
            {
                string cacheKey = persistedEntity.MetaData.Id.ToString();

                if (Cache.Contains(cacheKey))
                {
                    Cache.Remove(cacheKey);
                }
                Cache.Add(cacheKey, persistedEntity, new CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            if (Cache == null) return;
            var disposable = Cache as IDisposable; // E.g. MemoryCache implements IDisposable but base ObjectCache does not
            if (disposable != null) disposable.Dispose();
            else
            {
                var items = Cache.ToArray();
                items.ForEach(x => Cache.Remove(x.Key));
            }
        }
    }
}