namespace Umbraco.Tests.CoreAndFramework.Linq
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    public static class QueryTestDescriptionExtensions
    {
        public static QueryTestDescription<TModel> WithField<TModel>(this QueryTestDescription<TModel> queryTestDescription, params FieldPredicateExpression[] expectedFieldExpressions)
        {
            foreach (var fieldPredicateExpression in expectedFieldExpressions)
            {
                queryTestDescription.Fields.Add(fieldPredicateExpression);
            }
            return queryTestDescription;
        }

        public static QueryTestDescription<TModel> WithSchema<TModel>(this QueryTestDescription<TModel> queryTestDescription, params SchemaPredicateExpression[] expectedSchemaExpressions)
        {
            foreach (var schemaExpression in expectedSchemaExpressions)
            {
                queryTestDescription.Schemas.Add(schemaExpression);
            }
            return queryTestDescription;
        }
    }
}