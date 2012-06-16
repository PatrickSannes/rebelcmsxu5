using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Tests.CoreAndFramework.Linq
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    [TestFixture]
    public class TypedEntityExpressionConversionFixture : AbstractExpressionConversionFixture<TypedEntity>
    {
        public override IEnumerable<QueryTestDescription<TypedEntity>> GetAdditionalTests()
        {
            yield return new QueryTestDescription<TypedEntity>(x => x.Attributes["colour"] == "orange").WithField(new FieldPredicateExpression("colour", ValuePredicateType.Equal, "orange"));
            yield return new QueryTestDescription<TypedEntity>(x => x.Attribute<string>("colour") == "orange").WithField(new FieldPredicateExpression("colour", ValuePredicateType.Equal, "orange"));
            yield return new QueryTestDescription<TypedEntity>(x => x.Attribute<string>("car", "fuel") == "petrol").WithField(new FieldPredicateExpression("car", "fuel", ValuePredicateType.Equal, "petrol"));
        }

        public override QueryTestDescription<TypedEntity> GetQueryStringAttributeEndsWith(string fieldName, string fieldValue)
        {
            return new QueryTestDescription<TypedEntity>(x => x.Attribute<string>(fieldName).EndsWith(fieldValue));
        }

        public override QueryTestDescription<TypedEntity> GetQueryStringAttributeStartsWith(string fieldName, string fieldValue)
        {
            return new QueryTestDescription<TypedEntity>(x => x.Attribute<string>(fieldName).StartsWith(fieldValue));
        }

        public override QueryTestDescription<TypedEntity> GetQueryStringAttributeContainsString(string fieldName, string value)
        {
            return new QueryTestDescription<TypedEntity>(x => x.Attribute<string>(fieldName).Contains(value));
        }

        public override QueryTestDescription<TypedEntity> GetQueryById(HiveId value)
        {
            return new QueryTestDescription<TypedEntity>(x => x.Id == value);
        }

        public override QueryTestDescription<TypedEntity> GetQueryBySchemaAliasEqual(string alias)
        {
            return new QueryTestDescription<TypedEntity>(x => x.EntitySchema.Alias == alias);
        }

        public override IEnumerable<QueryTestDescription<TypedEntity>> GetQueryByStringAttributeEqual(string fieldName, string value)
        {
            yield return new QueryTestDescription<TypedEntity>(x => x.Attributes[fieldName] == value);
            yield return new QueryTestDescription<TypedEntity>(x => x.Attribute<string>(fieldName) == value);
        }

        public override IEnumerable<QueryTestDescription<TypedEntity>> GetQueryByTwoStringAttributeEqual(string fieldName, string value, string fieldTwo, string valueTwo)
        {
            yield return new QueryTestDescription<TypedEntity>(x => x.Attributes[fieldName] == value && x.Attributes[fieldTwo] == valueTwo);
            yield return new QueryTestDescription<TypedEntity>(x => x.Attribute<string>(fieldName) == value && x.Attribute<string>(fieldTwo) == valueTwo);
        }

        public override QueryTestDescription<TypedEntity> GetQueryNumberGreaterThan(string fieldAlias, int number)
        {
            return new QueryTestDescription<TypedEntity>(x => x.Attribute<int>(fieldAlias) > number);
        }

        public override QueryTestDescription<TypedEntity> GetQueryNumberGreaterThanOrEqual(string fieldAlias, int number)
        {
            return new QueryTestDescription<TypedEntity>(x => x.Attribute<int>(fieldAlias) >= number);
        }

        public override QueryTestDescription<TypedEntity> GetQueryNumberLessThan(string fieldAlias, int number)
        {
            return new QueryTestDescription<TypedEntity>(x => x.Attribute<int>(fieldAlias) < number);
        }

        public override QueryTestDescription<TypedEntity> GetQueryNumberLessThanOrEqual(string fieldAlias, int number)
        {
            return new QueryTestDescription<TypedEntity>(x => x.Attribute<int>(fieldAlias) <= number);
        }

        public override IEnumerable<QueryTestDescription<TypedEntity>> GetQueryByStringAttributeSubFieldEqual(string fieldName, string subFieldName, string value)
        {
            yield return new QueryTestDescription<TypedEntity>(x => x.Attributes[fieldName].Values[subFieldName] == (object)value);
            yield return new QueryTestDescription<TypedEntity>(x => x.Attribute<string>(fieldName, subFieldName) == value);
        }
    }
}
