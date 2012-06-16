using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.NHibernate.Dependencies;
using Umbraco.Framework.Persistence.NHibernate.Linq;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Persistence.RdbmsModel;
using Umbraco.Hive;
using Umbraco.Hive.ProviderSupport;
using Attribute = Umbraco.Framework.Persistence.RdbmsModel.Attribute;

namespace Umbraco.Framework.Persistence.NHibernate
{
    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;

    public class EntityRepository : AbstractEntityRepository
    {
        public EntityRepository(ProviderMetadata providerMetadata, 
            AbstractSchemaRepository schemas, 
            AbstractRevisionRepository<TypedEntity> revisions, 
            IProviderTransaction providerTransaction, 
            ISession nhSession, 
            IFrameworkContext frameworkContext,
            bool isReadOnly)
            : base(providerMetadata, providerTransaction, revisions, schemas, frameworkContext)
        {
            Helper = new NhSessionHelper(nhSession, frameworkContext);
            IsReadonly = isReadOnly;

#if DEBUG
            if (schemas is SchemaRepository)
            {
                var sesh = schemas as SchemaRepository;
                if (sesh.Helper.NhSession != Helper.NhSession)
                    throw new InvalidOperationException("NHibernate provider can only be used in conjunction with an NHibernate schema provider when they share the same NHibernate session");
            }
#endif
        }

        protected bool IsReadonly { get; set; }

        protected internal NhSessionHelper Helper { get; set; }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            Helper.IfNotNull(x => x.Dispose());
            Schemas.Dispose();
            Revisions.Dispose();
            Transaction.Dispose();
        }

        public override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
        {
            Mandate.ParameterNotNull(ids, "ids");
            ids.ForEach(x => Mandate.ParameterNotEmpty(x, "id"));

            // We don't just ask for the Node by id here because some other types inherit from Node
            // like AttributeSchemaDefinition. Therefore, a Node that represents a TypedEntity is said
            // to exist if a NodeVersion exists
            Guid[] nodeIds = ids.Where(x => x.Value.Type == HiveIdValueTypes.Guid).Select(x => (Guid)x.Value).ToArray();
            var nodeVersions = Helper.GetNodeVersionsByStatusDesc(nodeIds, limitToLatestRevision: true).ToArray();

            return nodeIds.Select(x => nodeVersions.SingleOrDefault(y => y.Node.Id == x)).WhereNotNull().Select(x => FrameworkContext.TypeMappers.Map<T>(x));
        }

        public override IEnumerable<T> PerformExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var direction = query.From.HierarchyScope;

            var disconnected = new NhQueryOverVisitor().Visit(query.Criteria);

            var revisionStatus = query.From.RevisionStatusType != FromClause.RevisionStatusNotSpecifiedType ? query.From.RevisionStatusType : null;

            var nodeIds = GetNodeIds(query);

            var buildVersionedQuery = Helper.GenerateVersionedQueryPlusAttributes(nodeIds, revisionStatus: revisionStatus, limitToLatestRevision: true, sortClauses: query.SortClauses);

            // If the criteria is null then the disconnected Id filter will be null too
            var queryOver = disconnected != null
                                ? buildVersionedQuery.WithSubquery.WhereProperty(x => x.NodeVersion.Id).In(disconnected)
                                : buildVersionedQuery;

            IEnumerable<NodeVersion> resultBuilder = Enumerable.Empty<NodeVersion>();

            // Need to order in memory using the materialised results because the field name is the value of a column in the resultset, not a column itself
            // First materialise the results. Note that the Take / Skip is inefficient atm; NH bugs in Skip / Take in 3.0 causing same results irrespective of request, so enumerating whole list and skipping in memory (ouch)
            // https://nhibernate.jira.com/browse/NH-2917
            resultBuilder = queryOver.List().Distinct().ToArray();
            resultBuilder = OrderMaterialisedResults(query, resultBuilder);

            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Take:
                    resultBuilder = resultBuilder.Take(query.ResultFilter.SelectorArgument);
                    break;
                case ResultFilterType.Skip:
                    resultBuilder = resultBuilder.Skip(query.ResultFilter.SelectorArgument);
                    break;
                default:
                    //resultBuilder = queryOver.List();
                    break;
            }

            //if (typeof(T).IsAssignableFrom(query.ResultFilter.ResultType))
            if (TypeFinder.IsTypeAssignableFrom(query.ResultFilter.ResultType, typeof(T)))
            {
                var nodeVersions = resultBuilder.Distinct();
                return nodeVersions.Select(node => FrameworkContext.TypeMappers.Map<T>(node));
                //return resultBuilder.Select(node => objectBinder.Execute(_fieldBinderFactory.Invoke(node))).Cast<T>();
            }

            return Enumerable.Empty<T>();
        }

        private static IEnumerable<NodeVersion> OrderMaterialisedResults(QueryDescription query, IEnumerable<NodeVersion> resultBuilder)
        {
            IOrderedEnumerable<NodeVersion> orderedResultBuilder = null;

            foreach (var source in query.SortClauses.OrderBy(x => x.Priority))
            {
                if (orderedResultBuilder != null)
                    if (source.Direction == SortDirection.Ascending)
                        orderedResultBuilder = orderedResultBuilder.ThenBy(x => x,
                                                                           new AttributeOrderingComparer(
                                                                               source.FieldSelector.FieldName,
                                                                               source.FieldSelector.ValueKey));
                    else
                        orderedResultBuilder = orderedResultBuilder.ThenByDescending(x => x,
                                                                                     new AttributeOrderingComparer(
                                                                                         source.FieldSelector.FieldName,
                                                                                         source.FieldSelector.ValueKey));
                else if (source.Direction == SortDirection.Ascending)
                    orderedResultBuilder = resultBuilder.OrderBy(x => x,
                                                                 new AttributeOrderingComparer(source.FieldSelector.FieldName,
                                                                                               source.FieldSelector.ValueKey));
                else
                    orderedResultBuilder = resultBuilder.OrderByDescending(x => x,
                                                                           new AttributeOrderingComparer(
                                                                               source.FieldSelector.FieldName,
                                                                               source.FieldSelector.ValueKey));
            }

            // If we've prepared an ordering, execute it here
            resultBuilder = orderedResultBuilder != null ? orderedResultBuilder.ToArray() : resultBuilder;
            return resultBuilder;
        }

        class AttributeOrderingComparer : IComparer<NodeVersion>
        {
            private readonly string _fieldAlias;
            private string _fieldSubAlias;

            public AttributeOrderingComparer(string fieldAlias, string fieldSubAlias = null)
            {
                _fieldAlias = fieldAlias;
                _fieldSubAlias = fieldSubAlias ?? "Value";
            }

            public int Compare(NodeVersion x, NodeVersion y)
            {
                int comparison;

                var leftAttrib = GetFirstAttribute(x);
                var rightAttrib = GetFirstAttribute(y);

                if (BreakEarly(leftAttrib, rightAttrib, out comparison)) return comparison;

                var leftLongStringVal = GetFirstLongString(leftAttrib);
                var rightLongStringVal = GetFirstLongString(rightAttrib);

                if (BreakEarly(leftLongStringVal, rightLongStringVal, out comparison, 
                    (left, right) => String.Compare(left.Value, right.Value, StringComparison.InvariantCultureIgnoreCase))) 
                    return comparison;


                var leftStringVal = GetFirstString(leftAttrib);
                var rightStringVal = GetFirstString(rightAttrib);

                if (BreakEarly(leftStringVal, rightStringVal, out comparison,
                    (left, right) => String.Compare(left.Value, right.Value, StringComparison.InvariantCultureIgnoreCase)))
                    return comparison;

                var leftDateVal = GetFirstDate(leftAttrib);
                var rightDateVal = GetFirstDate(rightAttrib);

                if (BreakEarly(leftDateVal, rightDateVal, out comparison,
                    (left, right) => DateTimeOffset.Compare(left.Value, right.Value)))
                    return comparison;

                return 0;
            }

            private static AttributeLongStringValue GetFirstLongString(Attribute leftAttrib)
            {
                return leftAttrib.AttributeLongStringValues.FirstOrDefault();
            }

            private static AttributeStringValue GetFirstString(Attribute leftAttrib)
            {
                return leftAttrib.AttributeStringValues.FirstOrDefault();
            }

            private static AttributeDateValue GetFirstDate(Attribute leftAttrib)
            {
                return leftAttrib.AttributeDateValues.FirstOrDefault();
            }

            private static bool BreakEarly<T>(
                T leftVal, 
                T rightVal,
                out int compare,
                Func<T, T, int> comparisonIfBothNotNull = null)
                where T : AbstractEquatableObject<T>
            {
                if (rightVal == null && leftVal == null)
                {
                    compare = 0;
                    return false;
                }

                if (rightVal == null)
                {
                    compare = 1;
                    return true;
                }

                if (leftVal == null)
                {
                    compare = -1;
                    return true;
                }
                compare = comparisonIfBothNotNull != null ? comparisonIfBothNotNull.Invoke(leftVal, rightVal) : 0;
                return compare != 0;
            }

            private Attribute GetFirstAttribute(NodeVersion x)
            {
                return x.Attributes.FirstOrDefault(att => att.AttributeDefinition.Alias.InvariantEquals(_fieldAlias));
            }
        }

        private static Guid[] GetNodeIds(QueryDescription query)
        {
            var entityIds = query.From.RequiredEntityIds;

            var nodeIds = entityIds.Any() ? entityIds.Select(x => (Guid)x.Value).ToArray() : null;
            return nodeIds;
        }

        public override T PerformExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var disconnected = new NhQueryOverVisitor().Visit(query.Criteria);
            var revisionStatus = query.From.RevisionStatusType != FromClause.RevisionStatusNotSpecifiedType ? query.From.RevisionStatusType : null;

            NodeVersion outerVersionSelectorAlias = null;
            var nodeIds = GetNodeIds(query);
            var buildVersionedQuery = Helper.GenerateVersionedQueryPlusAttributes(nodeIds, revisionStatus: revisionStatus, limitToLatestRevision: true);

            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Any:
                    var queryOverAny = buildVersionedQuery.WithSubquery.WhereProperty(x => x.NodeVersion.Id).In(disconnected);
                    return (T)(object)(queryOverAny.ToRowCountQuery().RowCount() != 0);

                case ResultFilterType.All:
                    var queryOverAll = buildVersionedQuery.WithSubquery.WhereProperty(x => x.NodeVersion.Id).In(disconnected);
                    var totalRowCount = Helper.NhSession.QueryOver<NodeVersion>().RowCountInt64();
                    var filteredRowCount = queryOverAll.ToRowCountQuery().RowCountInt64();
                    return (T)(object)(filteredRowCount > 0 && filteredRowCount == totalRowCount);

                case ResultFilterType.Count:
                    // The query returns a cartesian product, so get the ids and count distinct in memory.
                    // Later need to switch to a "count distinct" but SqlCe doesn't support it ffs

                    var countQuery = buildVersionedQuery
                        .WithSubquery.WhereProperty(x => x.NodeVersion.Id)
                        .In(disconnected)
                        .Select(x => x.Id);

                    countQuery.UnderlyingCriteria.SetComment("For distinct counting");

                    return (T)(object)countQuery.List<Guid>().Distinct().Count();
            }

            var many = ExecuteMany<T>(query, objectBinder);

            return many.Single();
        }

        public override T PerformExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var disconnected = new NhQueryOverVisitor().Visit(query.Criteria);

            var revisionStatus = query.From.RevisionStatusType != FromClause.RevisionStatusNotSpecifiedType ? query.From.RevisionStatusType : null;
            var nodeIds = GetNodeIds(query);
            var buildVersionedQuery = Helper.GenerateVersionedQueryPlusAttributes(nodeIds, revisionStatus: revisionStatus, limitToLatestRevision: true);

            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Single:
                case ResultFilterType.SingleOrDefault:
                    var queryOver = disconnected != null
                        ? buildVersionedQuery.WithSubquery.WhereProperty(x => x.NodeVersion.Id).In(disconnected)
                        : buildVersionedQuery;

                    var singleResults = queryOver.List().Distinct();

                    singleResults = OrderMaterialisedResults(query, singleResults);

                    try
                    {
                        //var singleItem = queryOver.SingleOrDefault<NodeVersion>();
                        if (singleResults.Count() > 1)
                            throw new NonUniqueResultException(singleResults.Count());

                        var singleItem = singleResults.Take(1).FirstOrDefault(); // buildVersionedQuery is a cartesian product so can't tell Nh to filter in the db otherwise Nh handily caches only the first attribute value row

                        if (ReferenceEquals(singleItem, null))
                        {
                            if (query.ResultFilter.ResultFilterType == ResultFilterType.Single)
                            {
                                throw new InvalidOperationException("Sequence contains 0 elements but query specified exactly 1 must be present");
                            }
                            return default(T);
                        }

                        return FrameworkContext.TypeMappers.Map<T>(singleItem);
                    }
                    catch (NonUniqueResultException ex)
                    {
                        const string nastyNhExceptionMessage = "query did not return a unique result: ";
                        var getNumberFromNastyNHMessage = ex.Message.Replace(nastyNhExceptionMessage, "");
                        throw new InvalidOperationException("Sequence contains {0} elements but query specified exactly 1 must be present.".InvariantFormat(getNumberFromNastyNHMessage), ex);
                    }
                    break;
                case ResultFilterType.First:
                case ResultFilterType.FirstOrDefault:
                    var firstQuery = disconnected != null
                        ? buildVersionedQuery.WithSubquery.WhereProperty(x => x.NodeVersion.Id).In(disconnected)
                        : buildVersionedQuery;

                    var results = firstQuery.List().Distinct();

                    results = OrderMaterialisedResults(query, results);

                    //var firstItem = firstQuery.Take(1).SingleOrDefault<NodeVersion>();
                    var firstItem = results.Take(1).FirstOrDefault(); // buildVersionedQuery is a cartesian product so can't tell Nh to filter in the db otherwise Nh handily caches only the first attribute value row

                    if (ReferenceEquals(firstItem, null))
                    {
                        if (query.ResultFilter.ResultFilterType == ResultFilterType.First)
                        {
                            throw new InvalidOperationException("Sequence contains 0 elements when non-null First element was required");
                        }
                        return default(T);
                    }

                    return FrameworkContext.TypeMappers.Map<T>(firstItem);

                    break;
                case ResultFilterType.Last:
                case ResultFilterType.LastOrDefault:
                    var lastQuery = buildVersionedQuery.WithSubquery.WhereProperty(x => x.NodeVersion.Id).In(disconnected);
                    //var firstItem = firstQuery.Take(1).SingleOrDefault<NodeVersion>();
                    var lastItem = lastQuery.List().LastOrDefault(); // buildVersionedQuery is a cartesian product so can't tell Nh to filter in the db otherwise Nh handily caches only the first attribute value row

                    if (ReferenceEquals(lastItem, null))
                    {
                        if (query.ResultFilter.ResultFilterType == ResultFilterType.Last)
                        {
                            throw new InvalidOperationException("Sequence contains 0 elements when non-null First element was required");
                        }
                        return default(T);
                    }

                    return FrameworkContext.TypeMappers.Map<T>(lastItem);

                    break;
            }

            return ExecuteMany<T>(query, objectBinder).FirstOrDefault();
        }

        public override IEnumerable<T> PerformGetAll<T>()
        {
            var outerQuery = Helper.GetNodeVersionsByStatusDesc();

            return outerQuery.Select(x => FrameworkContext.TypeMappers.Map<T>(x));
        }

        public override bool Exists<TEntity>(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var value = (Guid)id.Value;

            var qo = Helper.NhSession.QueryOver<NodeVersion>()
                .Where(x => x.Node.Id == value)
                .Select(Projections.RowCount())
                .Cacheable()
                .SingleOrDefault<int>();

            return qo > 0;
        }

        public override bool CanReadRelations
        {
            get { return true; }
        }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return Helper.PerformGetParentRelations(childId, relationType);
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return Helper.PerformFindRelation(sourceId, destinationId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            return GetParentRelations(descendentId, relationType).SelectRecursive(x => GetParentRelations(x.SourceId, relationType));
        }

        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            var childRelations = GetChildRelations(ancestorId, relationType).ToArray();
            return childRelations.SelectRecursive(x =>
                                                      {
                                                          var childRelationsSub = GetChildRelations(x.DestinationId, relationType).ToArray();
                                                          return childRelationsSub;
                                                      });
        }

        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            return Helper.PerformGetChildRelations(parentId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetBranchRelations(HiveId siblingId, RelationType relationType = null)
        {
            return Helper.PerformGetBranchRelations(siblingId, relationType);
        }

        protected override void PerformAddOrUpdate(TypedEntity entity)
        {
            Mandate.ParameterNotNull(entity, "persistedEntity");

            using (NhProfilerLogging.Start(Helper.NhSession, "PerformAddOrUpdate",
                 new OdbcParameter("entity", entity)))
            {
                // Note that it should be the caller's responsibility to add to revisions but the Cms backoffice code needs to change
                // to do that, so this is included to avoid breaking assumptions about auto-created versions until then
                if (Revisions.CanWrite)
                {
                    var newRevision = new Revision<TypedEntity>(entity);
                    Revisions.AddOrUpdate(newRevision);
                    return;
                }
                else
                {
                    if (TryUpdateExisting(entity)) return;

                    Helper.MapAndMerge(entity, FrameworkContext.TypeMappers);
                }
            }
        }

        private bool TryUpdateExisting(AbstractEntity persistedEntity)
        {
            var mappers = FrameworkContext.TypeMappers;

            // Get the entity with matching Id, provided the incoming Id is not null / empty
            if (!persistedEntity.Id.IsNullValueOrEmpty())
            {
                Type rdbmsType;
                if (mappers.TryGetDestinationType(persistedEntity.GetType(), typeof(IReferenceByGuid), out rdbmsType))
                {
                    //// Temp hack for testing
                    //if (typeof(NodeVersion) == rdbmsType && typeof(TypedEntity) == persistedEntity.GetType())
                    //{
                    //    rdbmsType = typeof(Node);

                    //    var nodeVersions = global::NHibernate.Linq.LinqExtensionMethods.Query<NodeVersion>(InnerDataContext.NhibernateSession).Where(x => x.Node.Id == persistedEntity.Id.AsGuid);
                    //    var firstOrDefault = nodeVersions.FirstOrDefault();
                    //    if (firstOrDefault == null) return false;

                    //    var latest = GetMostRecentVersionFromQuery(firstOrDefault.Node);
                    //    if (latest != null)
                    //    {
                    //        mappers.Map(persistedEntity, latest, persistedEntity.GetType(), latest.GetType());
                    //        //InnerDataContext.NhibernateSession.Evict(latest);
                    //        latest = InnerDataContext.NhibernateSession.Merge(latest) as NodeVersion;
                    //        //InnerDataContext.NhibernateSession.SaveOrUpdate(existingEntity);
                    //        mappers.Map(latest, persistedEntity, latest.GetType(), persistedEntity.GetType());
                    //        SetOutgoingId(persistedEntity);
                    //        //_trackNodePostCommits.Add((IReferenceByGuid)existingEntity, persistedEntity);
                    //        return true;
                    //    }
                    //}

                    var existingEntity = Helper.NhSession.Get(rdbmsType, (Guid)persistedEntity.Id.Value);
                    if (existingEntity != null)
                    {
                        mappers.Map(persistedEntity, existingEntity, persistedEntity.GetType(), existingEntity.GetType());
                        existingEntity = Helper.NhSession.Merge(existingEntity);
                        //InnerDataContext.NhibernateSession.SaveOrUpdate(existingEntity);
                        mappers.Map(existingEntity, persistedEntity, existingEntity.GetType(), persistedEntity.GetType());
                        // ##API2: Disabled: SetOutgoingId(persistedEntity);
                        //_trackNodePostCommits.Add((IReferenceByGuid)existingEntity, persistedEntity);
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void PerformDelete<T>(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            if (id.Value.Type != HiveIdValueTypes.Guid) return;

            // We don't issue a direct-to-db deletion because otherwise NH can't keep track
            // of any cascading deletes
            // InnerDataContext.NhibernateSession.Delete(destinationType, entityId.AsGuid);

            object nhObject;

            nhObject = Helper.NhSession.Get<Node>((Guid)id.Value);
            var node = (Node)nhObject;
            node.NodeVersions.EnsureClearedWithProxy();
            Helper.RemoveRelationsBiDirectional(node);

            Helper.NhSession.Delete(nhObject);
        }

        public override bool CanWriteRelations
        {
            get { return true; }
        }

        protected override void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            Helper.AddRelation(item, this.RepositoryScopedCache);
        }

        protected override void PerformRemoveRelation(IRelationById item)
        {
            Helper.RemoveRelation(item, this.RepositoryScopedCache);
        }


    }
}