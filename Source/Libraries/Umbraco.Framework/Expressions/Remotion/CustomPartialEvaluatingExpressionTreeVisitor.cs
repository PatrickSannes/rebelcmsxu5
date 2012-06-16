using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation;
using Remotion.Linq.Utilities;
using Umbraco.Framework.Dynamics.Expressions;

namespace Umbraco.Framework.Expressions.Remotion
{
    /// <summary>
    ///   This is based on <see cref = "PartialEvaluatingExpressionTreeVisitor" /> but with a modification to avoid
    ///   partially evaluating subtrees if the subtree is a binary expression between <see
    ///    cref = "DynamicMemberMetadata.GetMember" /> and a value.
    ///   Normally, <see cref = "PartialEvaluatingExpressionTreeVisitor" /> would eagerly compile certain types of subtree to make them "easier" to parse
    ///   but this would mean we'd lose the metadata contained in the <see cref = "DynamicMemberMetadata.GetMember" /> call and would just end up with the
    ///   constant <code>False</code> evaluation return value when later visiting the <see cref = "QueryModel" />.
    /// </summary>
    /// <remarks>Unfortunately <see cref = "PartialEvaluatingExpressionTreeVisitor" /> has a private constructor as of Relinq version 1.13.122.1 so some code is duplicated rather than inherited</remarks>
    public class CustomPartialEvaluatingExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private readonly PartialEvaluationInfo _partialEvaluationInfo;

        private CustomPartialEvaluatingExpressionTreeVisitor(
            Expression treeRoot, PartialEvaluationInfo partialEvaluationInfo)
        {
            ArgumentUtility.CheckNotNull("treeRoot", treeRoot);
            ArgumentUtility.CheckNotNull("partialEvaluationInfo", partialEvaluationInfo);
            this._partialEvaluationInfo = partialEvaluationInfo;
        }

        public static Expression EvaluateIndependentSubtrees(Expression expressionTree)
        {
            ArgumentUtility.CheckNotNull("expressionTree", expressionTree);
            var partialEvaluationInfo = EvaluatableTreeFindingExpressionTreeVisitor.Analyze(expressionTree);
            return
                new CustomPartialEvaluatingExpressionTreeVisitor(expressionTree, partialEvaluationInfo).VisitExpression(
                    expressionTree);
        }

        protected override Expression VisitUnknownNonExtensionExpression(Expression expression)
        {
            return expression;
        }

        public override Expression VisitExpression(Expression expression)
        {
            if (expression == null)
                return null;
            if (expression.NodeType == ExpressionType.Lambda ||
                !this._partialEvaluationInfo.IsEvaluatableExpression(expression))
                return base.VisitExpression(expression);
            Expression expressionTree = this.EvaluateSubtree(expression);
            if (expressionTree != expression)
                return CustomPartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees(expressionTree);
            else
                return expressionTree;
        }

        class DynamicMemberMetadataFindingVisitor : ExpressionTreeVisitor
        {
            public bool ContainsDynamic { get; set; }

            protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
            {
                if (expression.Method == DynamicMemberMetadata.GetMemberMethod) ContainsDynamic = true;
                return base.VisitMethodCallExpression(expression);
            }
        }

        protected Expression EvaluateSubtree(Expression subtree)
        {
            ArgumentUtility.CheckNotNull("subtree", subtree);

            // Start custom digression from Relinq sealed base
            var binary = subtree as BinaryExpression;
            var checker = new DynamicMemberMetadataFindingVisitor();
            checker.VisitExpression(subtree);
            if (checker.ContainsDynamic) return subtree;
            //if (binary != null)
            //{
            //    var method = binary.Left as MethodCallExpression;
            //    if (method != null)
            //    {
            //        if (method.Method == DynamicMemberMetadata.GetMemberMethod)
            //        {
            //            return subtree;
            //        }
            //    }
            //    var unary = binary.Left as UnaryExpression;
            //    if (unary != null)
            //    {
            //        var innerMethod = unary.Operand as MethodCallExpression;
            //        if (innerMethod != null)
            //        {
            //            if (innerMethod.Method == DynamicMemberMetadata.GetMemberMethod)
            //            {
            //                return subtree;
            //            }
            //        }
            //    }
            //}
            // End digression

            if (subtree.NodeType != ExpressionType.Constant)
                return
                    Expression.Constant(
                        Expression.Lambda<Func<object>>(
                            Expression.Convert(subtree, typeof (object)), new ParameterExpression[0]).Compile()(),
                        subtree.Type);
            var constantExpression = (ConstantExpression) subtree;
            var queryable = constantExpression.Value as IQueryable;
            if (queryable != null && queryable.Expression != constantExpression)
                return queryable.Expression;
            else
                return constantExpression;
        }
    }
}