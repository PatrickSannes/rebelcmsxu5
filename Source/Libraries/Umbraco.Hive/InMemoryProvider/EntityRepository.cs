namespace Umbraco.Hive.InMemoryProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Umbraco.Framework;
    using Umbraco.Framework.Context;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Associations;
    using Umbraco.Framework.Persistence.ProviderSupport._Revised;
    using Umbraco.Hive.ProviderSupport;
    using Umbraco.Framework.Linq.QueryModel;
    using Umbraco.Framework.Linq.ResultBinding;

    public class EntityRepository : AbstractEntityRepository 
    {
        private readonly DependencyHelper _helper;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public EntityRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, DependencyHelper helper)
            : base(providerMetadata, frameworkContext)
        {
            _helper = helper;
        }

        public EntityRepository(ProviderMetadata providerMetadata, IProviderTransaction providerTransaction, IFrameworkContext frameworkContext, DependencyHelper helper)
            : base(providerMetadata, providerTransaction, frameworkContext)
        {
            _helper = helper;
        }

        public EntityRepository(ProviderMetadata providerMetadata, IProviderTransaction providerTransaction, AbstractRevisionRepository<TypedEntity> revisions, AbstractSchemaRepository schemas, IFrameworkContext frameworkContext, DependencyHelper helper)
            : base(providerMetadata, providerTransaction, revisions, schemas, frameworkContext)
        {
            _helper = helper;
        }

        public EntityRepository(ProviderMetadata providerMetadata, AbstractSchemaRepository schemas, IFrameworkContext frameworkContext, DependencyHelper helper) 
            : base(providerMetadata, schemas, frameworkContext)
        {
            _helper = helper;
        }

        protected override void DisposeResources()
        {
            return;
        }

        public override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] id)
        {
            return _helper.CacheHelper.PerformGet<T>(allOrNothing, id);
        }

        public override IEnumerable<T> PerformExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            return Enumerable.Empty<T>();
        }

        public override T PerformExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            return default(T);
        }

        public override T PerformExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            return default(T);
        }

        public override IEnumerable<T> PerformGetAll<T>()
        {
            return _helper.CacheHelper.PerformGetAll<T>();
        }

        public override bool CanReadRelations { get { return true; } }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return _helper.CacheHelper.PerformGetParentRelations(childId, relationType);
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return _helper.CacheHelper.PerformFindRelation(sourceId, destinationId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            return _helper.CacheHelper.PerformGetAncestorRelations(descendentId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            return _helper.CacheHelper.PerformGetDescendentRelations(ancestorId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            return _helper.CacheHelper.PerformGetChildRelations(parentId, relationType);
        }

        public override bool Exists<TEntity>(HiveId id)
        {
            return _helper.CacheHelper.Exists<TEntity>(id);
        }

        protected override void PerformAddOrUpdate(TypedEntity entity)
        {
            _helper.CacheHelper.PerformAddOrUpdate(entity);
        }

        protected override void PerformDelete<T>(HiveId id)
        {
            _helper.CacheHelper.PerformDelete<T>(id);
        }

        public override bool CanWriteRelations { get { return true; } }

        protected override void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            _helper.CacheHelper.AddRelation(item);
        }

        protected override void PerformRemoveRelation(IRelationById item)
        {
            _helper.CacheHelper.RemoveRelation(item);
        }
    }
}
