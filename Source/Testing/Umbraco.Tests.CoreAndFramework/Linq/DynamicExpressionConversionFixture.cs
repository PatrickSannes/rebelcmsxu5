using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics.Expressions;

namespace Umbraco.Tests.CoreAndFramework.Linq
{
    [TestFixture]
    public class DynamicExpressionConversionFixture : AbstractExpressionConversionFixture<dynamic>
    {
        public override IEnumerable<QueryTestDescription<dynamic>> GetAdditionalTests()
        {
            yield break;
        }

        public override QueryTestDescription<dynamic> GetQueryById(HiveId value)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate("Id == @0", value));
        }

        public override QueryTestDescription<dynamic> GetQueryBySchemaAliasEqual(string alias)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate("ContentType.Alias == @0", alias));
        }

        public override IEnumerable<QueryTestDescription<dynamic>> GetQueryByStringAttributeEqual(string fieldName, string value)
        {
            yield return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldName + " == @0", value));
        }

        public override IEnumerable<QueryTestDescription<dynamic>> GetQueryByTwoStringAttributeEqual(string fieldName, string value, string fieldTwo, string valueTwo)
        {
            yield return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldName + " == @0 && " + fieldTwo + " == @1", value, valueTwo));
        }

        public override QueryTestDescription<dynamic> GetQueryNumberGreaterThan(string fieldAlias, int number)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldAlias + " > @0", number));
        }

        public override QueryTestDescription<dynamic> GetQueryNumberGreaterThanOrEqual(string fieldAlias, int number)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldAlias + " >= @0", number));
        }

        public override QueryTestDescription<dynamic> GetQueryNumberLessThan(string fieldAlias, int number)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldAlias + " < @0", number));
        }

        public override QueryTestDescription<dynamic> GetQueryNumberLessThanOrEqual(string fieldAlias, int number)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldAlias + " <= @0", number));
        }

        public override IEnumerable<QueryTestDescription<dynamic>> GetQueryByStringAttributeSubFieldEqual(string fieldName, string subFieldName, string value)
        {
            yield return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldName + ".@0 == @1", subFieldName, value));
        }

        public override QueryTestDescription<dynamic> GetQueryStringAttributeEndsWith(string fieldName, string fieldValue)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldName + ".EndsWith(@0)", fieldValue));
        }

        public override QueryTestDescription<dynamic> GetQueryStringAttributeStartsWith(string fieldName, string fieldValue)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldName + ".StartsWith(@0)", fieldValue));
        }

        public override QueryTestDescription<dynamic> GetQueryStringAttributeContainsString(string fieldName, string value)
        {
            return new QueryTestDescription<dynamic>(DynamicMemberMetadata.GetAsPredicate(fieldName + ".Contains(@0)", value));
        }
    }
}
