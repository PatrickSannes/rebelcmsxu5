using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Utilities;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Expressions.Remotion;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Linq
{
    using Umbraco.Framework.Linq;

    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    [TestFixture]
    public abstract class AbstractExpressionConversionFixture<TModel>
    {
        [TestFixtureSetUp]
        public static void TestSetup()
        {
            DataHelper.SetupLog4NetForTests();
        }

        public void Run(QueryTestDescription<TModel> onItem)
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TModel>(nullQueryableDataSource);
            var dynamicQuery = onItem.OriginalExpression as Expression<Func<TModel, bool>>;

            dynamic test = new ExpandoObject();
            test.colour = "orange";

            dynamic test2 = new BendyObject();
            test2.colour = "orange";

            var query = context.Where(dynamicQuery ?? onItem.OriginalQuery);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor<TModel>.Check(onItem, generatedQuery.Criteria);
        }

        [Category("ReSharper-Ignore")]
        [TestCaseSource("GetAdditionalTests")]
        public void RunExtras(QueryTestDescription<TModel> item)
        {
            Run(item);
        }

        public abstract IEnumerable<QueryTestDescription<TModel>> GetAdditionalTests();

        internal static IQueryable<T> GenerateContext<T>(IQueryableDataSource nullQueryableDataSource)
        {
            return new Queryable<T>(new Executor(nullQueryableDataSource, null), CustomQueryParser.CreateDefault());
        }

        public abstract QueryTestDescription<TModel> GetQueryById(HiveId value);
        public abstract QueryTestDescription<TModel> GetQueryBySchemaAliasEqual(string alias);
        public abstract IEnumerable<QueryTestDescription<TModel>> GetQueryByTwoStringAttributeEqual(string fieldName, string value, string fieldTwo, string valueTwo);
        public abstract IEnumerable<QueryTestDescription<TModel>> GetQueryByStringAttributeEqual(string fieldName, string value);
        public abstract QueryTestDescription<TModel> GetQueryNumberGreaterThan(string fieldAlias, int number);
        public abstract QueryTestDescription<TModel> GetQueryNumberGreaterThanOrEqual(string fieldAlias, int number);
        public abstract QueryTestDescription<TModel> GetQueryNumberLessThan(string fieldAlias, int number);
        public abstract QueryTestDescription<TModel> GetQueryNumberLessThanOrEqual(string fieldAlias, int number);

        [Test]
        public void ById()
        {
            var hiveId = new HiveId(5);
            var queryTest = GetQueryById(hiveId);
            queryTest.Fields.Add(new FieldPredicateExpression("Id", ValuePredicateType.Equal, hiveId));
            Run(queryTest);
        }

        [Test]
        public void BySchemaAlias()
        {
            var queryTest = GetQueryBySchemaAliasEqual("vehicle");
            queryTest.WithSchema(new SchemaPredicateExpression("Alias", ValuePredicateType.Equal, "vehicle"));
            Run(queryTest);
        }

        [Test]
        public void Equals()
        {
            foreach (var queryTest in GetQueryByStringAttributeEqual("colour", "orange"))
            {
                queryTest.Fields.Add(new FieldPredicateExpression("colour", ValuePredicateType.Equal, "orange"));
                Run(queryTest);
            }
        }

        [Test]
        public void EqualsAndEquals()
        {
            foreach (var queryTest in GetQueryByTwoStringAttributeEqual("colour", "orange", "category", "car"))
            {
                queryTest.Fields.Add(new FieldPredicateExpression("colour", ValuePredicateType.Equal, "orange"));
                queryTest.Fields.Add(new FieldPredicateExpression("category", ValuePredicateType.Equal, "car"));
                Run(queryTest);
            }
        }

        [Test]
        public void GreaterThan()
        {
            Run(
                GetQueryNumberGreaterThan("year", 1).WithField(
                    new FieldPredicateExpression("year", ValuePredicateType.GreaterThan, 1)));
        }

        [Test]
        public void GreaterThanOrEqual()
        {
            Run(
                GetQueryNumberGreaterThanOrEqual("year", 1).WithField(
                    new FieldPredicateExpression("year", ValuePredicateType.GreaterThanOrEqual, 1)));
        }

        [Test]
        public void LessThan()
        {
            Run(
                GetQueryNumberLessThan("year", 1).WithField(
                    new FieldPredicateExpression("year", ValuePredicateType.LessThan, 1)));
        }

        [Test]
        public void LessThanOrEqual()
        {
            Run(
                GetQueryNumberLessThanOrEqual("year", 1).WithField(
                    new FieldPredicateExpression("year", ValuePredicateType.LessThanOrEqual, 1)));
        }

        [Test]
        [Description("This test calls Run for each item in GetAdditionalTests, for runners like R#6 who have terribly buggy support for TestCaseSource")]
        public void RunExtrasLegacy()
        {
            foreach (var queryTest in GetAdditionalTests())
            {
                Run(queryTest);
            }
        }

        public abstract IEnumerable<QueryTestDescription<TModel>> GetQueryByStringAttributeSubFieldEqual(string fieldName, string subFieldName, string value);

        [Test]
        public virtual void SubItemEqual()
        {
            var queryTests = GetQueryByStringAttributeSubFieldEqual("car", "fuel", "diesel");

            foreach (var queryTest in queryTests)
            {
                queryTest.Fields.Add(new FieldPredicateExpression("car", "fuel", ValuePredicateType.Equal, "diesel"));
                Run(queryTest);
            }
        }

        [Test]
        public void ContainsString()
        {
            
            var queryTest = GetQueryStringAttributeContainsString("title", "news");
            queryTest.Fields.Add(new FieldPredicateExpression("title", ValuePredicateType.Contains, "news"));
            Run(queryTest);
        }

        [Test]
        public void StringEndsWith()
        {
            var queryTest = GetQueryStringAttributeEndsWith("name", "Surname");
            queryTest.Fields.Add(new FieldPredicateExpression("name", ValuePredicateType.EndsWith, "Surname"));
            Run(queryTest);
        }

        [Test]
        public void StringStartsWith()
        {
            var queryTest = GetQueryStringAttributeStartsWith("name", "Forename");
            queryTest.Fields.Add(new FieldPredicateExpression("name", ValuePredicateType.StartsWith, "Forename"));
            Run(queryTest);
        }

        public abstract QueryTestDescription<TModel> GetQueryStringAttributeEndsWith(string fieldName, string fieldValue);
        public abstract QueryTestDescription<TModel> GetQueryStringAttributeStartsWith(string fieldName, string fieldValue);
        public abstract QueryTestDescription<TModel> GetQueryStringAttributeContainsString(string fieldName, string value);
    }
}