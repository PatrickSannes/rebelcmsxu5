using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Tasks;
using Umbraco.Hive.Linq;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    using Umbraco.Framework.Linq;

    using Umbraco.Framework.Linq.ResultBinding;

    public class EntityRepositoryGroup<TFilter>
        : AbstractRepositoryGroup, IEntityRepositoryGroup<TFilter>
        where TFilter : class, IProviderTypeFilter
    {
        public EntityRepositoryGroup(IEnumerable<AbstractEntityRepository> childRepositories, 
            IEnumerable<AbstractRevisionRepository<TypedEntity>> childRevisionSessions, 
            IEnumerable<AbstractSchemaRepository> childSchemaSessions, 
            IEnumerable<AbstractRevisionRepository<EntitySchema>> childSchemaRevisionSessions, 
            Uri idRoot,
            AbstractScopedCache scopedCache,
            IFrameworkContext frameworkContext,
            RepositoryContext hiveContext)
            : base(childRepositories, idRoot, scopedCache, hiveContext)
        {
            Mandate.ParameterNotNull(idRoot, "idRoot");
            Mandate.ParameterNotNull(scopedCache, "scopedCache");
            Mandate.ParameterNotNullOrEmpty(childRepositories, "childRepositories");
            Mandate.ParameterNotNullOrEmpty(childRevisionSessions, "childRevisionSessions");
            Mandate.ParameterNotNullOrEmpty(childSchemaSessions, "childSchemaSessions");
            Mandate.ParameterNotNullOrEmpty(childSchemaRevisionSessions, "childSchemaRevisionSessions");
            
            ChildSessions = childRepositories;
            FrameworkContext = frameworkContext ?? childRepositories.First().FrameworkContext;
            Revisions = new RevisionRepositoryGroup<TFilter, TypedEntity>(childRevisionSessions, IdRoot, scopedCache, hiveContext);
            Schemas = new SchemaRepositoryGroup<TFilter>(childSchemaSessions, childSchemaRevisionSessions, IdRoot, scopedCache, hiveContext);
        }

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; protected set; }

        #endregion

        protected IEnumerable<AbstractEntityRepository> ChildSessions { get; set; }

        /// <summary>
        /// Used to access providers that can get or set revisions for <see cref="TypedEntity"/> types.
        /// </summary>
        /// <value>The revisions.</value>
        public IRevisionRepositoryGroup<TFilter, TypedEntity> Revisions { get; protected set; }

        /// <summary>
        /// Used to access providers that can get or set <see cref="AbstractSchemaPart"/> types.
        /// </summary>
        /// <value>The schemas.</value>
        public ISchemaRepositoryGroup<TFilter> Schemas { get; protected set; }

        protected override void DisposeResources()
        {
            ChildSessions.Dispose();
        }

        public IQueryContext<TypedEntity> QueryContext
        {
            get
            {
                return new QueryContextWrapper<TypedEntity>(new QueryableDataSourceWrapper(ChildSessions, this, IdRoot, FrameworkContext, UnitScopedCache, HiveContext));
            }
        }

        /// <summary>
        /// Gets a sequence of <see cref="TEntity"/> matching the specified ids.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="allOrNothing">If set to <c>true</c> all ids must match in order to return any <typeparamref name="TEntity"/> instances.</param>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> Get<TEntity>(bool allOrNothing, params HiveId[] ids) where TEntity : TypedEntity
        {
            return ChildSessions.Get<TEntity>(this, allOrNothing, IdRoot, ids).Select(RaiseEntityReady);
        }

        private TEntity RaiseEntityReady<TEntity>(TEntity x) where TEntity : TypedEntity
        {
            FrameworkContext.TaskManager.ExecuteInContext(TaskTriggers.Hive.PostReadEntity, this, new TaskEventArgs(FrameworkContext, new HiveEntityPostActionEventArgs(x, UnitScopedCache)));
            return x;
        }

        public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : TypedEntity
        {
            return ChildSessions.GetAll<TEntity>(this, IdRoot).Select(RaiseEntityReady);
        }

        public bool Exists<TEntity>(HiveId id) where TEntity : TypedEntity
        {
            return ChildSessions.Exists<TEntity>(id);
        }


        public void AddOrUpdate(TypedEntity entity)
        {
            ExecuteInCancellableTask(entity, TaskTriggers.Hive.PreAddOrUpdate, TaskTriggers.Hive.PostAddOrUpdate, () => ChildSessions.AddOrUpdate(entity, IdRoot));
        }

        private void ExecuteInCancellableTask(IRelatableEntity entity, string preActionTaskTrigger, string postActionTaskTrigger, Action execution)
        {
            FrameworkContext.TaskManager.ExecuteInCancellableTask(this, entity, preActionTaskTrigger, postActionTaskTrigger, execution, x => new HiveEntityPreActionEventArgs(x, UnitScopedCache), x => new HiveEntityPostActionEventArgs(x, UnitScopedCache), FrameworkContext);
        }

        private void ExecuteInCancellableTask(IReadonlyRelation<IRelatableEntity, IRelatableEntity> entity, string preActionTaskTrigger, string postActionTaskTrigger, Action execution)
        {
            FrameworkContext.TaskManager.ExecuteInCancellableTask(this, entity, preActionTaskTrigger, postActionTaskTrigger, execution, x => new HiveRelationPreActionEventArgs(x, UnitScopedCache), x => new HiveRelationPostActionEventArgs(x, UnitScopedCache), FrameworkContext);
        }

        private void ExecuteInCancellableTask(IRelationById entity, string preActionTaskTrigger, string postActionTaskTrigger, Action execution)
        {
            FrameworkContext.TaskManager.ExecuteInCancellableTask(this, entity, preActionTaskTrigger, postActionTaskTrigger, execution, x => new HiveRelationByIdPreActionEventArgs(x, UnitScopedCache), x => new HiveRelationByIdPreActionEventArgs(x, UnitScopedCache), FrameworkContext);
        }

        public void Delete<TEntity>(HiveId id) where TEntity : TypedEntity
        {
            ChildSessions.Delete(id);
        }

        public IEnumerable<IRelationById> GetParentRelations(HiveId childId, RelationType relationType)
        {
            return UnitScopedCache.GetOrCreateTyped(
                "erg_GetParentRelations" + childId + (relationType != null ? relationType.RelationName : "any_relationtype"),
                () => ChildSessions.GetParentRelations(childId, IdRoot, relationType));
        }

        public IEnumerable<IRelationById> GetAncestorRelations(HiveId descendentId, RelationType relationType)
        {
            return UnitScopedCache.GetOrCreateTyped(
                "erg_GetAncestorRelations" + descendentId + (relationType != null ? relationType.RelationName : "any_relationtype"),
                () => ChildSessions.GetAncestorRelations(descendentId, IdRoot, relationType));
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
            ExecuteInCancellableTask(item, 
                TaskTriggers.Hive.Relations.PreRelationAdded,
                TaskTriggers.Hive.Relations.PostRelationAdded,
                () => ChildSessions.AddRelation(item, IdRoot));
        }

        public void RemoveRelation(IRelationById item)
        {
            ExecuteInCancellableTask(item,
                TaskTriggers.Hive.Relations.PreRelationRemoved,
                TaskTriggers.Hive.Relations.PostRelationRemoved,
                () => ChildSessions.RemoveRelation(item));
            ;
        }

        public IQueryableDataSource QueryableDataSource
        {
            get
            {
                return new QueryableDataSourceWrapper(ChildSessions, this, IdRoot, FrameworkContext, UnitScopedCache, HiveContext);
            }
        }

        public virtual IQueryable<TypedEntity> Query() { return new Queryable<TypedEntity>(new Executor(QueryableDataSource, Queryable<TypedEntity>.GetBinderFromAssembly())); }
        public virtual IQueryable<TSpecific> Query<TSpecific>()
        {
            return new Queryable<TSpecific>(new Executor(QueryableDataSource, Queryable<TSpecific>.GetBinderFromAssembly()));
        }
        public virtual IQueryable<TSpecific> Query<TSpecific>(ObjectBinder objectBinder) { return new Queryable<TSpecific>(new Executor(QueryableDataSource, objectBinder)); }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TypedEntity> GetEnumerator()
        {
            return Query().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </returns>
        public Expression Expression { get { return Query().Expression; } }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
        /// </returns>
        public Type ElementType { get { return Query().ElementType; } }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
        /// </returns>
        public IQueryProvider Provider { get { return Query().Provider; } }
    }
}