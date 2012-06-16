using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Hive.ProviderSupport
{
    public abstract class AbstractReadonlyRevisionRepository<T>
        : AbstractProviderRepository, IReadonlyProviderRevisionRepository<T> where T : class, IVersionableEntity
    {
        protected AbstractReadonlyRevisionRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext) 
            : base(providerMetadata, frameworkContext)
        { }

        /// <summary>
        /// Gets the related entities delegate. This is used to provide returned Revisions' entities RelationProxyCollection with a delegate
        /// back to the relevant AbstractEntityRepository that may auto-load relations.
        /// </summary>
        /// <value>The related entities delegate.</value>
        public Func<HiveId, RelationProxyBucket> RelatedEntitiesLoader { get; set; }

        public abstract Revision<TEntity> PerformGet<TEntity>(HiveId entityId, HiveId revisionId) where TEntity : class, T;
        public Revision<TEntity> Get<TEntity>(HiveId entityId, HiveId revisionId) where TEntity : class, T
        {
            return ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, this.SetRelationProxyLazyLoadDelegate(PerformGet<TEntity>(entityId, revisionId)));            
        }

        public abstract IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>() where TEntity : class, T;
        public IEnumerable<Revision<TEntity>> GetAll<TEntity>() where TEntity : class, T
        {
            var performGetAll = PerformGetAll<TEntity>();
            return performGetAll == null && !performGetAll.Any()
                ? Enumerable.Empty<Revision<TEntity>>()
                : performGetAll.Select(x => ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, this.SetRelationProxyLazyLoadDelegate(x)));
        }

        public abstract EntitySnapshot<TEntity> PerformGetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null) where TEntity : class, T;
        public EntitySnapshot<TEntity> GetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            var performGet = PerformGetLatestSnapshot<TEntity>(hiveId, revisionStatusType);
            return ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, this.SetSnapshotProxyLazyLoadDelegate(performGet));            
        }

        public abstract Revision<TEntity> PerformGetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, T;
        public Revision<TEntity> GetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            var performGet = PerformGetLatestRevision<TEntity>(entityId, revisionStatusType);
            return ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, this.SetRelationProxyLazyLoadDelegate(performGet));   
        }

        public virtual IEnumerable<Revision<TEntity>> PerformGetLatestRevisions<TEntity>(bool allOrNothing, RevisionStatusType revisionStatusType = null, params HiveId[] entityIds) where TEntity : class, T
        {
            var revisions = entityIds.Select(x => GetLatestRevision<TEntity>(x, revisionStatusType)).ToArray();
            if (allOrNothing && !revisions.All(x => entityIds.Any(y => y.EqualsIgnoringProviderId(x.Item.Id)))) return Enumerable.Empty<Revision<TEntity>>();
            return revisions;
        }

        public IEnumerable<Revision<TEntity>> GetLatestRevisions<TEntity>(bool allOrNothing, RevisionStatusType revisionStatusType = null, params HiveId[] entityIds) where TEntity : class, T
        {
            var performGet = PerformGetLatestRevisions<TEntity>(allOrNothing, revisionStatusType, entityIds) ?? Enumerable.Empty<Revision<TEntity>>();
            return performGet.Select(x => ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, this.SetRelationProxyLazyLoadDelegate(x)));
        }

        public abstract IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, T;
        public IEnumerable<Revision<TEntity>> GetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            var performGetAll = PerformGetAll<TEntity>(entityId, revisionStatusType);
            return performGetAll == null && !performGetAll.Any()
                ? Enumerable.Empty<Revision<TEntity>>()
                : performGetAll.Select(x => ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, this.SetRelationProxyLazyLoadDelegate(x)));
        }

        public abstract EntitySnapshot<TEntity> GetSnapshot<TEntity>(HiveId hiveId, HiveId revisionId) where TEntity : class, T;
    }
}