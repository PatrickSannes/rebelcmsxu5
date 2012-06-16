namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// An expression which represents the selection of a field by a <see cref="string"/> alias. Designed to nominate a key in a key-value system.
    /// </summary>
    /// <remarks>
    /// This expression can also represent querying a field with an optional value key. This is useful for querying against 
    /// fields that store multiple values and a query is required to query against a particular value key.
    /// </remarks>
    public class FieldSelectorExpression : AbstractExpressionExtension<string>
    {
        public FieldSelectorExpression()
            : this(string.Empty, string.Empty)
        {
        }

        public static readonly FieldSelectorExpression Empty = new FieldSelectorExpression();

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSelectorExpression"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks></remarks>
        public FieldSelectorExpression(string name)
        {
            FieldName = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSelectorExpression"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="valueKey">The value key.</param>
        public FieldSelectorExpression(string name, string valueKey)
            :this(name)
        {
            ValueKey = valueKey;
        }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>The name of the field.</value>
        /// <remarks></remarks>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the value key.
        /// </summary>
        /// <value>
        /// The value key.
        /// </value>
        public string ValueKey { get; set; }

        public override string ToString()
        {
            return "Field: " + FieldName + " (SubField: " + ValueKey + ") ";
        }


        public override bool CanReduce
        {
            get { return true; }
        }

        public override Expression Reduce()
        {
            return new FieldSelectorExpression(FieldName, ValueKey);
        }
    }
}
