namespace Umbraco.Framework.Linq.CriteriaGeneration.ExpressionVisitors
{
    using System.Linq.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;

    /// <summary>
    /// A default implementation of an expression tree visitor designed to take an expression tree and rewrite it to the more simplified
    /// model supported by the Umbraco Framework and its descendent persistence providers.
    /// </summary>
    /// <remarks></remarks>
    public class DefaultExpressionTreeVisitor : AbstractExpressionTreeVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultExpressionTreeVisitor"/> class.
        /// </summary>
        /// <param name="structureBinder">The structure binder.</param>
        /// <remarks></remarks>
        public DefaultExpressionTreeVisitor(AbstractQueryStructureBinder structureBinder)
        {
            StructureBinder = structureBinder;
        }

        /// <summary>
        /// Rewrites the expression tree to a field predicate tree.
        /// </summary>
        /// <param name="expressionToVisit">The expression to visit.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Expression RewriteToFieldPredicateTree(Expression expressionToVisit)
        {
            return RewriteToFieldPredicateTree(expressionToVisit, new DefaultQueryStructureBinder());
        }

        /// <summary>
        /// Rewrites the expression tree to a field predicate tree given an <see cref="AbstractQueryStructureBinder"/> implementation.
        /// </summary>
        /// <param name="expressionToVisit">The expression to visit.</param>
        /// <param name="structureBinder">The structure binder.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Expression RewriteToFieldPredicateTree(Expression expressionToVisit, AbstractQueryStructureBinder structureBinder)
        {
            var visitor = new DefaultExpressionTreeVisitor(structureBinder);
            return visitor.VisitExpression(expressionToVisit);
        }
    }
}
