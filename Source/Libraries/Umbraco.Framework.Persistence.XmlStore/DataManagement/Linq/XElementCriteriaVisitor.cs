using System;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Umbraco.Framework.DataManagement.Linq.CriteriaGeneration.Expressions;
using Umbraco.Framework.DataManagement.Linq.CriteriaTranslation;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement.Linq
{
    public class XElementCriteriaVisitor : AbstractSimpleCriteriaVisitor<XElement>
    {
        public override Expression<Func<XElement, bool>> VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            throw new NotImplementedException();
        }

        public override Expression<Func<XElement, bool>> VisitFieldPredicate(FieldPredicateExpression node)
        {
            var fieldName = node.SelectorExpression.FieldName;
            var fieldValue = node.ValueExpression.Value.ToString();

            // Normalise incoming field names
            switch (fieldName.ToLowerInvariant())
            {
                case "id":
                    fieldName = "id";
                    var value = string.Empty;
                    if (node.ValueExpression.Type.IsAssignableFrom(typeof(HiveId)))
                    {
                        value = ((HiveId) node.ValueExpression.Value).ToString(HiveIdFormatStyle.AutoSingleValue);
                    }
                    else
                    {
                        value = node.ValueExpression.Value.ToString();
                    }
                    switch (node.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.Equal:
                            return x => x.Attributes(fieldName).Any() && string.Equals((string)x.Attribute(fieldName), value, StringComparison.InvariantCultureIgnoreCase);
                        case ValuePredicateType.NotEqual:
                            return x => x.Attributes(fieldName).Any() && !string.Equals((string)x.Attribute(fieldName), value, StringComparison.InvariantCultureIgnoreCase);
                        default:
                            throw new InvalidOperationException("Cannot query an item by id by any other method than Equals or NotEquals");
                    }
            }

            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    return x => x.Attributes(fieldName).Any() && string.Equals((string)x.Attribute(fieldName), fieldValue, StringComparison.InvariantCultureIgnoreCase);
                case ValuePredicateType.EndsWith:
                    return x => x.Attributes(fieldName).Any() && ((string)x.Attribute(fieldName)).EndsWith(fieldValue,StringComparison.InvariantCultureIgnoreCase);
            }

            // Only get here if the query has something we don't support
            return x => false;
        }
    }
}