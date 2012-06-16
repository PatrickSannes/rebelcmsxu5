namespace Umbraco.Hive.InMemoryProvider
{
    using System.Collections.Generic;
    using System.Threading;
    using Umbraco.Framework;
    using Umbraco.Framework.Context;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Associations;
    using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
    using Umbraco.Framework.Persistence.ProviderSupport._Revised;
    using Umbraco.Hive.ProviderSupport;

    public class SchemaRepository : AbstractSchemaRepository 
    {
        private readonly DependencyHelper _helper;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public SchemaRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, DependencyHelper helper)
            : base(providerMetadata, frameworkContext)
        {
            _helper = helper;
        }

        public SchemaRepository(ProviderMetadata providerMetadata, IProviderTransaction providerTransaction, IFrameworkContext frameworkContext, DependencyHelper helper)
            : base(providerMetadata, providerTransaction, frameworkContext)
        {
            _helper = helper;
        }

        public SchemaRepository(ProviderMetadata providerMetadata, AbstractRevisionRepository<EntitySchema> revisions, IProviderTransaction providerTransaction, IFrameworkContext frameworkContext, DependencyHelper helper)
            : base(providerMetadata, revisions, providerTransaction, frameworkContext)
        {
            _helper = helper;
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            return;
        }

        protected override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
        {
            return _helper.CacheHelper.PerformGet<T>(allOrNothing, ids);
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return _helper.CacheHelper.PerformFindRelation(sourceId, destinationId, relationType);
        }

        public override IEnumerable<TEntity> PerformGetAll<TEntity>()
        {
            return _helper.CacheHelper.PerformGetAll<TEntity>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance can read relations.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can read relations; otherwise, <c>false</c>.
        /// </value>
        public override bool CanReadRelations { get { return true; } }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return _helper.CacheHelper.PerformGetParentRelations(childId, relationType);
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

        /// <summary>
        /// Identifies if a <see cref="!:TEntity"/> with matching <paramref name="id"/> can be found in this repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam><param name="id">The id.</param>
        /// <returns>
        /// <code>
        /// true
        /// </code>
        ///  if the item with <paramref name="id"/> can be found, otherwise 
        /// <code>
        /// false
        /// </code>
        /// .
        /// </returns>
        public override bool Exists<TEntity>(HiveId id)
        {
            return _helper.CacheHelper.Exists<TEntity>(id);
        }

        public override void PerformAddOrUpdate(AbstractSchemaPart entity)
        {
            _helper.CacheHelper.PerformAddOrUpdate(entity);
        }

        public override void Delete<T>(HiveId id)
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
