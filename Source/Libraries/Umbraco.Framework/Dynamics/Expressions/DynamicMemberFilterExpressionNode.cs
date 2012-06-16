namespace Umbraco.Framework.Dynamics.Expressions
{
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Remotion.Linq.Parsing.Structure.IntermediateModel;

    public class DynamicMemberFilterExpressionNode : ResultOperatorExpressionNodeBase
    {
        public Expression MemberExpression { get; protected set; }

        public DynamicMemberFilterExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression idList)
            : base(parseInfo, (LambdaExpression)null, (LambdaExpression)null)
        {
            MemberExpression = idList;
        }

        #region Overrides of MethodCallExpressionNodeBase

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        #endregion

        #region Overrides of ResultOperatorExpressionNodeBase

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new DynamicMemberFilterResultOperator(MemberExpression);
        }

        #endregion
    }
}