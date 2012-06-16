namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    using System;
    using System.Linq.Expressions;

    public class SchemaSelectorExpression : AbstractExpressionExtension<string>
    {
        /// <summary>
        /// Constructs a new instance of <see cref="SchemaSelectorExpression"/>.
        /// </summary>
        public SchemaSelectorExpression(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <remarks></remarks>
        public string Name { get; private set; }

        public override string ToString()
        {
            return "Schema: " + Name;
        }
    }
}