using System.Linq.Expressions;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Utilities;

namespace Umbraco.Framework.Expressions.Remotion
{
    public class ModifiedPartialEvaluatingExpressionTreeProcessor : IExpressionTreeProcessor
    {
        public Expression Process(Expression expressionTree)
        {
            ArgumentUtility.CheckNotNull<Expression>("expressionTree", expressionTree);
            return CustomPartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees(expressionTree);
        }
    }
}