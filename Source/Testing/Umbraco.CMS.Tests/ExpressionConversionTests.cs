using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.Cms
{
    using Umbraco.Framework.Expressions.Remotion;
    using Umbraco.Framework.Linq;

    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaTranslation;

    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;
    using Umbraco.Hive;

    [TestFixture]
    public class ExpressionConversionTests
    {
        [TestFixtureSetUp]
        public static void TestSetup()
        {
            DataHelper.SetupLog4NetForTests();
        }

        [Test]
        public void WhenRevisionStatusIsGiven_FromClauseRevisionStatus_IsCorrect()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.OfRevisionType("draft");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            Assert.That(generatedQuery.From.RevisionStatus, Is.EqualTo("draft"));
        }

        [Test]
        public void WhenOrderIsSpecified_SortClauseIsPopulated()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.OrderBy(x => x.Attribute<string>("title"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            Assert.That(generatedQuery.SortClauses.Any());
            var firstSortClause = generatedQuery.SortClauses.FirstOrDefault();
            Assert.That(firstSortClause.Direction, Is.EqualTo(SortDirection.Ascending));
            Assert.That(firstSortClause.FieldSelector.FieldName, Is.EqualTo("title"));
        }

        [Test]
        public void WhenOrderByDescendingIsSpecified_SortClauseIsPopulated()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.OrderByDescending(x => x.Attribute<string>("title"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            Assert.That(generatedQuery.SortClauses.Any());
            var firstSortClause = generatedQuery.SortClauses.FirstOrDefault();
            Assert.That(firstSortClause.Direction, Is.EqualTo(SortDirection.Descending));
            Assert.That(firstSortClause.FieldSelector.FieldName, Is.EqualTo("title"));
        }

        [Test]
        public void WhenOrderThenByOrderIsSpecified_SortClausesArePopulated()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.OrderBy(x => x.Attribute<string>("title")).ThenBy(x => x.Attribute<string>("subtitle"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            Assert.That(generatedQuery.SortClauses.Count(), Is.EqualTo(2));
            var firstSortClause = generatedQuery.SortClauses.FirstOrDefault();
            Assert.That(firstSortClause.Direction, Is.EqualTo(SortDirection.Ascending));
            Assert.That(firstSortClause.FieldSelector.FieldName, Is.EqualTo("title"));
            Assert.That(firstSortClause.Priority, Is.EqualTo(0));

            var secondSortClause = generatedQuery.SortClauses.Skip(1).FirstOrDefault();
            Assert.That(secondSortClause.Direction, Is.EqualTo(SortDirection.Ascending));
            Assert.That(secondSortClause.FieldSelector.FieldName, Is.EqualTo("subtitle"));
            Assert.That(secondSortClause.Priority, Is.EqualTo(1));
        }

        [Test]
        public void WhenOrderThenByOrderDescendingIsSpecified_SortClausesArePopulated()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.OrderBy(x => x.Attribute<string>("title")).ThenByDescending(x => x.Attribute<string>("subtitle"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            Assert.That(generatedQuery.SortClauses.Count(), Is.EqualTo(2));
            var firstSortClause = generatedQuery.SortClauses.FirstOrDefault();
            Assert.That(firstSortClause.Direction, Is.EqualTo(SortDirection.Ascending));
            Assert.That(firstSortClause.FieldSelector.FieldName, Is.EqualTo("title"));
            Assert.That(firstSortClause.Priority, Is.EqualTo(0));

            var secondSortClause = generatedQuery.SortClauses.Skip(1).FirstOrDefault();
            Assert.That(secondSortClause.Direction, Is.EqualTo(SortDirection.Descending));
            Assert.That(secondSortClause.FieldSelector.FieldName, Is.EqualTo("subtitle"));
            Assert.That(secondSortClause.Priority, Is.EqualTo(1));
        }

        [Test]
        public void WhenRevisionStatusIsNotGiven_FromClauseRevisionStatus_IsPublished()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.Where(x => x.Attribute<string>("whatever") == "blah");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            Assert.That(generatedQuery.From.RevisionStatus, Is.EqualTo(FromClause.RevisionStatusNotSpecified));
        }

        #region TypedEntity model

        [Test]
        public void TypedEntityModel_Attribute_EqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.Where(x => x.Attributes["items"].Values["default"] == (object)"blah");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", "blah", ValuePredicateType.Equal);
        }

        [Test]
        public void TypedEntityModel_Attribute_EqualsOperator_ViaExtension()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.Where(x => x.Attribute<string>("items") == "blah");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", "blah", ValuePredicateType.Equal);
        }

        [Test]
        public void TypedEntityModel_Attribute_WithSubFieldKey_EqualsOperator_ViaExtension()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.Where(x => x.Attribute<string>("items", "subItems") == "blah");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", "blah", ValuePredicateType.Equal, "subItems");
        }

        [Test]
        public void TypedEntityModel_Id_EqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.Where(x => x.Id == new HiveId(5));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "Id", new HiveId(5), ValuePredicateType.Equal);
        }

        [Test]
        public void TypedEntityModel_EntitySchema_EqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<TypedEntity>(nullQueryableDataSource);
            var query = context.Where(x => x.EntitySchema.Alias == "NewsItem");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "Alias", "NewsItem", ValuePredicateType.Equal);
        }

        #endregion

        #region NumberField extension method

        [Test]
        public void ContentViewModel_NumberField_EqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.NumberField("items") == 2);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", 2, ValuePredicateType.Equal);
        }

        [Test]
        public void ContentViewModel_NumberField_GreaterThanOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.NumberField("items") > 2);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", 2, ValuePredicateType.GreaterThan);
        }

        [Test]
        public void ContentViewModel_NumberField_GreaterThanOrEqualToOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.NumberField("items") >= 2);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", 2, ValuePredicateType.GreaterThanOrEqual);
        }

        [Test]
        public void ContentViewModel_NumberField_LessThanOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.NumberField("items") < 2);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", 2, ValuePredicateType.LessThan);
        }

        [Test]
        public void ContentViewModel_NumberField_LessThanOrEqualToOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.NumberField("items") <= 2);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", 2, ValuePredicateType.LessThanOrEqual);
        }

        #endregion

        #region StringField extension method

        [Test]
        public void ContentViewModel_StringField_EqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.StringField("bodyText") == "frank");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.Equal);
        }

        [Test]
        public void ContentViewModel_StringField_NotEqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.StringField("bodyText") != "frank");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.NotEqual);
        }

        [Test]
        public void ContentViewModel_StringField_EqualsMethod()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.StringField("bodyText").Equals("frank"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.Equal);
        }

        [Test]
        public void ContentViewModel_StringField_NotEqualsMethod()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.StringField("bodyText").Equals("frank") != true);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.NotEqual);
        }

        [Test]
        public void ContentViewModel_StringField_NotEqualsMethod_ViaUnaryExpression()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => !x.StringField("bodyText").Equals("frank"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.NotEqual);
        }



        [Test]
        public void ContentViewModel_StringField_StartsWith()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.StringField("bodyText").StartsWith("frank"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.StartsWith);
        }

        [Test]
        public void ContentViewModel_StringField_EndsWith()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.StringField("bodyText").EndsWith("frank"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.EndsWith);
        }

        #endregion

        #region Field<T> extension method

        [Test]
        public void ContentViewModel_FieldOfString_EqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.Field<string>("bodyText") == "frank");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.Equal);
        }

        [Test]
        public void ContentViewModel_FieldOfString_EqualsMethod()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.Field<string>("bodyText").Equals("frank"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.Equal);
        }

        [Test]
        public void ContentViewModel_FieldOfString_StartsWith()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.Field<string>("bodyText").StartsWith("frank"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.StartsWith);
        }

        [Test]
        public void ContentViewModel_FieldOfString_EndsWith()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.Field<string>("bodyText").EndsWith("frank"));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.EndsWith);
        }

        #endregion

        #region Field() using casting - should fail

        [Test]
        [ExpectedException(typeof(NotSupportedException), UserMessage = "Expressions which cast are not supported because not all persistence providers might be capable")]
        public void ContentViewModel_FieldCastAsString_Equals()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => (string)x.Field("bodyText") == "frank");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.Equal);
        }

        #endregion

        [Test]
        public void UserGroup_WithDefaultSchemaForQueryingAttribute_Equals()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContext<UserGroup>(nullQueryableDataSource);
            var query = context.Where(x => x.Attributes["items"].Values["default"] == (object)"blah");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "Alias", UserGroupSchema.SchemaAlias, ValuePredicateType.Equal);
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", "blah", ValuePredicateType.Equal);
        }

        [Test]
        public void ContentViewModel_ContentType_Alias()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.ContentType.Alias == "NewsItem");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "Alias", "NewsItem", ValuePredicateType.Equal);
        }

        [Test]
        public void ContentViewModel_ContentType_Alias_NotEqualOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.ContentType.Alias != "NewsItem");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "Alias", "NewsItem", ValuePredicateType.NotEqual);
        }

        [Test]
        public void ContentViewModel_UtcCreated_LessThanOrEqualOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var dateTime = DateTimeOffset.Now;
            var query = context.Where(x => x.UtcCreated <= dateTime);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "UtcCreated", dateTime, ValuePredicateType.LessThanOrEqual);
        }

        [Test]
        public void ContentViewModel_UtcCreated_GreaterThanOrEqualOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var dateTime = DateTimeOffset.Now;
            var query = context.Where(x => x.UtcCreated >= dateTime);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "UtcCreated", dateTime, ValuePredicateType.GreaterThanOrEqual);
        }

        [Test]
        public void ContentViewModel_UtcCreated_SinglePredicateRange()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var dateTime = DateTimeOffset.Now;
            var query = context.Where(x => x.UtcCreated <= dateTime && x.UtcCreated >= dateTime);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "UtcCreated", dateTime, ValuePredicateType.GreaterThanOrEqual);
        }

        [Test]
        public void ContentViewModel_UtcCreated_ChainedPredicateRange()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var dateTime = DateTimeOffset.Now;
            var query = context
                .Where(x => x.UtcCreated <= dateTime)
                .Where(x => x.UtcCreated >= dateTime);

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "UtcCreated", dateTime, ValuePredicateType.GreaterThanOrEqual);
        }

        #region StringField extension x 2

        [Test]
        public void ContentViewModel_StringField_EqualsOperator_AndAlso_StringField_EqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.StringField("bodyText") == "frank" && x.StringField("title") == "bob");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.Equal);
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "title", "bob", ValuePredicateType.Equal);
        }

        [Test]
        public void ContentViewModel_StringField_EqualsOperator_AndAlso_StringField_NotEqualsOperator()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => x.StringField("bodyText") == "frank" && x.StringField("title") != "bob");

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.Equal);
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "title", "bob", ValuePredicateType.NotEqual);
        }

        [Test]
        public void ContentViewModel_StringField_NotEqualsMethod_ViaUnaryExpression_AndAlso_NumberField_NotGreaterThan_ViaUnaryExpression()
        {
            // Arrange
            var nullQueryableDataSource = new NullQueryableDataSource();
            var context = GenerateContentContext(nullQueryableDataSource);
            var query = context.Where(x => !x.StringField("bodyText").Equals("frank") && !(x.NumberField("items") > 2));

            // Act
            query.ToList();
            var generatedQuery = nullQueryableDataSource.GeneratedQueryDescription;

            // Assert
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "bodyText", "frank", ValuePredicateType.NotEqual);
            AssertingCriteriaVisitor.Check(generatedQuery.Criteria, "items", 2, ValuePredicateType.LessThan);
        }

        #endregion

        private static Queryable<Content> GenerateContentContext(NullQueryableDataSource nullQueryableDataSource)
        {
            return new Queryable<Content>(new Executor(nullQueryableDataSource, null), CustomQueryParser.CreateDefault());
        }

        private static Queryable<T> GenerateContext<T>(NullQueryableDataSource nullQueryableDataSource)
        {
            return new Queryable<T>(new Executor(nullQueryableDataSource, null), CustomQueryParser.CreateDefault());
        }
    }

    public class AssertingCriteriaVisitor : AbstractCriteriaVisitor<Expression>
    {
        public static void Check(Expression criteria, string fieldName, object fieldValue, ValuePredicateType valuePredicateType, string subFieldName = null)
        {
            var assertingVisitor = new AssertingCriteriaVisitor(fieldName, fieldValue, valuePredicateType, subFieldName);
            assertingVisitor.Visit(criteria);
            Assert.IsTrue(assertingVisitor.FoundMatch, "Found no expression for {0} {1} {2}. Found: {3}".InvariantFormat(fieldName, valuePredicateType.ToString(), fieldValue, assertingVisitor.MatchesFound));
        }

        private readonly string _fieldName;
        private readonly string _subFieldName;
        private readonly object _fieldValue;
        private readonly ValuePredicateType _valuePredicateType;
        private readonly StringBuilder _expressionsFound = new StringBuilder();


        public bool FoundMatch { get; set; }
        public string MatchesFound { get { return _expressionsFound.ToString(); } }

        public AssertingCriteriaVisitor(string fieldName, object fieldValue, ValuePredicateType valuePredicateType, string subFieldName = null)
        {
            _valuePredicateType = valuePredicateType;
            _fieldValue = fieldValue;
            _fieldName = fieldName;
            _subFieldName = subFieldName;
        }

        #region Overrides of AbstractCriteriaVisitor<Expression<Func<T,bool>>>

        public override Expression Visit(Expression criteria)
        {
            _expressionsFound.AppendFormat("\nExpression: Type {0}", criteria.NodeType);           
            
            return base.Visit(criteria);
        }

        public override Expression VisitNoCriteriaPresent()
        {
            _expressionsFound.AppendFormat("\nNo criteria supplied");
            return Expression.Constant(null);
        }

        public override Expression VisitBinary(BinaryExpression node)
        {
            _expressionsFound.AppendFormat("\nBinary: {0} and {1}", node.Left.Type.Name, node.Right.Type.Name);

            Visit(node.Left);
            Visit(node.Right);

            return node;
        }

        public override Expression VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            _expressionsFound.AppendFormat("\nSchema: {0} {1} {2}", node.SelectorExpression.Name, node.ValueExpression.ClauseType.ToString(), node.ValueExpression.Value);

            if (node.SelectorExpression.Name == _fieldName && node.ValueExpression.Value.Equals(_fieldValue) && node.ValueExpression.ClauseType == _valuePredicateType)
                FoundMatch = true;

            return node;
        }

        public override Expression VisitFieldPredicate(FieldPredicateExpression node)
        {
            if (node.SelectorExpression == null)
            {
                _expressionsFound.Append("(SelectorExpression == null)");
                return node;
            }

            _expressionsFound.AppendFormat("\nField: {0} {1} {2}", node.SelectorExpression.FieldName, node.ValueExpression.ClauseType.ToString(), node.ValueExpression.Value);           
            
            if (node.SelectorExpression.FieldName == _fieldName && node.ValueExpression.Value.Equals(_fieldValue) && node.ValueExpression.ClauseType == _valuePredicateType)
                if ((_subFieldName != null && node.SelectorExpression.ValueKey == _subFieldName) || _subFieldName == null)
                    FoundMatch = true;

            return node;
        }

        #endregion
    }

    public class NullQueryableDataSource : IQueryableDataSource
    {
        public QueryDescription GeneratedQueryDescription { get; set; }

        #region Implementation of IQueryableDataSource

        public T ExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            GeneratedQueryDescription = query;
            return default(T);
        }

        public T ExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            GeneratedQueryDescription = query;
            return default(T);
        }

        public IEnumerable<T> ExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            GeneratedQueryDescription = query;
            return Enumerable.Empty<T>();
        }

        #endregion

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get { throw new NotImplementedException(); } }

        #endregion
    }
}
