namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    /// <summary>
    /// An expression which represents a value for consideration in an operation, given the <see cref="AbstractValueExpression.ClauseType"/> and the value itself.
    /// </summary>
    /// <remarks></remarks>
    public class SchemaValueExpression : AbstractValueExpression // Cannot override ConstantExpression because there is no way to set the value
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValueExpression"/> class.
        /// </summary>
        /// <param name="clauseType">Type of the clause.</param>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        public SchemaValueExpression(ValuePredicateType clauseType, object value)
        {
            ClauseType = clauseType;
            Value = value;
        }
    }
}