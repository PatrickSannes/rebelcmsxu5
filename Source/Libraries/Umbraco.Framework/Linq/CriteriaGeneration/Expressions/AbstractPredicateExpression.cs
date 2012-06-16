namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    using System.Linq.Expressions;

    /// <summary>
    /// An expression which represents a predicate on a schema of fields, comprising a <typeparam name="TSelector" /> to select the schema,
    /// and a <typeparam name="TValueSelector"/> to indicate the value and operator.
    /// </summary>
    /// <remarks></remarks>
    public class AbstractPredicateExpression<TSelector, TValueSelector> : AbstractExpressionExtension<bool>
        where TSelector : Expression
        where TValueSelector : Expression
    {
        /// <summary>
        /// Gets or sets the selector expression (for example, the field or the schema of an entity).
        /// </summary>
        /// <value>The schema selector.</value>
        /// <remarks></remarks>
        public TSelector SelectorExpression { get; set; }

        /// <summary>
        /// Gets or sets the value expression (for example, the value of the field or schema represented by the <see cref="SelectorExpression"/>.
        /// </summary>
        /// <value>The schema value.</value>
        /// <remarks></remarks>
        public TValueSelector ValueExpression { get; set; }

        public override string ToString()
        {
            string selector = SelectorExpression != null ? SelectorExpression.ToString() : string.Empty;
            string value = ValueExpression != null ? ValueExpression.ToString() : string.Empty;
            return selector + " " + value;
        }
    }
}