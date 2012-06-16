using System.Collections.Generic;
using Examine.LuceneEngine.Providers;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine.Hive
{
    public class SchemaRepository : AbstractSchemaRepository
    {
        protected ExamineTransaction ExamineTransaction { get; private set; }
        public ExamineHelper Helper { get; private set; }

        public SchemaRepository(
            ProviderMetadata providerMetadata,
            IProviderTransaction providerTransaction,
            IFrameworkContext frameworkContext,
            ExamineHelper helper)
            : base(providerMetadata, providerTransaction, frameworkContext)
        {
            Helper = helper;
            ExamineTransaction = providerTransaction as ExamineTransaction;
        }

        protected override void DisposeResources()
        {
            Revisions.Dispose();
            Transaction.Dispose();
        }

        protected override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
        {
            return Helper.PerformGet<T>(allOrNothing, LuceneIndexer.IndexNodeIdFieldName, ids);
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return Helper.PerformFindRelation(sourceId, destinationId, relationType);
        }

        public override IEnumerable<TEntity> PerformGetAll<TEntity>()
        {
            return Helper.PerformGetAll<TEntity>(typeof(TEntity).GetEntityBaseType().Name);
        }

        public override bool CanReadRelations
        {
            get { return true; }
        }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return Helper.PeformGetParentRelations(childId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            return Helper.PerformGetAncestorRelations(this, descendentId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            return Helper.PerformGetDescendentRelations(this, ancestorId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            return Helper.PerformGetChildRelations(parentId, relationType);
        }

        public override bool Exists<TEntity>(HiveId id)
        {
            return Helper.Exists<TEntity>(id, LuceneIndexer.IndexNodeIdFieldName);
        }

        public override void PerformAddOrUpdate(AbstractSchemaPart entity)
        {
            Helper.PerformAddOrUpdate(entity, ExamineTransaction);
        }

        public override void Delete<T>(HiveId id)
        {
            Helper.PerformDelete(id, ExamineTransaction);
        }

        public override bool CanWriteRelations
        {
            get { return true; }
        }

        protected override void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            Helper.PerformAddRelation(item, ExamineTransaction);
        }

        protected override void PerformRemoveRelation(IRelationById item)
        {
            Helper.PerformDelete(item.GetCompositeId(), ExamineTransaction);
        }
    }
}