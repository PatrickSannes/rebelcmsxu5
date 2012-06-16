using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using HibernatingRhinos.Profiler.Appender;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using Umbraco.Framework.Context;
using Umbraco.Framework.Linq.QueryModel;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.NHibernate.Dependencies;
using Umbraco.Framework.Persistence.RdbmsModel;
using Umbraco.Framework.TypeMapping;
using Attribute = Umbraco.Framework.Persistence.RdbmsModel.Attribute;
using AttributeDefinition = Umbraco.Framework.Persistence.RdbmsModel.AttributeDefinition;

namespace Umbraco.Framework.Persistence.NHibernate
{
    using global::NHibernate.Dialect;

    public class NhSessionHelper : DisposableObject
    {
        private readonly NodeChangeTrackerBag _trackPostCommits = new NodeChangeTrackerBag();
        public ISession NhSession { get; protected set; }

        public NhSessionHelper(ISession nhSession, IFrameworkContext frameworkContext)
        {
            NhSession = nhSession;
            NhEventListeners.AddNodeIdHandler(this, HandleNodeIdAvailable);
            FrameworkContext = frameworkContext;
        }

        public IFrameworkContext FrameworkContext { get; protected set; }

        public void HandleNodeIdAvailable(IReferenceByGuid node, Guid id)
        {
            var persistenceEntity = _trackPostCommits.FlushWhere(x => x.Key.Id == node.Id);

            foreach (var keyValuePair in persistenceEntity)
            {
                keyValuePair.Value.Id = (HiveId)id;
            }
        }

        public NodeRelationType GetOrCreateNodeRelationType(string alias, AbstractScopedCache repositoryScopedCache)
        {
            return repositoryScopedCache.GetOrCreateTyped(
                "NodeRelationType-" + alias,
                () =>
                {
                    var existing = NhSession.QueryOver<NodeRelationType>()
                        .Where(x => x.Alias == alias)
                        .Cacheable()
                        .Take(1)
                        .SingleOrDefault<NodeRelationType>();

                    return existing ?? new NodeRelationType() { Alias = alias, Id = alias.EncodeAsGuid() };
                });
        }

        public const string ProfilerLoggingPrefix = "CUSTOM PROFILE: ";

        public Guid GetSessionId()
        {
            return ((ISessionImplementor)NhSession).SessionId;
        }

        private static Dialect GetDialect(ISessionFactory sessionFactory)
        {
            var implementor = sessionFactory as ISessionFactoryImplementor;
            if (implementor == null) return null;
            return implementor.Dialect;
        }

        private bool RequiresAllForGtSubquery()
        {
            var dialect = GetDialect(NhSession.SessionFactory);
            if (dialect == null) return false;
            var sqlCeDialect = dialect as MsSqlCe40Dialect;
            if (sqlCeDialect != null) return true;
            return false;
        }

        public IQueryOver<NodeVersion, NodeVersionStatusHistory> GenerateVersionedQuery(out NodeVersion outerVersionSelectAlias, Guid[] nodeIds = null, RevisionStatusType revisionStatus = null, bool limitToLatestRevision = true)
        {
            // We want the NodeVersion table, joined to the NodeVersionStatusHistory table,
            // ordered by the status history date descending, but we only want the latest one

            Node node = null;
            NodeVersion subSelectVersion = null;
            NodeVersionStatusHistory subSelectTopStatus = null;
            NodeVersion outerVersionSelect = null;
            NodeVersionStatusType subSelectStatusType = null;

            // First define the subselection of the top 1 version items when joined and sorted by version status history in date-descending order
            // We also add a clause to say "where the outer selected version's node id equals the subselected node id" since it's selecting the top 1
            // so we want it to be the 1 latest-date item relevant for each row of the outer select
            var subSelectTopStatusByDate = QueryOver.Of(() => subSelectTopStatus)
                //.Where(() => subSelectTopStatus.NodeVersion.Id == outerVersionSelect.Id);
                .JoinQueryOver(() => subSelectTopStatus.NodeVersion, () => subSelectVersion)
                .JoinQueryOver(() => subSelectVersion.Node, () => node)
                .Where(() => subSelectTopStatus.NodeVersion.Id == subSelectVersion.Id)
                .And(() => outerVersionSelect.Node.Id == node.Id);

            int takeCount = limitToLatestRevision ? 1 : 999;

            // Now we need to add a filter for the revision status type, if one was supplied
            QueryOver<NodeVersionStatusHistory> subSelectTopStatusByDateWithFilter = null;
            QueryOver<NodeVersionStatusHistory> excludeNegatingStatusses = null;
            if (revisionStatus != null)
            {
                var statusAlias = revisionStatus.Alias;

                if (revisionStatus.NegatedByTypes.Any())
                {
                    NodeVersionStatusHistory negateHistory = null;
                    NodeVersionStatusType negateType = null;
                    NodeVersion negateVersion = null;
                    var negatingAliases = revisionStatus.NegatedByTypes.Select(x => x.Alias).ToArray();

                    //var first = negatingAliases.First();
                    excludeNegatingStatusses = QueryOver.Of(() => negateHistory)
                        .JoinAlias(() => negateHistory.NodeVersionStatusType, () => negateType)
                        .JoinAlias(() => negateHistory.NodeVersion, () => negateVersion)
                        .Where(() => negateType.Alias.IsIn(negatingAliases))
                        .And(() => outerVersionSelect.Node == negateVersion.Node)
                        .Select(Projections.SqlFunction("coalesce", NHibernateUtil.DateTime, Projections.Max<NodeVersionStatusHistory>(x => x.Date), new ConstantProjection(new DateTime(1981, 8, 1))));

                    //excludeNegatingStatusses = QueryOver.Of(() => negateHistory)
                    //    .JoinAlias(() => negateHistory.NodeVersionStatusType, () => negateType)
                    //    .JoinAlias(() => negateHistory.NodeVersion, () => negateVersion)
                    //    .JoinAlias(() => negateVersion.Node, () => node)
                    //    .Where(() => negateType.Alias.IsIn(negatingAliases))
                    //    .And(() => negateHistory.Date > subSelectTopStatus.Date)
                    //    //.And(() => subSelectTopStatus.NodeVersion.Id == negateVersion.Id)
                    //    .And(() => outerVersionSelect.Node.Id == node.Id)
                    //    .Select(x => x.Id)
                    //    .Take(1);

                    //excludeNegatingStatusses = QueryOver.Of(() => negateHistory)
                    //    .JoinAlias(() => negateHistory.NodeVersionStatusType, () => negateType)
                    //    .JoinAlias(() => negateHistory.NodeVersion, () => negateVersion)
                    //    .JoinAlias(() => negateVersion.Node, () => node)
                    //    .Where(() => negateType.Alias.IsIn(negatingAliases))
                    //    .And(() => outerVersionSelect.Node.Id == node.Id)
                    //    .And(() => negateHistory.Date > subSelectTopStatus.Date)
                    //    .OrderBy(x => x.Date).Desc
                    //    .Select(x => x.Id)
                    //    .Take(1);
                }

                var subSelectBuilder = subSelectTopStatusByDate
                    .And(() => subSelectStatusType.Alias == statusAlias)
                    .JoinQueryOver(x => subSelectTopStatus.NodeVersionStatusType, () => subSelectStatusType)
                    .OrderBy(() => subSelectTopStatus.Date).Desc;

                if (excludeNegatingStatusses != null)
                {
                    // Yeah, I know, horrible to check Sql dialect when generating query, but Nh's dialect support doesn't allow a provider-based
                    // way of telling whether the db engine supports / requires "all" to be prefix before a subquery when doing an operation.
                    // e.g. SqlCe requires "blah > all (select max(blah) from blah)", SqlServer doesn't mind, SQLite doesn't support it
                    if (RequiresAllForGtSubquery())
                        subSelectBuilder = subSelectBuilder.WithSubquery.WhereProperty(() => subSelectTopStatus.Date).GtAll(excludeNegatingStatusses);
                    else
                        subSelectBuilder = subSelectBuilder.WithSubquery.WhereProperty(() => subSelectTopStatus.Date).Gt(excludeNegatingStatusses);

                    //subSelectBuilder = subSelectBuilder.WithSubquery.WhereNotExists(excludeNegatingStatusses);
                    //subSelectBuilder = subSelectBuilder.WithSubquery.WhereProperty(() => subSelectTopStatus.Id).NotIn(excludeNegatingStatusses);
                }

                 subSelectTopStatusByDateWithFilter = subSelectBuilder.Select(x => subSelectTopStatus.NodeVersion.Id).Take(takeCount);
                // We have to include a Take here for compatibility with SqlServerCe
            }
            else
            {
                subSelectTopStatusByDateWithFilter = subSelectTopStatusByDate
                    .OrderBy(() => subSelectTopStatus.Date).Desc
                    .Select(x => subSelectTopStatus.NodeVersion.Id).Take(takeCount);
                // We have to include a Take here for compatibility with SqlServerCe
            }

            NodeVersionStatusHistory outerHistoryForSort = null;
            IQueryOver<NodeVersion, NodeVersionStatusHistory> outerQuery = NhSession.QueryOver<NodeVersion>(
                () => outerVersionSelect)
                //.Fetch(x => x.AttributeSchemaDefinition).Eager
                .Fetch(x => x.Attributes).Eager
                // We load these eagerly rather than in a Future to avoid a separate query
                .Fetch(x => x.Node).Eager
                // There's a 1-m mapping between Node-NodeVersion so safe to load this with a join too rather than with a future
                .Inner.JoinQueryOver(x => outerHistoryForSort.NodeVersionStatusType, () => subSelectStatusType)
                .Inner.JoinQueryOver(x => outerVersionSelect.NodeVersionStatuses, () => outerHistoryForSort);

            NodeVersion innerVersion = null;
            NodeVersionStatusHistory innerHistory = null;
            NodeVersionStatusType innerType = null;
            var buildInnerHistorySubQuery = QueryOver.Of<NodeVersionStatusHistory>(() => innerHistory)
                .JoinQueryOver(() => innerHistory.NodeVersion, () => innerVersion)
                .JoinQueryOver(() => innerHistory.NodeVersionStatusType, () => innerType)
                .Where(() => innerVersion.Node == outerVersionSelect.Node);
                
            if (revisionStatus != null)
            {
                var statusAlias = revisionStatus.Alias;
                buildInnerHistorySubQuery = buildInnerHistorySubQuery
                    .And(() => innerType.Alias == statusAlias);

                NodeVersionStatusType reselectType = null;
                outerQuery = outerQuery
                    .And(() => subSelectStatusType.Alias == statusAlias);

                // Yeah, I know, horrible to check Sql dialect when generating query, but Nh's dialect support doesn't allow a provider-based
                // way of telling whether the db engine supports / requires "all" to be prefix before a subquery when doing an operation.
                // e.g. SqlCe requires "blah > all (select max(blah) from blah)", SqlServer doesn't mind, SQLite doesn't support it
                if (excludeNegatingStatusses != null)
                    if (RequiresAllForGtSubquery())
                    {
                        outerQuery = outerQuery.WithSubquery.WhereProperty(() => outerHistoryForSort.Date).GtAll(excludeNegatingStatusses);
                    }
                    else
                    {
                        outerQuery = outerQuery.WithSubquery.WhereProperty(() => outerHistoryForSort.Date).Gt(excludeNegatingStatusses);
                    }
            }

            var subQueryInnerHistory = buildInnerHistorySubQuery.Select(x => x.Id)
                .OrderBy(() => innerHistory.Date).Desc
                .Take(takeCount);

            outerQuery = outerQuery
                .OrderBy(() => outerHistoryForSort.Date).Asc
                .WithSubquery.WhereProperty(() => outerHistoryForSort.Id).In(subQueryInnerHistory);

            //// Based on perf testing of SqlCe queries, it shaves a lot off execution time if we specify the status type in the
            //// outer query in addition to the subquery
            //if (revisionStatus != null)
            //{
            //    var statusAlias = revisionStatus.Alias;
            //    outerQuery = outerQuery
            //        .And(x => subSelectStatusType.Alias == statusAlias);
            //}

            //outerQuery = outerQuery
            //    .OrderBy(() => outerHistoryForSort.Date).Asc
            //    .WithSubquery.WhereProperty(() => outerVersionSelect.Id).In(subSelectTopStatusByDateWithFilter.Clone());

            if (nodeIds != null && nodeIds.Any()) outerQuery = outerQuery.And(() => outerVersionSelect.Node.Id.IsIn(nodeIds));

            outerVersionSelectAlias = outerVersionSelect;
            return outerQuery;
        }

        public IQueryOver<NodeVersion, NodeVersionStatusHistory> GenerateVersionedQueryPlusAttributes(Guid[] nodeIds = null, RevisionStatusType revisionStatus = null, bool limitToLatestRevision = true, IEnumerable<SortClause> sortClauses = null)
        {
            if (sortClauses == null) sortClauses = Enumerable.Empty<SortClause>();
            NodeVersion outerVersionSelect = null;
            var outerQuery = GenerateVersionedQuery(out outerVersionSelect, nodeIds, revisionStatus, limitToLatestRevision);

            Attribute attribAlias = null;
            AttributeDateValue attributeDateValue = null;
            AttributeDecimalValue attributeDecimalValue = null;
            AttributeIntegerValue attributeIntegerValue = null;
            AttributeStringValue attributeStringValue = null;
            AttributeLongStringValue attributeLongStringValue = null;
            outerQuery = outerQuery
                .Left.JoinAlias(() => outerVersionSelect.Attributes, () => attribAlias) // Using a left join identifies to Nh that it can reuse the loaded Attributes because Nh considers them to be unaffected by the query, otherwise it issues another select from accessing NodeVersion.Attributes
                .Left.JoinAlias(() => attribAlias.AttributeStringValues, () => attributeStringValue)
                .Left.JoinAlias(() => attribAlias.AttributeLongStringValues, () => attributeLongStringValue)
                .Left.JoinAlias(() => attribAlias.AttributeIntegerValues, () => attributeIntegerValue)
                .Left.JoinAlias(() => attribAlias.AttributeDecimalValues, () => attributeDecimalValue)
                .Left.JoinAlias(() => attribAlias.AttributeDateValues, () => attributeDateValue);

            //if (sortClauses.Any())
            //{
            //    AttributeDefinition attribDef = null;
            //    outerQuery = outerQuery.JoinAlias(() => attribAlias.AttributeDefinition, () => attribDef);
            //    foreach (var sortClause in sortClauses)
            //    {
            //        var coalesce = Projections.SqlFunction("coalesce", NHibernateUtil.String, 
            //            Projections.Property(() => attributeStringValue.Value), 
            //            Projections.Property(() => attributeLongStringValue.Value));
            //        if (sortClause.Direction == SortDirection.Ascending)
            //            outerQuery = outerQuery.OrderBy(coalesce).Asc;
            //        else
            //            outerQuery = outerQuery.OrderBy(coalesce).Desc;
            //    }
            //}

            return outerQuery;
        }

        public IEnumerable<NodeVersion> GetNodeVersionsByStatusDesc(Guid[] nodeIds = null, RevisionStatusType revisionStatus = null, bool limitToLatestRevision = true)
        {
            using (NhProfilerLogging.Start(NhSession, "GetNodeVersionsByStatusDesc",
                new OdbcParameter("nodeIds", nodeIds != null ? string.Join(", ", nodeIds.Select(x => x.ToString())) : "(empty)"),
                new OdbcParameter("revisionStatus", revisionStatus != null ? revisionStatus.Alias : "(empty)"),
                new OdbcParameter("limitToLatestRevision", limitToLatestRevision)))
            {
                //NhSession.Clear();
                var query = GenerateVersionedQueryPlusAttributes(nodeIds, revisionStatus, limitToLatestRevision);
                var nodeVersions = query.List();
                return nodeVersions.Distinct();




                //// The query creates a Cartesian product because of the joins, but it's still more efficient than using Futures until it's possible for NH
                //// to recognise AttributeStringValues (for example) when selecting only the 8 that match the Attribute; at the moment you have to left-join
                //// Attribute to AttributeStringValues which causes Attribute*AttributeStringValues rows anyway
                //var foundVersions = attribs.List().Select(x => x.NodeVersion).Distinct();

                //// Because we selected from Attributes in order to do an efficient query for NodeVersions that have Attributes, we'll actually
                //// miss any NodeVersions that have zero attributes, such as the SystemRoot node. Therefore, for any items that were requested
                //// but not returned by the first query, we'll request just the NodeVersion
                //var idsFound = foundVersions.Select(x => x.Node.Id).Distinct().ToArray();

                //// We need to account for if this method was called without nodeIds specified though, in which case we'll need to 
                //// get all of the nodeIds that have an attached version from the database
                //Guid[] idsNotFound;
                //if (nodeIds != null)
                //{
                //    idsNotFound = nodeIds.Except(idsFound).ToArray();
                //}
                //else
                //{
                //    var getAllIds = NhSession.QueryOver<NodeVersion>().Select(x => x.Node.Id).List<Guid>().Distinct();
                //    idsNotFound = getAllIds.Except(idsFound).ToArray();
                //}

                //if (idsNotFound.Any())
                //{
                //    NodeVersionStatusHistory outerHistoryForSort = null;
                //    var outerQuery = NhSession.QueryOver<NodeVersion>(() => outerVersionSelect)
                //        .Fetch(x => x.Attributes).Eager     // Even though we know these nodes have no Attributes, we need to allow NH to know this without
                //        .Fetch(x => x.Node).Eager           // issuing a separate request during mapping of the Attributes / Node properties
                //        .JoinQueryOver(x => outerVersionSelect.NodeVersionStatuses, () => outerHistoryForSort)
                //        .OrderBy(() => outerHistoryForSort.Date).Asc
                //        .WithSubquery.WhereProperty(() => outerVersionSelect.Id).In(subSelectTopStatusByDateWithFilter)
                //        .And(() => outerVersionSelect.Node.Id.IsIn(idsNotFound));

                //    foundVersions = foundVersions.Concat(outerQuery.List());
                //}

            }
        }

        public void AddAttributeValueFuturesToSession(Guid[] versionIds, NodeVersion outerVersionSelect)
        {
            Attribute attribAlias = null;
            Attribute aliasForString = null;
            AttributeStringValue stringsLoader = null;
            var strings = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeStringValues, () => stringsLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            Attribute aliasForLongString = null;
            AttributeStringValue longStringsLoader = null;
            var longStrings = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeLongStringValues, () => longStringsLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            Attribute aliasForInteger = null;
            AttributeIntegerValue integerLoader = null;
            var integers = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeIntegerValues, () => integerLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            Attribute aliasForDecimal = null;
            AttributeDecimalValue decimalLoader = null;
            var decimals = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeDecimalValues, () => decimalLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            Attribute aliasForDate = null;
            AttributeDateValue dateLoader = null;
            var dates = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeDateValues, () => dateLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();
        }

        public void RemoveRelation(IRelationById item, AbstractScopedCache repositoryScopedCache)
        {
            var sessionIdAsString = GetSessionId().ToString("n");
            using (DisposableTimer.TraceDuration<NhSessionHelper>("In RemoveRelation for session " + sessionIdAsString, "End RemoveRelation for session " + sessionIdAsString))
            {
                var relationType = GetOrCreateNodeRelationType(item.Type.RelationName, repositoryScopedCache);

                // Nh should handle this for us but got an error with mappings atm in SqlCe (APN 09/11/11)
                // Clear session cache to make sure it loads all of the tags in order to delete them all
                NhSession.Flush();

                // Clear the repository cache of this relation in case one has been added and then removed in the same unit of work
                var cacheKey = GenerateCacheKeyForRelation(item, relationType);
                repositoryScopedCache.InvalidateItems(cacheKey);

                var sourceIdValue = (Guid)item.SourceId.Value;
                var destinationIdValue = (Guid)item.DestinationId.Value;

                var existingRelation = GetDbRelation(relationType.Alias, sourceIdValue, destinationIdValue).ToArray();

                if (existingRelation.Any())
                {
                    existingRelation.ForEach(x => NhSession.Delete(x));
                }
            }
        }

        private static string GenerateCacheKeyForRelation(IRelationById item, NodeRelationType nodeRelationType)
        {
            return GenerateCacheKeyForRelation(nodeRelationType, item.SourceId, item.DestinationId);
        }

        private static string GenerateCacheKeyForRelation(NodeRelationType nodeRelationType, HiveId sourceId, HiveId destinationId)
        {
            return "NodeRelation-" + sourceId + "|" + destinationId + "|" + nodeRelationType.Alias;
        }

        public void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item, AbstractScopedCache repositoryScopedCache)
        {
            var sessionIdAsString = GetSessionId().ToString("n");
            using (DisposableTimer.TraceDuration<NhSessionHelper>("In AddRelation for session " + sessionIdAsString, "End AddRelation for session " + sessionIdAsString))
            {
                // Get the source and destination items from the Nh session
                var sourceNode = NhSession.Get<Node>((Guid)item.SourceId.Value);
                var destNode = NhSession.Get<Node>((Guid)item.DestinationId.Value);

                // Check the Nh session is already aware of the items
                if (sourceNode == null || destNode == null)
                {
                    string extraMessage = string.Empty;
                    if (sourceNode == null) extraMessage = "Source {0} cannot be found.\n".InvariantFormat(item.SourceId.Value);
                    if (destNode == null) extraMessage += "Destination {0} cannot be found.".InvariantFormat(item.DestinationId.Value);
                    throw new InvalidOperationException(
                        "Before adding a relation between source {0} and destination {1}, you must call AddOrUpdate with those items or they must already exist in the datastore.\n{2}"
                            .InvariantFormat(item.SourceId, item.DestinationId, extraMessage));
                }

                // Try to load an existing relation of the same type between the two
                var relationType = GetOrCreateNodeRelationType(item.Type.RelationName, repositoryScopedCache);

                // Grab the existing relation (if exists) using the compound key of start node / end node / relation type
                var cacheKey = GenerateCacheKeyForRelation(item, relationType);

                NodeRelation relationToReturn = repositoryScopedCache.GetOrCreateTyped(
                    cacheKey,
                    () =>
                    {
                        return NhSession
                            .QueryOver<NodeRelation>()
                            .Where(x => x.StartNode == sourceNode && x.EndNode == destNode && x.NodeRelationType == relationType)
                            .Cacheable()
                            .SingleOrDefault();
                    });

                // Avoid a duplicate by checking if one already exists
                if (relationToReturn != null)
                {
                    // Make sure existing relation has ordinal
                    relationToReturn.Ordinal = item.Ordinal;
                }
                else
                {
                    // Create a new relation
                    relationToReturn = new NodeRelation { StartNode = sourceNode, EndNode = destNode, NodeRelationType = relationType, Ordinal = item.Ordinal };
                    relationToReturn = NhSession.Merge(relationToReturn) as NodeRelation;
                }

                // Ensure metadata correct on existing or new entity
                CreateAndAddRelationTags(item, relationToReturn);
            }
        }

        private static void CreateAndAddRelationTags(IReadonlyRelation<IRelatableEntity, IRelatableEntity> incomingRelation, NodeRelation dbRelation)
        {
            dbRelation.NodeRelationTags.Clear();
            var newRelationMetadata = incomingRelation.MetaData.Select(x => new NodeRelationTag() { Name = x.Key, Value = x.Value, NodeRelation = dbRelation });
            newRelationMetadata.ForEach(x => dbRelation.NodeRelationTags.Add(x));
        }

        public void RemoveRelationsBiDirectional(Node node)
        {
            // For each relation, we need to remove the relation for the related node otherwise we get exceptions when deleting 
            // because of the way that Cascades are setup: MergeSaveAllDeleteOrphan.
            // If we don't do this we will get the exception that "deleted object would be re-saved by cascade" and this is because
            // the node on the other end of the relation is being saved as well because of our cascade options... 
            // To me, it would make more sense to change the cascade options to something else so that when you save a node it 
            // doesn't go re-save every other node that it's related to, however at this point in time it seems that we must have 'merge'
            // at least enabled but we also need delete and FluentNHibernate currently only has MergeSaveAllDeleteOrphan... perhaps it 
            // would be better to just have MergeDeleteOrphan ??
            foreach (var r in node.OutgoingRelations)
                r.EndNode.IncomingRelations.RemoveAll(x => x.StartNode.Id == node.Id);
            foreach (var r in node.OutgoingRelationCaches)
                r.EndNode.IncomingRelationCaches.RemoveAll(x => x.StartNode.Id == node.Id);
            foreach (var r in node.IncomingRelations)
                r.StartNode.OutgoingRelations.RemoveAll(x => x.EndNode.Id == node.Id);
            foreach (var r in node.IncomingRelationCaches)
                r.StartNode.OutgoingRelationCaches.RemoveAll(x => x.EndNode.Id == node.Id);

            node.ClearAllRelationsWithProxy();
        }

        public void MapAndMerge(AbstractEntity entity, MappingEngineCollection mappers)
        {
            using (DisposableTimer.TraceDuration<NhSessionHelper>("Start MapAndMerge for entity " + entity.Id, "End MapAndMerge"))
            using (NhProfilerLogging.Start(NhSession, "MapAndMerge",
                 new OdbcParameter("entity", entity)))
            {
                var rdbmsEntity = mappers.MapToIntent<IReferenceByGuid>(entity);

                // Track ID generation on the Rdbms object so that it can be pinged to the AbstractEntity upon Save/Update commit
                rdbmsEntity = NhSession.Merge(rdbmsEntity) as IReferenceByGuid;

                //InnerDataContext.NhibernateSession.SaveOrUpdate(rdbmsEntity);
                mappers.Map(rdbmsEntity, entity, rdbmsEntity.GetType(), entity.GetType());
            }
        }

        public void MapAndMerge<T>(Revision<T> entity, MappingEngineCollection mappers) where T : class, IVersionableEntity
        {
            HiveId hiveId = entity.MetaData != null ? entity.MetaData.Id : HiveId.Empty;
            HiveId entityId = entity.Item != null ? entity.Item.Id : HiveId.Empty;
            using (DisposableTimer.TraceDuration<NhSessionHelper>("Start MapAndMerge for revision " + hiveId + " entity " + entityId, "End MapAndMerge"))
            using (NhProfilerLogging.Start(NhSession, "MapAndMerge<T> (Revision<T>)",
                 new OdbcParameter("entity", entity)))
            {
                var rdbmsEntity = mappers.MapToIntent<IReferenceByGuid>(entity);

                // Track ID generation on the Rdbms object so that it can be pinged to the AbstractEntity upon Save/Update commit
                rdbmsEntity = NhSession.Merge(rdbmsEntity) as IReferenceByGuid;

                // 16th Jan 12 (APN) NH is not flushing if the above merged entity is queried before the transaction is committed, despite
                // the flushmode being Auto. So, explicit call to Flush here pending a bugfix/ better solution
                NhSession.Flush();

                //InnerDataContext.NhibernateSession.SaveOrUpdate(rdbmsEntity);
                mappers.Map(rdbmsEntity, entity, rdbmsEntity.GetType(), entity.GetType());
            }
        }

        public RelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            if (sourceId.Value.Type != HiveIdValueTypes.Guid || destinationId.Value.Type != HiveIdValueTypes.Guid)
                return null;

            using (NhProfilerLogging.Start(NhSession, "PerformFindRelation",
                 new OdbcParameter("sourceId", sourceId),
                 new OdbcParameter("destinationId", destinationId),
                 new OdbcParameter("relationType", relationType)))
            {
                // Reference the values here because otherwise it gets lost in NH's query cache
                var sourceValue = (Guid)sourceId.Value;
                var destValue = (Guid)destinationId.Value;
                var relationName = relationType.RelationName;

                var firstRelation = GetDbRelation(relationName, sourceValue, destValue).FirstOrDefault();

                return firstRelation != null ? MapNodeRelation(firstRelation) : null;
            }
        }

        private IEnumerable<NodeRelation> GetDbRelation(string relationName, Guid sourceValue, Guid destValue)
        {
            return NhSession.QueryOver<NodeRelation>()
                .Where(x => x.StartNode.Id == sourceValue)
                .And(x => x.EndNode.Id == destValue)
                .Fetch(x => x.StartNode).Lazy
                .Fetch(x => x.EndNode).Lazy
                .Fetch(x => x.NodeRelationTags).Eager
                .Fetch(x => x.NodeRelationType).Eager
                .JoinQueryOver(x => x.NodeRelationType)
                .Where(x => x.Alias == relationName)
                .Cacheable()
                .List()
                .Distinct(); // This query generates a cartesian product of the NodeRelationTags and NH 'handily' gives us x * NodeRelations so can't do a Take(1)
        }

        public IEnumerable<RelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            if (childId.Value.Type != HiveIdValueTypes.Guid) return Enumerable.Empty<RelationById>();

            using (NhProfilerLogging.Start(NhSession, "PerformGetParentRelations",
                new OdbcParameter("childId", childId),
                new OdbcParameter("relationType", relationType)))
            {
                var value = (Guid)childId.Value;
                // Reference the value here because otherwise it gets lost in NH's query cache

                var query = NhSession.QueryOver<NodeRelation>()
                    .Where(x => x.EndNode.Id == value)
                    .Fetch(x => x.StartNode).Lazy
                    .Fetch(x => x.EndNode).Lazy
                    .Fetch(x => x.NodeRelationTags).Eager
                    .Fetch(x => x.NodeRelationType).Eager;

                if (relationType != null)
                {
                    var relationName = relationType.RelationName;
                    var nodeRelations = query.JoinQueryOver<NodeRelationType>(x => x.NodeRelationType).Where(x => x.Alias == relationName).Cacheable().List();
                    return nodeRelations.Select(MapNodeRelation).ToList();
                }

                var relations = query.Cacheable().List();
                return relations.Select(MapNodeRelation).ToList();
            }
        }

        public IEnumerable<RelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            if (parentId.Value.Type != HiveIdValueTypes.Guid) return Enumerable.Empty<RelationById>();

            using (NhProfilerLogging.Start(NhSession, "PerformGetChildRelations",
                new OdbcParameter("parentId", parentId),
                new OdbcParameter("relationType", relationType)))
            {
                var value = (Guid)parentId.Value;
                // Reference the value here because otherwise it gets lost in NH's query cache

                var query = NhSession.QueryOver<NodeRelation>()
                    .Where(x => x.StartNode.Id == value)
                    .Fetch(x => x.StartNode).Lazy
                    .Fetch(x => x.EndNode).Lazy
                    .Fetch(x => x.NodeRelationTags).Eager
                    .Fetch(x => x.NodeRelationType).Eager;

                if (relationType != null)
                {
                    var relationName = relationType.RelationName;
                    var listWithType = query.JoinQueryOver<NodeRelationType>(x => x.NodeRelationType)
                        .Where(x => x.Alias == relationName)
                        .Cacheable()
                        .List();
                    return listWithType.Select(MapNodeRelation);
                }

                var list = query
                    .Cacheable()
                    .List();

                return list.Select(MapNodeRelation);
            }
        }

        public IEnumerable<RelationById> PerformGetBranchRelations(HiveId siblingId, RelationType relationType = null)
        {
            if (siblingId.Value.Type != HiveIdValueTypes.Guid) return Enumerable.Empty<RelationById>();

            using (NhProfilerLogging.Start(NhSession, "PerformGetBranchRelations",
                new OdbcParameter("siblingId", siblingId),
                new OdbcParameter("relationType", relationType)))
            {
                var value = (Guid)siblingId.Value;
                // Reference the value here because otherwise it gets lost in NH's query cache

                var query = NhSession.QueryOver<NodeRelation>()
                   .Fetch(x => x.StartNode).Lazy
                   .Fetch(x => x.EndNode).Lazy
                   .Fetch(x => x.NodeRelationTags).Eager
                   .Fetch(x => x.NodeRelationType).Eager;

                var parentQuery = QueryOver.Of<NodeRelation>().Where(x => x.EndNode.Id == value);

                if (relationType != null)
                {
                    var copyRelationName = relationType.RelationName;

                    var parentQueryWithType = parentQuery
                        .JoinQueryOver(x => x.NodeRelationType)
                        .Where(x => x.Alias == copyRelationName);

                    return query.WithSubquery.WhereProperty(x => x.StartNode.Id).In(parentQueryWithType.Select(x => x.StartNode.Id).Take(1))
                        .CacheRegion("Relations").Cacheable().List().Select(MapNodeRelation);
                }
                else
                {
                    return query.WithSubquery.WhereProperty(x => x.StartNode.Id).In(parentQuery.Select(x => x.StartNode.Id).Take(1))
                        .CacheRegion("Relations").Cacheable().List().Select(MapNodeRelation);
                }
            }
        }

        private class SimpleRelation
        {
            public Guid StartNodeId { get; set; }
            public Guid EndNodeId { get; set; }
            public int Ordinal { get; set; }
            public RelationMetaDatum[] NodeRelationTags { get; set; }
        }

        private static RelationById MapNodeRelation(SimpleRelation nodeRelation, string aliasName)
        {
            return new RelationById(
                new HiveId(nodeRelation.StartNodeId),
                new HiveId(nodeRelation.EndNodeId),
                new RelationType(aliasName),
                nodeRelation.Ordinal,
                nodeRelation.NodeRelationTags);
        }

        private RelationById MapNodeRelation(NodeRelation nodeRelation)
        {
            IRelatableEntity startMapped;
            IRelatableEntity endMapped;

            var nodeRelationTags = MapRelationTags(nodeRelation);

            return new RelationById(new HiveId(nodeRelation.StartNode.Id), new HiveId(nodeRelation.EndNode.Id), new RelationType(nodeRelation.NodeRelationType.Alias), nodeRelation.Ordinal, nodeRelationTags);

            if (nodeRelation.StartNode is AttributeSchemaDefinition)
            {
                startMapped = FrameworkContext.TypeMappers.MapToIntent<EntitySchema>(nodeRelation.StartNode);
            }
            else
            {
                startMapped =
                    FrameworkContext.TypeMappers.Map<TypedEntity>(
                        nodeRelation.StartNode.NodeVersions.OrderByDescending(x => x.DateCreated).FirstOrDefault());
            }

            if (nodeRelation.EndNode is AttributeSchemaDefinition)
            {
                endMapped = FrameworkContext.TypeMappers.MapToIntent<EntitySchema>(nodeRelation.EndNode);
            }
            else
            {
                endMapped =
                    FrameworkContext.TypeMappers.Map<TypedEntity>(
                        nodeRelation.EndNode.NodeVersions.OrderByDescending(x => x.DateCreated).FirstOrDefault());
            }

            //TODO: Need to move to RdbmsModelMapper but doesn't seem to be wired up at the moment
            var nodeRelations =
                nodeRelation.NodeRelationTags.Select(x => new RelationMetaDatum(x.Name, x.Value)).ToArray();

            return new Relation(new RelationType(nodeRelation.NodeRelationType.Alias), startMapped, endMapped,
                                nodeRelation.Ordinal, nodeRelations);
        }

        private static RelationMetaDatum[] MapRelationTags(NodeRelation nodeRelation)
        {
            var nodeRelationTags =
                nodeRelation.NodeRelationTags.Select(x => new RelationMetaDatum(x.Name, x.Value)).ToArray();
            return nodeRelationTags;
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            NhEventListeners.RemoveNodeIdHandler(this);
        }

        #endregion
    }
}