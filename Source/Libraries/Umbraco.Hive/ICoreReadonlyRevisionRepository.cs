using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Hive
{
    public interface ICoreReadonlyRevisionRepository<in TBaseEntity> 
        : IDisposable 
        where TBaseEntity : class, IVersionableEntity
    {
        Revision<TEntity> Get<TEntity>(HiveId entityId, HiveId revisionId) where TEntity : class, TBaseEntity;
        IEnumerable<Revision<TEntity>> GetAll<TEntity>() where TEntity : class, TBaseEntity;
        EntitySnapshot<TEntity> GetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null) where TEntity : class, TBaseEntity;
        Revision<TEntity> GetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, TBaseEntity;
        IEnumerable<Revision<TEntity>> GetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, TBaseEntity;

        IEnumerable<Revision<TEntity>> GetLatestRevisions<TEntity>(bool allOrNothing, RevisionStatusType revisionStatusType = null, params HiveId[] entityIds) where TEntity : class, TBaseEntity;
    }
}