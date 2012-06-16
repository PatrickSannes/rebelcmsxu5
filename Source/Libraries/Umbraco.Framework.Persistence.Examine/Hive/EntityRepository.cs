using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.SearchCriteria;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Examine.Linq;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine.Hive
{
    using Lucene.Net.Search;
    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;
    using global::Examine.LuceneEngine.SearchCriteria;

    public class EntityRepository : AbstractEntityRepository
    {
        public ExamineHelper Helper { get; private set; }
        protected ExamineTransaction ExamineTransaction { get; private set; }

        public EntityRepository(
            ProviderMetadata providerMetadata,
            IProviderTransaction providerTransaction,
            IFrameworkContext frameworkContext,
            AbstractRevisionRepository<TypedEntity> revisionRepository,
            AbstractSchemaRepository schemaSession,
            ExamineHelper helper)
            : base(providerMetadata, providerTransaction, revisionRepository, schemaSession, frameworkContext)
        {
            Helper = helper;
            ExamineTransaction = providerTransaction as ExamineTransaction;
            Mandate.That(ExamineTransaction != null, x => new InvalidCastException("The IProviderTransaction for the Examine EntityRepository must be of Type ExamineTransaction"));
        }

        protected override void DisposeResources()
        {
            Schemas.Dispose();
            Revisions.Dispose();
            Transaction.Dispose();
        }

        public override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
        {
            return Helper.PerformGet<T>(allOrNothing, FixedIndexedFields.EntityId, ids);
        }

        public override IEnumerable<T> PerformExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var direction = query.From.HierarchyScope;

            var criteria = new ExamineQueryVisitor(Helper.ExamineManager).Visit(query.Criteria);

            // Include revision status in query
            var revisionCriteria = Helper.ExamineManager.CreateSearchCriteria();
            revisionCriteria.Must().Field(FixedRevisionIndexFields.RevisionStatusAlias, query.From.RevisionStatus);

            IQuery finalQuery = null;

            // Only include the revision status if it was included in the query
            if (query.From.RevisionStatus != FromClause.RevisionStatusNotSpecified)
            {
                finalQuery = (criteria.ToString() == "") ? revisionCriteria.Compile() : ((LuceneSearchCriteria)criteria).Join((LuceneSearchCriteria)revisionCriteria.Compile(), BooleanClause.Occur.MUST); ;
            }
            else
            {
                finalQuery = criteria;
            }

            IEnumerable<SearchResult> results = Helper.ExamineManager.Search(finalQuery);

            // Now apply an in-memory filter to account for some of the shortcomings of Lucene (e.g. no MAX support)
            var detailedResults = results.Select(x => new
                {
                    EntityId = HiveId.Parse(x.Fields.GetValue(FixedIndexedFields.EntityId, HiveId.Empty.ToString())),
                    RevisionId = HiveId.Parse(x.Fields.GetValue(FixedRevisionIndexFields.RevisionId, HiveId.Empty.ToString())),
                    UtcStatusChanged = ExamineHelper.FromExamineDateTime(x.Fields, FixedIndexedFields.UtcStatusChanged),
                    Result = x
                }).ToArray();

            IEnumerable<SearchResult> maxByDate = detailedResults.GroupBy(x => x.EntityId, 
                (key, matches) => matches.OrderByDescending(x => x.UtcStatusChanged).FirstOrDefault())
                .Select(x => x.Result)
                .ToArray();

            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Count:
                    //this is weird but returns an integer
                    return new[] { maxByDate.Count() }.Cast<T>();
                case ResultFilterType.Take:
                    maxByDate = maxByDate.Take(query.ResultFilter.SelectorArgument);
                    break;
            }

            if (typeof(T).IsAssignableFrom(query.ResultFilter.ResultType))
            {
                return maxByDate.Distinct().Select(node => FrameworkContext.TypeMappers.Map<T>(node));
            }

            return Enumerable.Empty<T>();
        }

        public override T PerformExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Count:
                    //this is weird but returns an integer
                    return ExecuteMany<T>(query, objectBinder).Single();
            }

            var many = ExecuteMany<T>(query, objectBinder);

            return many.Single();
        }

        public override T PerformExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var results = ExecuteMany<T>(query, objectBinder);

            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Single:
                case ResultFilterType.SingleOrDefault:
                    if (!results.Any())
                    {
                        if (query.ResultFilter.ResultFilterType == ResultFilterType.SingleOrDefault)
                        {
                            return default(T);
                        }
                        throw new InvalidOperationException("Sequence contains 0 elements but query specified exactly 1 must be present");
                    }
                    var count = results.Count();
                    if (count > 1)
                    {
                        throw new InvalidOperationException("Sequence contains {0} elements but query specified exactly 1 must be present.".InvariantFormat(count));
                    }
                    return results.Single();
            }

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Returns all entities for the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override IEnumerable<T> PerformGetAll<T>()
        {
            return Helper.PerformGetAll<T>(typeof(TypedEntity).Name);
        }

        public override bool CanReadRelations
        {
            get { return true; }
        }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return Helper.PeformGetParentRelations(childId, relationType);
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return Helper.PerformFindRelation(sourceId, destinationId, relationType);
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
            return Helper.Exists<TEntity>(id, FixedIndexedFields.EntityId);
        }

        protected override void PerformAddOrUpdate(TypedEntity entity)
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
                Helper.PerformAddOrUpdate(entity, ExamineTransaction);
            }
        }

        protected override void PerformDelete<T>(HiveId id)
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