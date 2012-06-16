namespace Umbraco.Framework.Linq.CriteriaGeneration.ExpressionVisitors
{
    using System;

    using System.Linq.Expressions;

    using Remotion.Linq.Clauses.Expressions;

    using Remotion.Linq.Parsing;

    using Umbraco.Framework.Dynamics.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;

    public abstract class AbstractExpressionTreeVisitor : ThrowingExpressionTreeVisitor
    {
        public AbstractQueryStructureBinder StructureBinder { get; protected set; }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            string itemText = ExpressionHelper.FormatUnhandledItem(unhandledItem as Expression);
            var message = string.Format("The expression '{0}' (type: {1}) is not supported by this LINQ provider.", itemText, typeof(T));
            return new NotSupportedException(message);
        }

        /// <summary>
        /// Visits a member expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            return expression;
        }

        protected override Expression VisitUnaryExpression(UnaryExpression expression)
        {
            // The only type of Unary that we support is "Not", i.e. reversing the inner expression
            if (expression.NodeType == ExpressionType.Not)
            {
                var output = VisitExpression(expression.Operand);
                if (typeof(FieldPredicateExpression).IsAssignableFrom(output.GetType()))
                {
                    return FieldPredicateExpressionRewriter.NegateFieldPredicate(output as FieldPredicateExpression);
                }
            }

            if (expression.NodeType == ExpressionType.Convert && 
                (expression.Type == typeof(object) || IsDynamicMemberGetter(expression.Operand)))
            {
                return expression.Operand;
            }


            return base.VisitUnaryExpression(expression);
        }

        private static bool IsDynamicMemberGetter(Expression expression)
        {
            var methodCall = expression as MethodCallExpression;
            if (methodCall != null)
            {
                if (methodCall.Method == DynamicMemberMetadata.GetMemberMethod)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Visits a method call expression, returning a rewritten expression if supported.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Type == typeof(bool))
            {
                // Make booleans like Equal or EndsWith be a binary
                return VisitExpression(Expression.MakeBinary(ExpressionType.Equal, expression, Expression.Constant(true)));
            }

            if (expression.Method == ExpressionHelper.GetMethodInfo<string>(x => x.EndsWith(default(string))))
            {
                // This is all hacked together, better to make a method which can visit a Member to get its name or a FieldSelectorExpression
                var argument = expression.Arguments[0] as ConstantExpression;
                var fieldName = ((MemberExpression) expression.Object).Member.Name;
                return new FieldPredicateExpression(
                    new FieldSelectorExpression(fieldName), new FieldValueExpression(
                                                                ValuePredicateType.EndsWith,
                                                                (argument).Value));
            }

            if (expression.Method.DeclaringType == typeof(DynamicMemberMetadata))
            {
                // This is the fake intermediary model to help represent dynamic expressions in a form compatible with Relinq
                // which doesn't support DynamicExpression in .NET 4
                var getMember = DynamicMemberMetadata.GetMemberMethod;
                if (expression.Method == getMember)
                {
                    // The first parameter is the name of the field
                    var firstParam = expression.Arguments[0] as ConstantExpression;
                    var fieldName = firstParam.Value.ToString();
                    return new FieldSelectorExpression(fieldName);
                }
            }

            // Not handled so throw
            return base.VisitMethodCallExpression(expression);
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            return base.VisitSubQueryExpression(expression);
        }

        /// <summary>
        /// Visits a binary expression, returning a rewritten expression if supported.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {
            var left = expression.Left;
            var right = expression.Right;

            //if (UmbracoExpressionRewriter.IsConvertibleBinary(expression))
            //    return UmbracoExpressionRewriter.ConvertToFieldEvaluationExpression(expression);
            if (FieldPredicateExpressionRewriter.IsConvertibleBinary(expression, StructureBinder))
                return FieldPredicateExpressionRewriter.ConvertToFieldPredicate(expression, StructureBinder);

            //else make a new binary with the results of visiting deeper in the tree
            var visitedLeft = VisitExpression(left);
            var visitedRight = VisitExpression(right);
            return Expression.MakeBinary(expression.NodeType, visitedLeft, visitedRight, expression.IsLiftedToNull, expression.Method);
        }

        protected override Expression VisitParameterExpression(ParameterExpression expression)
        {
            return base.VisitParameterExpression(expression);
        }

        protected override Expression VisitQuerySourceReferenceExpression(QuerySourceReferenceExpression expression)
        {
            return base.VisitQuerySourceReferenceExpression(expression);
        }

        protected override Expression VisitTypeBinaryExpression(TypeBinaryExpression expression)
        {
            return base.VisitTypeBinaryExpression(expression);
        }

        protected override Expression VisitConditionalExpression(ConditionalExpression expression)
        {
            return base.VisitConditionalExpression(expression);
        }

        public override Expression VisitExpression(Expression expression)
        {
            return base.VisitExpression(expression);
        }

        protected override ElementInit VisitElementInit(ElementInit elementInit)
        {
            return base.VisitElementInit(elementInit);
        }

        protected override Expression VisitExtensionExpression(ExtensionExpression expression)
        {
            return base.VisitExtensionExpression(expression);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding memberBinding)
        {
            return base.VisitMemberBinding(memberBinding);
        }

        /// <summary>
        /// Visits a constant expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            return expression;
        }
    }
}