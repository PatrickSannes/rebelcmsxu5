using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine.Hive
{
    public class RevisionRepository : AbstractRevisionRepository<TypedEntity>
    {
        public ExamineTransaction ExamineTransaction { get; private set; }
        public ExamineHelper Helper { get; private set; }

        public RevisionRepository(
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
            Transaction.Dispose();
        }

        public override Revision<TEntity> PerformGet<TEntity>(HiveId entityId, HiveId revisionId)
        {
            return Helper.PerformGetRevision<TEntity>(entityId, revisionId);
        }

        public override IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>()
        {
            return Helper.PerformGetAllRevisions<TEntity>(typeof(TypedEntity).Name);
        }

        public override EntitySnapshot<TEntity> PerformGetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null)
        {
            Mandate.ParameterNotEmpty(hiveId, "hiveId");

            var revision = GetLatestRevision<TEntity>(hiveId, revisionStatusType);
            if (revision == null) return null;

            //get all the revisions for this entity and only select the revision data for it
            var otherRevisionData = Helper.PerformGetAllRevisions<TEntity>(typeof(TypedEntity).Name, hiveId)
                .Select(x => x.MetaData);

            return new EntitySnapshot<TEntity>(revision, otherRevisionData);
        }

        public override Revision<TEntity> PerformGetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
        {
            return Helper.PerformGetLatestRevision<TEntity>(entityId, revisionStatusType);
        }

        public override IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
        {
            return Helper.PerformGetAllRevisions<TEntity>(typeof(TypedEntity).Name, entityId, revisionStatusType);
        }

        public override EntitySnapshot<TEntity> GetSnapshot<TEntity>(HiveId hiveId, HiveId revisionId)
        {
            Mandate.ParameterNotEmpty(hiveId, "hiveId");
            Mandate.ParameterNotEmpty(revisionId, "revisionId");

            var revision = Get<TEntity>(hiveId, revisionId);

            if (revision == null) return null;

            //get all the revisions for this entity and only select the revision data for it
            var otherRevisionData = Helper.PerformGetAllRevisions<TEntity>(typeof (TypedEntity).Name, hiveId)
                .Select(x => x.MetaData);

            return new EntitySnapshot<TEntity>(revision, otherRevisionData);
        }

        public override void PerformAdd<TEntity>(Revision<TEntity> revision)
        {
            Helper.PerformAddRevision(revision, ExamineTransaction);
        }
    }
}