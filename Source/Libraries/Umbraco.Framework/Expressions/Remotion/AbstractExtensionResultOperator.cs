namespace Umbraco.Framework.Expressions.Remotion
{
    using System;
    using System.Linq.Expressions;
    using Umbraco.Framework.Linq.QueryModel;
    using global::Remotion.Linq.Clauses;
    using global::Remotion.Linq.Clauses.ExpressionTreeVisitors;
    using global::Remotion.Linq.Clauses.ResultOperators;
    using global::Remotion.Linq.Clauses.StreamedData;

    public abstract class AbstractExtensionResultOperator : SequenceTypePreservingResultOperatorBase
    {
        protected AbstractExtensionResultOperator(Expression parameter)
        {
            Parameter = parameter;
        }

        public abstract void ModifyQueryDescription(QueryDescription queryDescription);

        public Expression Parameter { get; private set; }

        public abstract string Name { get; }

        public override string ToString()
        {
            return Name + " (" + FormattingExpressionTreeVisitor.Format(Parameter) + ")";
        }

        public override abstract ResultOperatorBase Clone(CloneContext cloneContext);

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            Parameter = transformation(Parameter);
        }

        public override StreamedSequence ExecuteInMemory<T>(StreamedSequence input)
        {
            return input; // sequence is not changed by this operator
        }
    }
}