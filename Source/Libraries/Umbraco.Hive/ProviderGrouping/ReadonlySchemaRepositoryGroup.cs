using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public class ReadonlySchemaRepositoryGroup<TFilter>
        : AbstractRepositoryGroup, IReadonlySchemaRepositoryGroup<TFilter>
        where TFilter : class, IProviderTypeFilter
    {
        public ReadonlySchemaRepositoryGroup(IEnumerable<AbstractReadonlySchemaRepository> childRepositories, IEnumerable<AbstractReadonlyRevisionRepository<EntitySchema>> childRevisionSessions, Uri idRoot, AbstractScopedCache scopedCache, RepositoryContext hiveContext)
            : base(childRepositories, idRoot, scopedCache, hiveContext)
        {
            ChildSessions = childRepositories;
            Revisions = new ReadonlyRevisionRepositoryGroup<TFilter, EntitySchema>(childRevisionSessions, IdRoot, scopedCache, hiveContext);
        }
        
        protected IEnumerable<AbstractReadonlySchemaRepository> ChildSessions { get; set; }       
        
        public IReadonlyRevisionRepositoryGroup<TFilter, EntitySchema> Revisions { get; protected set; }

        protected override void DisposeResources()
        {
            ChildSessions.Dispose();
        }

        /// <summary>
        /// Gets a sequence of <see cref="TEntity"/> matching the specified ids.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="allOrNothing">If set to <c>true</c> all ids must match in order to return any <typeparamref name="TEntity"/> instances.</param>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> Get<TEntity>(bool allOrNothing, params HiveId[] ids) where TEntity : AbstractSchemaPart
        {
            return ChildSessions.Get<TEntity>(this, allOrNothing, IdRoot, ids);
        }

        public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : AbstractSchemaPart
        {
            return ChildSessions.GetAll<TEntity>(this, IdRoot);
        }

        public bool Exists<TEntity>(HiveId id) where TEntity : AbstractSchemaPart
        {
            return ChildSessions.Exists<TEntity>(id);
        }

        public IEnumerable<IRelationById> GetParentRelations(HiveId childId, RelationType relationType)
        {
            return ChildSessions.GetParentRelations(childId, IdRoot, relationType);
        }

        public IEnumerable<IRelationById> GetAncestorRelations(HiveId descendentId, RelationType relationType)
        {
            return ChildSessions.GetAncestorRelations(descendentId, IdRoot, relationType);
        }

        public IEnumerable<IRelationById> GetDescendentRelations(HiveId ancestorId, RelationType relationType)
        {
            return ChildSessions.GetDescendentRelations(ancestorId, IdRoot, relationType);
        }

        public IEnumerable<IRelationById> GetChildRelations(HiveId parentId, RelationType relationType)
        {
            return ChildSessions.GetChildRelations(parentId, IdRoot, relationType);
        }

        public IEnumerable<IRelationById> GetBranchRelations(HiveId siblingId, RelationType relationType = null)
        {
            return ChildSessions.GetBranchRelations(siblingId, IdRoot, relationType);
        }

        public IRelationById FindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return ChildSessions.FindRelation(sourceId, destinationId, IdRoot, relationType);
        }
    }
}