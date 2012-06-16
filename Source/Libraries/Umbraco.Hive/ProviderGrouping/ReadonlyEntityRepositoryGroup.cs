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
using Umbraco.Hive.Linq;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    using Umbraco.Framework.Linq;

    using Umbraco.Framework.Linq.ResultBinding;

    public class ReadonlyEntityRepositoryGroup<TFilter> 
        : AbstractRepositoryGroup, IReadonlyEntityRepositoryGroup<TFilter>
        where TFilter : class, IProviderTypeFilter
    {
        public ReadonlyEntityRepositoryGroup(IEnumerable<AbstractReadonlyEntityRepository> childRepositories, 
            IEnumerable<AbstractReadonlyRevisionRepository<TypedEntity>> childRevisionSessions, 
            IEnumerable<AbstractReadonlySchemaRepository> childSchemaSessions, 
            IEnumerable<AbstractReadonlyRevisionRepository<EntitySchema>> childSchemaRevisionSessions, 
            Uri idRoot,
            AbstractScopedCache scopedCache,
            IFrameworkContext context,
            RepositoryContext hiveContext) 
            : base(childRepositories, idRoot, scopedCache, hiveContext)
        {
            ChildSessions = childRepositories;
            Revisions = new ReadonlyRevisionRepositoryGroup<TFilter, TypedEntity>(childRevisionSessions, IdRoot, scopedCache, hiveContext);
            Schemas = new ReadonlySchemaRepositoryGroup<TFilter>(childSchemaSessions, childSchemaRevisionSessions, IdRoot, scopedCache, hiveContext);
            FrameworkContext = context;
        }

        protected IEnumerable<AbstractReadonlyEntityRepository> ChildSessions { get; set; }
        public IReadonlyRevisionRepositoryGroup<TFilter, TypedEntity> Revisions { get; protected set; }
        public IReadonlySchemaRepositoryGroup<TFilter> Schemas { get; protected set; }

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
            return ChildSessions.Get<TEntity>(this, allOrNothing, IdRoot, ids);
        }

        public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : TypedEntity
        {
            return ChildSessions.GetAll<TEntity>(this, IdRoot);
        }

        public bool Exists<TEntity>(HiveId id) where TEntity : TypedEntity
        {
            return ChildSessions.Exists<TEntity>(id);
        }

        public IEnumerable<IRelationById> GetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return UnitScopedCache.GetOrCreateTyped(
                "rerg_GetParentRelations" + childId + (relationType != null ? relationType.RelationName : "any_relationtype"),
                () => ChildSessions.GetParentRelations(childId, IdRoot, relationType).ToArray());
        }

        public IEnumerable<IRelationById> GetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            return UnitScopedCache.GetOrCreateTyped(
                "rerg_GetAncestorRelations" + descendentId + (relationType != null ? relationType.RelationName : "any_relationtype"),
                () => ChildSessions.GetAncestorRelations(descendentId, IdRoot, relationType).ToArray());
        }

        public IEnumerable<IRelationById> GetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            return ChildSessions.GetDescendentRelations(ancestorId, IdRoot, relationType);
        }

        public IEnumerable<IRelationById> GetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            return ChildSessions.GetChildRelations(parentId, IdRoot, relationType);
        }

        public IEnumerable<IRelationById> GetBranchRelations(HiveId siblingId, RelationType relationType = null)
        {
            return ChildSessions.GetBranchRelations(siblingId, IdRoot, relationType);
        }

        public IRelationById FindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType = null)
        {
            return ChildSessions.FindRelation(sourceId, destinationId, IdRoot, relationType);
        }


        public IQueryableDataSource QueryableDataSource
        {
            get
            {
                return new QueryableDataSourceWrapper(ChildSessions, this, IdRoot, FrameworkContext, UnitScopedCache, HiveContext);
            }
        }

        public virtual IQueryable<TypedEntity> Query() { return new Queryable<TypedEntity>(new Executor(QueryableDataSource, Queryable<TypedEntity>.GetBinderFromAssembly())); }
        public virtual IQueryable<TSpecific> Query<TSpecific>() { return new Queryable<TSpecific>(new Executor(QueryableDataSource, Queryable<TSpecific>.GetBinderFromAssembly())); }
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

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; protected set; }

        #endregion
    }
}