using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public sealed class NullProviderSchemaRepository : AbstractSchemaRepository
    {
        public NullProviderSchemaRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext)
            : base(providerMetadata, new NullProviderTransaction(), frameworkContext)
        {
            CanRead = false;
            CanWrite = false;
        }

        protected override void DisposeResources()
        {
            return;
        }

        protected override IEnumerable<TEntity> PerformGet<TEntity>(bool allOrNothing, params HiveId[] id)
        {
            return null;
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return null;
        }

        public override IEnumerable<TEntity> PerformGetAll<TEntity>()
        {
            return Enumerable.Empty<TEntity>().AsQueryable();
        }

        public override bool Exists<TEntity>(HiveId id)
        {
            return false;
        }

        public override bool CanReadRelations
        {
            get { return false; }
        }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return Enumerable.Empty<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>();
        }

        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId childId, RelationType relationType = null)
        {
            return Enumerable.Empty<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>();
        }

        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId childId, RelationType relationType = null)
        {
            return Enumerable.Empty<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>();
        }

        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId childId, RelationType relationType = null)
        {
            return Enumerable.Empty<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>();
        }

        public override void PerformAddOrUpdate(AbstractSchemaPart entity)
        {
            return;
        }

        public override void Delete<T>(HiveId id)
        {
            return;
        }

        public override bool CanWriteRelations
        {
            get { return false; }
        }

        protected override void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            return;
        }

        protected override void PerformRemoveRelation(IRelationById item)
        {
            return;
        }
    }
}