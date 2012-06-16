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
    public class SchemaRepositoryGroup<TFilter> : AbstractRepositoryGroup, ISchemaRepositoryGroup<TFilter>
        where TFilter : class, IProviderTypeFilter
    {
        public SchemaRepositoryGroup(IEnumerable<AbstractSchemaRepository> childRepositories, IEnumerable<AbstractRevisionRepository<EntitySchema>> childRevisionSessions, Uri idRoot, AbstractScopedCache scopedCache, RepositoryContext hiveContext)
            : base(childRepositories, idRoot, scopedCache, hiveContext)
        {
            ChildSessions = childRepositories;
            Revisions = new RevisionRepositoryGroup<TFilter, EntitySchema>(childRevisionSessions, IdRoot, scopedCache, hiveContext);
        }

        protected IEnumerable<AbstractSchemaRepository> ChildSessions { get; set; }

        /// <summary>
        /// Used to access providers that can get or set revisions for <see cref="AbstractSchemaPart"/> types.
        /// </summary>
        /// <value>The revisions.</value>
        public IRevisionRepositoryGroup<TFilter, EntitySchema> Revisions { get; protected set; }

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

        public void AddOrUpdate(AbstractSchemaPart entity)
        {
            ChildSessions.AddOrUpdate(entity, IdRoot);
        }

        public void Delete<TEntity>(HiveId id) where TEntity : AbstractSchemaPart
        {
            ChildSessions.Delete<TEntity>(id);
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

        public void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            ChildSessions.AddRelation(item, IdRoot);
        }

        public void RemoveRelation(IRelationById item)
        {
            ChildSessions.RemoveRelation(item);
        }
    }
}