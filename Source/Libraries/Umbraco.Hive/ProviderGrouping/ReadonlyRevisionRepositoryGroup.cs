using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public class ReadonlyRevisionRepositoryGroup<TFilter, T>
        : AbstractRepositoryGroup, IReadonlyRevisionRepositoryGroup<TFilter, T>
        where T : class, IVersionableEntity
        where TFilter : class, IProviderTypeFilter
    {
        public ReadonlyRevisionRepositoryGroup(IEnumerable<AbstractReadonlyRevisionRepository<T>> childRepositories, Uri idRoot, AbstractScopedCache scopedCache, RepositoryContext hiveContext)
            : base(childRepositories, idRoot, scopedCache, hiveContext)
        {
            ChildSessions = childRepositories;
        }
        
        protected IEnumerable<AbstractReadonlyRevisionRepository<T>> ChildSessions { get; set; }

        protected override void DisposeResources()
        {
            ChildSessions.Dispose();
        }

        public Revision<TEntity> Get<TEntity>(HiveId entityId, HiveId revisionId) where TEntity : class, T
        {
            return ChildSessions.Get<TEntity>(entityId, revisionId, IdRoot);
        }

        public IEnumerable<Revision<TEntity>> GetAll<TEntity>() where TEntity : class, T
        {
            return ChildSessions.GetAll<TEntity>(IdRoot);
        }

        public EntitySnapshot<TEntity> GetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            return ChildSessions.GetLatestSnapshot<TEntity>(hiveId, IdRoot, revisionStatusType);
        }

        public Revision<TEntity> GetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            return ChildSessions.GetLatestRevision<TEntity>(entityId, IdRoot, revisionStatusType);
        }

        public IEnumerable<Revision<TEntity>> GetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            return ChildSessions.GetAll<TEntity>(entityId, IdRoot, revisionStatusType);
        }

        public IEnumerable<Revision<TEntity>> GetLatestRevisions<TEntity>(bool allOrNothing, RevisionStatusType revisionStatusType = null, params HiveId[] entityIds) where TEntity : class, T
        {
            return ChildSessions.GetLatestRevisions<TEntity>(allOrNothing, IdRoot, revisionStatusType, entityIds);
        }
    }
}