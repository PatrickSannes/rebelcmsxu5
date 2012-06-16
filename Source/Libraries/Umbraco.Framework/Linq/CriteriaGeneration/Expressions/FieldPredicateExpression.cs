namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    using System.Linq.Expressions;

    /// <summary>
    /// An expression which represents a predicate on a field, comprising a <see cref="FieldSelectorExpression"/> to select the field,
    /// and a <see cref="FieldValueExpression"/> to indicate the value and operator.
    /// </summary>
    /// <remarks></remarks>
    public class FieldPredicateExpression : AbstractPredicateExpression<FieldSelectorExpression, FieldValueExpression>
    {
        public FieldPredicateExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldPredicateExpression"/> class.
        /// </summary>
        /// <param name="fieldSelector">The field selector.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <remarks></remarks>
        public FieldPredicateExpression(FieldSelectorExpression fieldSelector, FieldValueExpression fieldValue)
        {
            SelectorExpression = fieldSelector;
            ValueExpression = fieldValue;
        }

        public FieldPredicateExpression(string fieldName, ValuePredicateType predicateType, object value)
            : this(new FieldSelectorExpression(fieldName), new FieldValueExpression(predicateType, value))
        {}

        public FieldPredicateExpression(string fieldName, string subFieldName, ValuePredicateType predicateType, object value)
            : this(new FieldSelectorExpression(fieldName, subFieldName), new FieldValueExpression(predicateType, value))
        { }

        public override bool CanReduce
        {
            get { return true; }
        }

        public override Expression Reduce()
        {
            return new FieldPredicateExpression(SelectorExpression, ValueExpression);
        }
    }
}