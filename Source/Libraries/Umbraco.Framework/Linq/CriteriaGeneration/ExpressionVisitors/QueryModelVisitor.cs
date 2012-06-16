namespace Umbraco.Framework.Linq.CriteriaGeneration.ExpressionVisitors
{
    using System;

    using System.Linq;

    using System.Linq.Expressions;

    using Remotion.Linq;

    using Remotion.Linq.Clauses;
    using Remotion.Linq.Clauses.Expressions;
    using Remotion.Linq.Clauses.ResultOperators;

    using Umbraco.Framework.Data;
    using Umbraco.Framework.Expressions.Remotion;
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;

    using Umbraco.Framework.Linq.QueryModel;

    /// <summary>
    /// Visits a <see cref="QueryModel"/> object and rewrites it into a <see cref="QueryDescription"/> object.
    /// </summary>
    /// <remarks></remarks>
    public class QueryModelVisitor : QueryModelVisitorBase
    {
        private QueryDescriptionBuilder _queryDescription;
        private AbstractQueryStructureBinder _structureBinder;
        private AbstractExpressionTreeVisitor _treeVisitor;

        protected AbstractExpressionTreeVisitor TreeVisitor
        {
            get { return _treeVisitor; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryModelVisitor"/> class.
        /// </summary>
        /// <param name="structureBinder">The structure binder.</param>
        /// <param name="treeVisitor">The tree visitor.</param>
        /// <remarks></remarks>
        public QueryModelVisitor(AbstractQueryStructureBinder structureBinder, AbstractExpressionTreeVisitor treeVisitor)
        {
            _structureBinder = structureBinder;
            _treeVisitor = treeVisitor;
        }

        protected AbstractQueryStructureBinder StructureBinder
        {
            get { return _structureBinder; }
        }

        /// <summary>
        /// Creates a new <see cref="QueryModelVisitor"/>, and returns the result of running <see cref="QueryModelVisitor.VisitAndGenerateQueryDescription"/> on the
        /// supplied <paramref name="queryModel"/>.
        /// </summary>
        /// <param name="queryModel">The query model.</param>
        /// <param name="structureBinder">The structure binder.</param>
        /// <param name="treeVisitor">The tree visitor.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static QueryDescription FromQueryModel(Remotion.Linq.QueryModel queryModel, AbstractQueryStructureBinder structureBinder, AbstractExpressionTreeVisitor treeVisitor)
        {
            var elrmv = new QueryModelVisitor(structureBinder, treeVisitor);
            return elrmv.VisitAndGenerateQueryDescription(queryModel);
        }

        /// <summary>
        /// Creates a new <see cref="QueryModelVisitor"/>, and returns the result of running <see cref="QueryModelVisitor.VisitAndGenerateQueryDescription"/> on the
        /// supplied <paramref name="queryModel"/>.
        /// </summary>
        /// <param name="queryModel">The query model.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static QueryDescription FromQueryModel(Remotion.Linq.QueryModel queryModel)
        {
            Func<AbstractQueryStructureBinder> generateBinder = () =>
                {

                    var resultType = queryModel.MainFromClause.ItemType;
                    var structureBinderAttrib =
                        resultType.GetCustomAttributes(typeof(QueryStructureBinderOfTypeAttribute),
                                                       true).OfType<QueryStructureBinderOfTypeAttribute>().FirstOrDefault();

                    if (structureBinderAttrib == null) return new DefaultQueryStructureBinder();

                    return Activator.CreateInstance(structureBinderAttrib.StructureBinder) as AbstractQueryStructureBinder;
                };

            var defaultQueryStructureBinder = generateBinder.Invoke();
            return FromQueryModel(queryModel, defaultQueryStructureBinder, new DefaultExpressionTreeVisitor(defaultQueryStructureBinder));
        }

        /// <summary>
        /// Visits the <paramref name="queryModel"/> and generates a <see cref="QueryDescription"/>.
        /// </summary>
        /// <param name="queryModel">The query model.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public QueryDescription VisitAndGenerateQueryDescription(Remotion.Linq.QueryModel queryModel)
        {
            // Reset description builder
            _queryDescription = new QueryDescriptionBuilder();

            VisitQueryModel(queryModel);

            return _queryDescription;
        }

        /// <summary>
        /// Visits the additional from clause.
        /// </summary>
        /// <param name="fromClause">From clause.</param>
        /// <param name="queryModel">The query model.</param>
        /// <param name="index">The index.</param>
        /// <remarks></remarks>
        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, Remotion.Linq.QueryModel queryModel, int index)
        {
            base.VisitAdditionalFromClause(fromClause, queryModel, index);
        }

        /// <summary>
        /// Visits the main from clause.
        /// </summary>
        /// <param name="fromClause">From clause.</param>
        /// <param name="queryModel">The query model.</param>
        /// <remarks></remarks>
        public override void VisitMainFromClause(MainFromClause fromClause, Remotion.Linq.QueryModel queryModel)
        {
            var sourceAsConstant = queryModel.MainFromClause.FromExpression as ConstantExpression;

            if (sourceAsConstant != null)
            {
                _queryDescription.SetFromClause("", HierarchyScope.AllOrNone, FromClause.RevisionStatusNotSpecifiedType);
                // Tues 1 Nov: _queryDescription.SetResultFilterClause(fromClause.ItemType, ResultFilterType.Sequence, -1);
                _queryDescription.SetResultFilterClause(GetResultType(fromClause, queryModel), ResultFilterType.Sequence, -1);

                RunCustomModifiers(queryModel);
            }

            var sourceAsSubQuery = fromClause.FromExpression as SubQueryExpression;
            if (sourceAsSubQuery != null)
            {
                var subQueryModel = sourceAsSubQuery.QueryModel;
                RunCustomModifiers(subQueryModel);
            }

            // If the Where clause is null, we have a request for "get all", but the VisitWhereClause method never runs (thanks Remotion!)
            // so let's fake it here
            if (!queryModel.BodyClauses.Any())
            {
                queryModel.BodyClauses.Add(new WhereClause(Expression.Equal(IgnoreExpression, IgnoreExpression)));
            }

            base.VisitMainFromClause(fromClause, queryModel);
        }

        private void RunCustomModifiers(QueryModel subQueryModel)
        {
            var modifiers = subQueryModel.ResultOperators.OfType<AbstractExtensionResultOperator>();
            foreach (var modifier in modifiers)
            {
                modifier.ModifyQueryDescription(_queryDescription);
            }
        }

        private static Type GetResultType(MainFromClause fromClause, Remotion.Linq.QueryModel queryModel)
        {
            var resultTypeOverride = queryModel.ResultTypeOverride;
            if (typeof(IQueryable).IsAssignableFrom(resultTypeOverride))
            {
                var genericArguments = resultTypeOverride.GetGenericArguments();
                if (genericArguments.Any())
                {
                    return genericArguments.FirstOrDefault();
                }
            }
            return resultTypeOverride ?? fromClause.ItemType;
        }

        private const int IgnoreConstant = 42;
        private readonly ConstantExpression IgnoreExpression = Expression.Constant(IgnoreConstant);

        /// <summary>
        /// Visits the result operator.
        /// </summary>
        /// <param name="resultOperator">The result operator.</param>
        /// <param name="queryModel">The query model.</param>
        /// <param name="index">The index.</param>
        /// <remarks></remarks>
        public override void VisitResultOperator(ResultOperatorBase resultOperator, Remotion.Linq.QueryModel queryModel, int index)
        {
            if (typeof(CountResultOperator).IsAssignableFrom(resultOperator.GetType()))
            {
                _queryDescription.SetResultFilterClause(queryModel.MainFromClause.ItemType, ResultFilterType.Count, 0);
            }

            if (typeof(SingleResultOperator).IsAssignableFrom(resultOperator.GetType()))
            {
                var op = resultOperator as SingleResultOperator;
                var filter = op.ReturnDefaultWhenEmpty ? ResultFilterType.SingleOrDefault : ResultFilterType.Single;
                _queryDescription.SetResultFilterClause(queryModel.MainFromClause.ItemType, filter, 0);
            }

            if (typeof(TakeResultOperator).IsAssignableFrom(resultOperator.GetType()))
            {
                var firstResultOperator = resultOperator as TakeResultOperator;
                var countExpression = firstResultOperator.Count as ConstantExpression;
                var count = Convert.ToInt32(countExpression.Value);
                _queryDescription.SetResultFilterClause(queryModel.MainFromClause.ItemType, ResultFilterType.Take, count);
            }

            var skipResultOp = resultOperator as SkipResultOperator;
            if (skipResultOp != null)
            {
                var countExpression = skipResultOp.Count as ConstantExpression;
                var count = Convert.ToInt32(countExpression.Value);
                _queryDescription.SetResultFilterClause(queryModel.MainFromClause.ItemType, ResultFilterType.Skip, count);
            }

            if (resultOperator is AnyResultOperator)
            {
                _queryDescription.SetResultFilterClause(queryModel.MainFromClause.ItemType, ResultFilterType.Any, 0);
            }

            var allResultOp = resultOperator as AllResultOperator;
            if (allResultOp != null)
            {
                var criteriaExpression = this.GetCriteriaExpression(queryModel, allResultOp.Predicate);
                _queryDescription.SetCriteria(criteriaExpression);
                _queryDescription.SetResultFilterClause(queryModel.MainFromClause.ItemType, ResultFilterType.All, 0);
            }

            var firstResultOp = resultOperator as FirstResultOperator;
            if (firstResultOp != null)
            {
                this._queryDescription.SetResultFilterClause(
                    queryModel.MainFromClause.ItemType,
                    firstResultOp.ReturnDefaultWhenEmpty ? ResultFilterType.FirstOrDefault : ResultFilterType.First,
                    0);
            }

            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        /// <summary>
        /// Visits the select clause.
        /// </summary>
        /// <param name="selectClause">The select clause.</param>
        /// <param name="queryModel">The query model.</param>
        /// <remarks></remarks>
        public override void VisitSelectClause(SelectClause selectClause, Remotion.Linq.QueryModel queryModel)
        {
            // Assess for stuff like Count, Top etc.
           
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            int priority = _queryDescription.SortClauses.Count() - 1;
            foreach (var ordering in orderByClause.Orderings)
            {
                priority++;
                var expr = ordering.Expression;
                var dir = ordering.OrderingDirection;
                var crit = GetFieldSelector(queryModel, expr);
                var sortDirection = (dir == OrderingDirection.Asc) ? SortDirection.Ascending : SortDirection.Descending;
                var clause = new SortClause(crit, sortDirection, priority);
                _queryDescription.AddSortClause(clause);
            }
        }

        /// <summary>
        /// Visits the where clause.
        /// </summary>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="queryModel">The query model.</param>
        /// <param name="index">The index.</param>
        /// <remarks></remarks>
        public override void VisitWhereClause(WhereClause whereClause, Remotion.Linq.QueryModel queryModel, int index)
        {
            var wherePredicate = whereClause.Predicate;

            var criteriaExpression = this.GetCriteriaExpression(queryModel, wherePredicate);

            _queryDescription.SetCriteria(criteriaExpression);
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        private FieldSelectorExpression GetFieldSelector(QueryModel queryModel, Expression predicate)
        {
            return FieldPredicateExpressionRewriter.GetFieldSelector(predicate, _treeVisitor.StructureBinder);
        }

        private Expression GetCriteriaExpression(QueryModel queryModel, Expression wherePredicate)
        {
            var visitExpression = true;

            // Check if this is a fake Where clause to fool Remotion
            var casted = wherePredicate as BinaryExpression;
            if (casted != null)
            {
                var left = casted.Left as ConstantExpression;
                var right = casted.Right as ConstantExpression;
                if (left != null && right != null && left.Equals(this.IgnoreExpression) && right.Equals(this.IgnoreExpression))
                {
                    visitExpression = false;
                }
            }

            Expression criteriaExpression = null;

            if (visitExpression)
            {
                criteriaExpression = this._treeVisitor.VisitExpression(wherePredicate);
            }

            // If the item type of the FromClause has a DefaultSchemaForQueryingAttribute, add that automatically
            // to the Where expression
            var itemType = queryModel.MainFromClause.ItemType;
            if (itemType != null)
            {
                var attribs = itemType.GetCustomAttributes<DefaultSchemaForQueryingAttribute>(true).ToArray();
                if (attribs.Any())
                {
                    foreach (var schemaForQueryingAttribute in attribs)
                    {
                        var schemaExpression = new SchemaPredicateExpression(
                            new SchemaSelectorExpression("Alias"),
                            new SchemaValueExpression(ValuePredicateType.Equal, schemaForQueryingAttribute.SchemaAlias));

                        if (criteriaExpression == null)
                        {
                            criteriaExpression = schemaExpression;
                        }
                        else
                        {
                            criteriaExpression = Expression.And(criteriaExpression, schemaExpression);
                        }
                    }
                }
            }
            return criteriaExpression;
        }
    }
}
