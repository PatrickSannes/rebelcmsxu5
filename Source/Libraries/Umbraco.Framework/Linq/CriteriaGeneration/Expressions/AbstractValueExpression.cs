namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// An expression which represents a value for consideration in an operation, given the <see cref="AbstractValueExpression.ClauseType"/> and the value itself.
    /// </summary>
    /// <remarks></remarks>
    public abstract class AbstractValueExpression : Expression
    {
        /// <summary>
        /// Gets the type of the clause.
        /// </summary>
        /// <value>The type of the clause.</value>
        /// <remarks></remarks>
        public ValuePredicateType ClauseType { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks></remarks>
        public object Value { get; set; }

        /// <summary>
        /// Gets the node type of this <see cref="T:System.Linq.Expressions.Expression"/>.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Linq.Expressions.ExpressionType"/> values.</returns>
        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression"/> represents.
        /// </summary>
        /// <returns>The <see cref="T:System.Type"/> that represents the static type of the expression.</returns>
        /// <remarks></remarks>
        public override Type Type
        {
            get
            {
                return Value == null ? typeof(object) : Value.GetType();
            }
        }

        public override string ToString()
        {
            return ClauseType + " " + (Value ?? "(null)");
        }
    }
}