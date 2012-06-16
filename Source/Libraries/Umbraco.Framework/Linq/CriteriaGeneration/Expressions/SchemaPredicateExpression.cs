namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    /// <summary>
     /// An expression which represents a predicate on a schema of fields, comprising a <see cref="SchemaSelectorExpression"/> to select the schema,
     /// and a <see cref="SchemaValueExpression"/> to indicate the value and operator.
     /// </summary>
     /// <remarks></remarks>
     public class SchemaPredicateExpression : AbstractPredicateExpression<SchemaSelectorExpression, SchemaValueExpression>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Linq.Expressions.Expression"/> class.
        /// </summary>
        public SchemaPredicateExpression(SchemaSelectorExpression schemaSelector, SchemaValueExpression schemaValue)
        {
            SelectorExpression = schemaSelector;
            ValueExpression = schemaValue;
        }

         public SchemaPredicateExpression(string name, ValuePredicateType predicateType, object value)
             : this(new SchemaSelectorExpression(name), new SchemaValueExpression(predicateType, value))
         {}
    }
}
