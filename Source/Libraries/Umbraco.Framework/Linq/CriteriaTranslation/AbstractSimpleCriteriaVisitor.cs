namespace Umbraco.Framework.Linq.CriteriaTranslation
{
    using System;

    using System.Linq;

    using System.Linq.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    public abstract class AbstractCriteriaVisitor<T>
    {
        public virtual T Visit(Expression criteria)
        {
            if (criteria == null)
            {
                return VisitNoCriteriaPresent();
            }

            if (typeof(BinaryExpression).IsAssignableFrom(criteria.GetType()))
            {
                return VisitBinary(criteria as BinaryExpression);
            }
            if (typeof(FieldPredicateExpression).IsAssignableFrom(criteria.GetType()))
            {
                return VisitFieldPredicate(criteria as FieldPredicateExpression);
            }
            if (typeof(SchemaPredicateExpression).IsAssignableFrom(criteria.GetType()))
            {
                return VisitSchemaPredicate(criteria as SchemaPredicateExpression);
            }

            return default(T);
        }

        public abstract T VisitNoCriteriaPresent();
        public abstract T VisitSchemaPredicate(SchemaPredicateExpression node);
        public abstract T VisitFieldPredicate(FieldPredicateExpression node);
        public abstract T VisitBinary(BinaryExpression node);
    }

    public abstract class AbstractSimpleCriteriaVisitor<T> : AbstractCriteriaVisitor<Expression<Func<T, bool>>>
    {
        public override Expression<Func<T, bool>> VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            var invokedRight = Expression.Invoke(right, left.Parameters.Cast<Expression>());

            Expression<Func<T, bool>> lambda;
            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    lambda = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, invokedRight), left.Parameters);
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                default:
                    lambda = Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, invokedRight), left.Parameters);
                    break;
            }

            return lambda;
        }
    }
}