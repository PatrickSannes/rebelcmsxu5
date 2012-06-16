using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbraco.Tests.CoreAndFramework.Linq
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    public class QueryTestDescription<TModel>
    {
        protected QueryTestDescription()
        {
            Fields = new List<FieldPredicateExpression>();
            Schemas = new List<SchemaPredicateExpression>();
        }

        public QueryTestDescription(LambdaExpression originalExpression) : this()
        {
            OriginalExpression = originalExpression;
        }

        public QueryTestDescription(Expression<Func<TModel, bool>> originalQuery) : this()
        {
            OriginalQuery = originalQuery;
        }

        public QueryTestDescription(Expression<Func<TModel, bool>> originalQuery, params FieldPredicateExpression[] expectedFieldExpressions)
            : this(originalQuery)
        {
            Fields = expectedFieldExpressions;
        }

        public QueryTestDescription(Expression<Func<TModel, bool>> originalQuery, params SchemaPredicateExpression[] expectedSchemaExpressions)
            : this(originalQuery)
        {
            Schemas = expectedSchemaExpressions;
        }

        public LambdaExpression OriginalExpression { get; set; }

        public Expression<Func<TModel, bool>> OriginalQuery { get; set; }

        public IList<FieldPredicateExpression> Fields { get; private set; }

        public IList<SchemaPredicateExpression> Schemas { get; private set; }

        public override string ToString()
        {
            var fieldBuilder = new StringBuilder();
            foreach (var fieldPredicateExpression in Fields)
            {
                fieldBuilder.Append(fieldPredicateExpression.ToString());
                fieldBuilder.Append(", ");
            }
            foreach (var schemaPredicateExpression in Schemas)
            {
                fieldBuilder.Append(schemaPredicateExpression.ToString());
                fieldBuilder.Append(", ");
            }
            return fieldBuilder.ToString();
        }
    }
}