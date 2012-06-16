namespace Umbraco.Hive.InMemoryProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Framework;
    using Umbraco.Framework.Context;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Versioning;
    using Umbraco.Framework.Persistence.ProviderSupport._Revised;
    using Umbraco.Hive.ProviderSupport;

    public class EntityRevisionRepository : AbstractRevisionRepository<TypedEntity>
    {
        private readonly DependencyHelper _helper;

        public EntityRevisionRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, DependencyHelper helper) 
            : base(providerMetadata, frameworkContext)
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

        public override Revision<TEntity> PerformGet<TEntity>(HiveId entityId, HiveId revisionId)
        {
            return _helper.CacheHelper.PerformGet<TEntity>(entityId, revisionId);
        }

        public override IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>()
        {
            return _helper.CacheHelper.PerformGetAllRevisions<TEntity>();
        }

        public override EntitySnapshot<TEntity> PerformGetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null)
        {
            var allRevisions = GetAll<TEntity>(hiveId, revisionStatusType).ToArray();
            var selectLatest = allRevisions.OrderBy(x => x.MetaData.UtcCreated);
            var latest = selectLatest.FirstOrDefault();
            if (latest == null) return null;
            return new EntitySnapshot<TEntity>(latest, selectLatest.Select(x => x.MetaData));
        }

        public override Revision<TEntity> PerformGetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
        {
            return
                GetAll<TEntity>().Where(
                    x =>
                    x.Item.Id.EqualsIgnoringProviderId(entityId) &&
                    (revisionStatusType == null || x.MetaData.StatusType.Alias == revisionStatusType.Alias));
        }

        public override EntitySnapshot<TEntity> GetSnapshot<TEntity>(HiveId hiveId, HiveId revisionId)
        {
            var allRevisions = GetAll<TEntity>().ToArray();
            var requestedRevision = allRevisions.FirstOrDefault(x => x.MetaData.Id.EqualsIgnoringProviderId(revisionId));
            if (requestedRevision == null) return null;
            return new EntitySnapshot<TEntity>(requestedRevision, allRevisions.Select(x => x.MetaData));
        }

        public override void PerformAdd<TEntity>(Revision<TEntity> revision)
        {
            _helper.CacheHelper.PerformAdd(revision);
        }
    }
}
