using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Hive.ProviderSupport
{
    using System;
    using Umbraco.Framework.Caching;
    using Umbraco.Framework.Data;
    using Umbraco.Hive.Caching;

    public abstract class AbstractReadonlySchemaRepository : AbstractProviderRepository, IReadonlyProviderRepository<AbstractSchemaPart>
    {
        protected AbstractReadonlySchemaRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext)
            : this(providerMetadata, new NullProviderRevisionRepository<EntitySchema>(providerMetadata, frameworkContext), frameworkContext)
        { }

        protected AbstractReadonlySchemaRepository(ProviderMetadata providerMetadata, AbstractReadonlyRevisionRepository<EntitySchema> revisions, IFrameworkContext frameworkContext)
            : base(providerMetadata, frameworkContext)
        {
            Revisions = revisions;
            EntitiesAddedOrUpdated = new HashSet<AbstractSchemaPart>();
        }

        protected internal HashSet<AbstractSchemaPart> EntitiesAddedOrUpdated { get; set; }

        public AbstractReadonlyRevisionRepository<EntitySchema> Revisions { get; set; }

        protected abstract IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids) where T : AbstractSchemaPart;
        public IEnumerable<T> Get<T>(bool allOrNothing, params HiveId[] ids) where T : AbstractSchemaPart
        {
            var performGet = PerformGet<T>(allOrNothing, ids) ?? Enumerable.Empty<T>();

            // Enumerate the sequence once
            var all = performGet.ToArray();

            // Operate on a subset of the same array that we will then return
            all
                .Where(x => typeof(IRelatableEntity).IsAssignableFrom(x.GetType())).ToArray()
                .ForEach(x => this.SetRelationProxyLazyLoadDelegate((IRelatableEntity)x));

            // Return the total set manipulating their id beforehand
            return all.Select(x => ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, x));
        }

        public abstract IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType);
        public IRelationById FindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            var findRelation = PerformFindRelation(sourceId, destinationId, relationType);
            return findRelation == null ? null : this.ProcessRelations(Enumerable.Repeat(findRelation, 1), this.ProviderMetadata).FirstOrDefault();
        }

        public abstract IEnumerable<TEntity> PerformGetAll<TEntity>() where TEntity : AbstractSchemaPart;
        public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : AbstractSchemaPart
        {
            var performGetAll = PerformGetAll<TEntity>() ?? Enumerable.Empty<TEntity>();

            // Enumerate the sequence once
            var all = performGetAll.ToArray();

            // Operate on a subset of the same array that we will then return
            all
                .WhereNotNull()
                .Where(x => typeof(IRelatableEntity).IsAssignableFrom(x.GetType())).ToArray()
                .ForEach(x => this.SetRelationProxyLazyLoadDelegate((IRelatableEntity)x));

            // Return the total set manipulating their id beforehand
            return all.Select(x => ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, x));
        }

        public abstract bool CanReadRelations { get; }

        public abstract IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null);
        public IEnumerable<IRelationById> GetParentRelations(HiveId childId, RelationType relationType = null)
        {
            var key = CacheKey.Create(new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Schema, childId, Direction.Parents, relationType));
            Func<List<IRelationById>> execution = () =>
                {
                    var performGetRelations = PerformGetParentRelations(childId, relationType) ?? Enumerable.Empty<IRelationById>();
                    return performGetRelations.ToList();
                };
            var items = (ContextCacheAvailable()) ? HiveContext.GenerationScopedCache.GetOrCreate(key, execution) : null;
            var results = items != null ? items.Value.Item : execution.Invoke();
            return this.ProcessRelations(results, ProviderMetadata);
        }

        public abstract IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null);
        public IEnumerable<IRelationById> GetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            //var key = CacheKey.Create(new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Schema, descendentId, Direction.Ancestors, relationType));
            //var items = HiveContext.GenerationScopedCache.GetOrCreate(key, () =>
            //    {
            //        var performGetRelations = PerformGetAncestorRelations(descendentId, relationType) ?? Enumerable.Empty<IRelationById>();
            //        return performGetRelations.ToList();
            //    });
            //return this.ProcessRelations(items.Value.Item, ProviderMetadata);
            var performGetRelations = PerformGetAncestorRelations(descendentId, relationType) ?? Enumerable.Empty<IRelationById>();
            return this.ProcessRelations(performGetRelations.ToList(), ProviderMetadata);
        }

        public abstract IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null);
        public IEnumerable<IRelationById> GetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            var performGetRelations = PerformGetDescendentRelations(ancestorId, relationType);
            return this.ProcessRelations(performGetRelations, this.ProviderMetadata);
        }

        public abstract IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null);
        public IEnumerable<IRelationById> GetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            var performGetRelations = PerformGetChildRelations(parentId, relationType);
            return this.ProcessRelations(performGetRelations, this.ProviderMetadata);
        }

        public virtual IEnumerable<IRelationById> PerformGetBranchRelations(HiveId siblingId, RelationType relationType = null)
        {
            var parentRelation = GetParentRelations(siblingId, relationType).FirstOrDefault();
            if (parentRelation == null) return Enumerable.Empty<IRelationById>();
            return GetChildRelations(parentRelation.SourceId, relationType);
        }

        public IEnumerable<IRelationById> GetBranchRelations(HiveId siblingId, RelationType relationType = null)
        {
            var performGetRelations = PerformGetBranchRelations(siblingId, relationType);
            return this.ProcessRelations(performGetRelations, this.ProviderMetadata);
        }

        public abstract bool Exists<TEntity>(HiveId id) where TEntity : AbstractSchemaPart;
    }
}