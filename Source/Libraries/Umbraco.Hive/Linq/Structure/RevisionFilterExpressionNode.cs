namespace Umbraco.Hive.Linq.Structure
{
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Remotion.Linq.Parsing.Structure.IntermediateModel;

    public class RevisionFilterExpressionNode : ResultOperatorExpressionNodeBase
    {
        public Expression RevisionStatusType { get; protected set; }

        public RevisionFilterExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression revisionStatusType)
            : base(parseInfo, (LambdaExpression)null, (LambdaExpression)null)
        {
            RevisionStatusType = revisionStatusType;
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
            return new RevisionFilterResultOperator(RevisionStatusType);
        }

        #endregion
    }

    public class IdFilterExpressionNode : ResultOperatorExpressionNodeBase
    {
        public Expression IdList { get; protected set; }

        public IdFilterExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression idList)
            : base(parseInfo, (LambdaExpression)null, (LambdaExpression)null)
        {
            IdList = idList;
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
            return new IdFilterResultOperator(IdList);
        }

        #endregion
    }
}