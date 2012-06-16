using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Persistence.RdbmsModel;
using Umbraco.Hive;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.NHibernate
{
    public class RevisionRepository : AbstractRevisionRepository<TypedEntity>
    {
        public RevisionRepository(ProviderMetadata providerMetadata, IProviderTransaction providerTransaction, ISession nhSession, IFrameworkContext frameworkContext,
            bool isReadOnly)
            : base(providerMetadata, providerTransaction, frameworkContext)
        {
            IsReadonly = isReadOnly;
            Transaction = providerTransaction;
            Helper = new NhSessionHelper(nhSession, frameworkContext);
        }

        protected bool IsReadonly { get; set; }
        protected internal NhSessionHelper Helper { get; set; }

        protected override void DisposeResources()
        {
            Helper.IfNotNull(x => x.Dispose());
            Transaction.Dispose();
        }

        public override Revision<TEntity> PerformGet<TEntity>(HiveId entityId, HiveId revisionId)
        {
            Mandate.ParameterNotEmpty(entityId, "entityId");
            Mandate.ParameterNotEmpty(revisionId, "revisionId");

            var revisionGuid = (Guid)revisionId.Value;
            var entityGuid = (Guid)entityId.Value;


            NodeVersion outerVersionQuery = null;
            var futureValue = Helper.NhSession.QueryOver<NodeVersion>(() => outerVersionQuery)
                .Where(x => x.Id == revisionGuid).And(x => x.Node.Id == entityGuid)
                .Fetch(x => x.Node).Eager
                .Fetch(x => x.NodeVersionStatuses).Eager
                .FutureValue();

            Helper.AddAttributeValueFuturesToSession(new[] { revisionGuid }, outerVersionQuery);

            // Cause the multi-query to execute
            var entity = futureValue.Value;

            return entity == null ? null : FrameworkContext.TypeMappers.Map<Revision<TEntity>>(entity);
        }

        public override IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>()
        {
            var versions = Helper.GetNodeVersionsByStatusDesc(limitToLatestRevision: false);

            return versions.Select(x => FrameworkContext.TypeMappers.Map<Revision<TEntity>>(x));
        }

        public override Revision<TEntity> PerformGetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
        {
            var revision =
                Helper.GetNodeVersionsByStatusDesc(
                    new[] { (Guid)entityId.Value }, revisionStatusType, true)
                    .FirstOrDefault();
            return revision == null ? null : FrameworkContext.TypeMappers.Map<Revision<TEntity>>(revision);
        }

        public override IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
        {
            if (entityId.Value.Type != HiveIdValueTypes.Guid) return Enumerable.Empty<Revision<TEntity>>();

            var versions = Helper.GetNodeVersionsByStatusDesc(new[] { (Guid)entityId.Value }, revisionStatusType, false).ToArray();

            return versions.Select(x => FrameworkContext.TypeMappers.Map<Revision<TEntity>>(x));

            //IList<NodeVersion> resultSet;

            //NodeVersion outerVersionQuery = null;
            //var baseQuery = Helper.NhSession.QueryOver<NodeVersion>(() => outerVersionQuery)
            //            .Where(x => x.Node.Id == (Guid)entityId.Value);
            ////.Fetch(x => x.NodeVersionStatuses).Eager;


            //if (revisionStatusType == null)
            //    resultSet = baseQuery.List();
            //else
            //{
            //    resultSet = baseQuery.JoinQueryOver<NodeVersionStatusHistory>(x => x.NodeVersionStatuses)
            //        .JoinQueryOver<NodeVersionStatusType>(x => x.NodeVersionStatusType)
            //        .Where(x => x.Name == revisionStatusType.Alias)
            //        .List();
            //}

            //return resultSet.DistinctBy(x => x.Id).Select(x => FrameworkContext.TypeMappers.Map<Revision<TEntity>>(x));
        }

        public override void PerformAdd<TEntity>(Revision<TEntity> revision)
        {
            Mandate.ParameterNotNull(revision, "revision");

            if (!revision.Item.Id.IsNullValueOrEmpty() && revision.Item.Id.Value.Type != HiveIdValueTypes.Guid) return;

            if (TryUpdateExisting(revision)) return;

            Helper.MapAndMerge(revision, FrameworkContext.TypeMappers);
        }

        public override EntitySnapshot<TEntity> PerformGetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null)
        {
            Mandate.ParameterNotEmpty(hiveId, "hiveId");

            var latestRevision = GetLatestRevision<TEntity>(hiveId, revisionStatusType);

            if (latestRevision == null) return null;



            //IEnumerable<Revision<TEntity>> revisions = GetAll<TEntity>(hiveId, revisionStatusType);

            //if (!revisions.Any()) return null;

            //if (revisionStatusType != null)
            //    revisions = revisions.Where(x => x.MetaData.StatusType.Id == revisionStatusType.Id);
            //var revision = revisions.OrderByDescending(x => x.MetaData.UtcStatusChanged).FirstOrDefault();

            IEnumerable<RevisionData> otherRevisionData = GetAllRevisionData(hiveId);

            return new EntitySnapshot<TEntity>(latestRevision, otherRevisionData);
        }

        public override EntitySnapshot<TEntity> GetSnapshot<TEntity>(HiveId hiveId, HiveId revisionId)
        {
            Mandate.ParameterNotEmpty(hiveId, "hiveId");
            Mandate.ParameterNotEmpty(revisionId, "revisionId");

            var revision = Get<TEntity>(hiveId, revisionId);

            if (revision == null) return null;

            IEnumerable<RevisionData> otherRevisionData = GetAllRevisionData(hiveId);

            return new EntitySnapshot<TEntity>(revision, otherRevisionData);
        }

        private IEnumerable<RevisionData> GetAllRevisionData(HiveId entityUri)
        {
            Mandate.ParameterNotEmpty(entityUri, "hiveId");

            //var entityStatusLog = Helper.NhSession.QueryOver<NodeVersionStatusHistory>()
            //    .OrderBy(x => x.Date).Desc
            //    .JoinQueryOver(x => x.NodeVersion).Where(x => x.Node.Id == (Guid)entityUri.Value)
            //    .List()
            //    .DistinctBy(x => x.Id);

            NodeVersionStatusHistory aliasHistory = null;
            NodeVersion aliasVersion = null;
            Node aliasNode = null;
            NodeVersionStatusType aliasType = null;
            var entityStatusLog = Helper.NhSession.QueryOver<NodeVersionStatusHistory>(() => aliasHistory)
                .OrderBy(x => x.Date).Desc
                .JoinQueryOver(x => x.NodeVersionStatusType, () => aliasType)
                .JoinQueryOver(x => aliasHistory.NodeVersion, () => aliasVersion)
                .JoinQueryOver(x => x.Node, () => aliasNode)
                .Where(x => x.Id == (Guid)entityUri.Value)
                .Fetch(x => aliasHistory.NodeVersionStatusType).Eager
                .Select(x => x.Date, x => x.Id, x => aliasNode.DateCreated, x => aliasType.Id, x => aliasType.IsSystem, x => aliasType.Alias, x => aliasType.Name, x => aliasVersion.Id)
                .List<object[]>()
                .Select(col => new
                    {
                        Date = (DateTimeOffset)col[0],
                        Id = (Guid)col[1],
                        DateCreated = (DateTimeOffset)col[2],
                        TypeId = (Guid)col[3],
                        TypeIsSystem = (bool)col[4],
                        TypeAlias = (string)col[5],
                        TypeName = (string)col[6],
                        VersionId = (Guid)col[7]
                    });

            var otherRevisionData = new HashSet<RevisionData>();
            var changeset = new Changeset(new Branch("default")); // Ignored for the moment in the persistence layer for this provider

            foreach (var statusQueryRow in entityStatusLog)
            {
                var nodeVersionStatusType = new NodeVersionStatusType
                                                {
                                                    Alias = statusQueryRow.TypeAlias,
                                                    Name = statusQueryRow.TypeName,
                                                    Id = statusQueryRow.TypeId,
                                                    IsSystem = statusQueryRow.TypeIsSystem
                                                };

                var revisionStatusType = FrameworkContext.TypeMappers.Map<RevisionStatusType>(nodeVersionStatusType);
                var revisionData = new RevisionData(changeset, (HiveId)statusQueryRow.VersionId, revisionStatusType)
                {
                    UtcCreated = statusQueryRow.DateCreated,
                    UtcModified = statusQueryRow.Date,
                    UtcStatusChanged = statusQueryRow.Date
                };

                otherRevisionData.Add(revisionData);
            }
            return otherRevisionData;
        }

        private bool TryUpdateExisting<T>(Revision<T> persistedEntity) where T : TypedEntity
        {
            var mappers = FrameworkContext.TypeMappers;

            if (persistedEntity.Item.Id.IsNullValueOrEmpty() && persistedEntity.MetaData.Id.IsNullValueOrEmpty()) return false;

            NodeVersion versionToEdit = null;
            if (!persistedEntity.MetaData.Id.IsNullValueOrEmpty())
            {
                // We're editing an existing revision
                versionToEdit = Helper.NhSession.Get<NodeVersion>((Guid)persistedEntity.MetaData.Id.Value);
            }

            if (versionToEdit == null)
            {
                var node = Helper.NhSession.Get<Node>((Guid)persistedEntity.Item.Id.Value);
                if (node == null) return false;

                // Since this is a new version we're adding to the db, ensure we haven't been given any attribute ids
                // that may affect another NodeVersion
                foreach (var attribute in persistedEntity.Item.Attributes)
                {
                    attribute.Id = HiveId.Empty;
                }

                versionToEdit = mappers.Map<NodeVersion>(persistedEntity);
                versionToEdit.Node = node;

                // Save it so that we get an ID
                versionToEdit = Helper.NhSession.Merge(versionToEdit) as NodeVersion;
                node.NodeVersions.Add(versionToEdit);
            }
            else
            {
                // Map our incoming data onto the entity. No need to save anything as we're dealing only in entities that 
                // were already in the session anyway
                mappers.Map(persistedEntity, versionToEdit, persistedEntity.GetType(), versionToEdit.GetType());
            }

            // Map the merge result back onto our incoming data to refresh it (with Ids, etc.)
            mappers.Map(versionToEdit, persistedEntity, versionToEdit.GetType(), persistedEntity.GetType());

            return true;
        }

        public override IEnumerable<Revision<TEntity>> PerformGetLatestRevisions<TEntity>(bool allOrNothing, RevisionStatusType revisionStatusType = null, params HiveId[] entityIds)
        {
            Mandate.ParameterNotNull(entityIds, "entityIds");
            entityIds.ForEach(x => Mandate.ParameterNotEmpty(x, "entityIds"));
            Guid[] nodeIds = entityIds.Where(x => x.Value.Type == HiveIdValueTypes.Guid).Select(x => (Guid)x.Value).ToArray();
            var revisions = Helper.GetNodeVersionsByStatusDesc(nodeIds, revisionStatusType, true).Distinct();
            return nodeIds.Select(x => revisions.SingleOrDefault(y => y.Node.Id == x)).WhereNotNull().Select(x => FrameworkContext.TypeMappers.Map<Revision<TEntity>>(x));
        }
    }
}