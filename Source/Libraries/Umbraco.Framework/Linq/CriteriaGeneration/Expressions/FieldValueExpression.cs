namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    using System.Linq.Expressions;

    /// <summary>
    /// An expression which represents a value for consideration in an operation, given the <see cref="AbstractValueExpression.ClauseType"/> and the value itself.
    /// </summary>
    /// <remarks></remarks>
    public class FieldValueExpression : AbstractValueExpression // Cannot override ConstantExpression because there is no way to set the value
    {
        public FieldValueExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValueExpression"/> class.
        /// </summary>
        /// <param name="clauseType">Type of the clause.</param>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        public FieldValueExpression(ValuePredicateType clauseType, object value)
        {
            ClauseType = clauseType;
            Value = value;
        }

        public override bool CanReduce
        {
            get { return true; }
        }

        public override Expression Reduce()
        {
            return new FieldValueExpression(ClauseType, Value);
        }
    }
}