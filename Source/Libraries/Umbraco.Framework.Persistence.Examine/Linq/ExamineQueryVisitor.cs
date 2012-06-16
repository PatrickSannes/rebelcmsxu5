using System;
using System.Linq.Expressions;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Lucene.Net.Search;

using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Framework.Persistence.Examine.Linq
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaTranslation;

    public class ExamineQueryVisitor : AbstractCriteriaVisitor<IQuery>
    {
        private readonly ExamineManager _manager;

        public ExamineQueryVisitor(ExamineManager manager)
        {
            _manager = manager;
        }

        private IQuery _criteria;

        #region Overrides of AbstractCriteriaVisitor<IQueryOver<NodeVersion>>

        public override IQuery VisitNoCriteriaPresent()
        {
            return _manager.CreateSearchCriteria().Must().EntityType<TypedEntity>().Compile();
        }

        public override IQuery VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            var schemaPropertyname = node.SelectorExpression.Name;
            var fieldValue = node.ValueExpression.Value.ToString();

            var c = _manager.CreateSearchCriteria();

            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    return c.Must().EntityType<TypedEntity>().Must().Field(FixedIndexedFields.SchemaAlias, fieldValue.Escape()).Compile();
                case ValuePredicateType.NotEqual:
                    return c.Must().EntityType<TypedEntity>().Not().Field(FixedIndexedFields.SchemaAlias, fieldValue.Escape()).Compile();                    
                default:
                    throw new InvalidOperationException(
                        "Cannot query an item by schema alias by any other operator than == or !=");
            }
        }

        public override IQuery VisitFieldPredicate(FieldPredicateExpression node)
        {
            var fieldName = node.SelectorExpression.FieldName;
            var valueKey = node.SelectorExpression.ValueKey;
            var fieldValue = node.ValueExpression.Value != null ? node.ValueExpression.Value.ToString() : string.Empty;

            var c = _manager.CreateSearchCriteria();

            switch (fieldName.ToLowerInvariant())
            {
                case "id":
                    Guid idValue = GetIdValue(node);

                    switch (node.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.Equal:
                            return c.Must().Id(idValue.ToString("N"), FixedIndexedFields.EntityId).Compile();
                        case ValuePredicateType.NotEqual:
                            //match all Id's (*) except...
                            return c.Must().Id("*").Not().Id(idValue.ToString("N"), FixedIndexedFields.EntityId).Compile();
                        default:
                            throw new InvalidOperationException(
                                "Cannot query an item by id by any other operator than == or !=");
                    }
            }

            var attributeFieldName = FixedAttributeIndexFields.AttributePrefix + fieldName;

            if (!valueKey.IsNullOrWhiteSpace())
            {                
                attributeFieldName += "." + valueKey;
            }
            
            //now, select the field value....
            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    var query = c.Must().EntityType<TypedEntity>().Must();

                    if (fieldValue != string.Empty)
                    {
                        return query.Field(attributeFieldName, fieldValue.Escape()).Compile();
                    }

                    return query.Field(attributeFieldName, string.Empty).Compile();
                case ValuePredicateType.NotEqual:
                    return c.Must().EntityType<TypedEntity>()
                        .Must()
                        .Field(attributeFieldName, "*")
                        .Not()
                        .Field(attributeFieldName, fieldValue)
                        .Compile();
                case ValuePredicateType.MatchesWildcard:
                case ValuePredicateType.Contains:
                    throw new NotSupportedException("Need to configure Examine to support start and end wildcards");
                    return c.Must().EntityType<TypedEntity>().Must().Field(attributeFieldName, fieldValue.MultipleCharacterWildcard()).Compile();                    
                case ValuePredicateType.StartsWith:
                    return c.Must().EntityType<TypedEntity>().Must().Field(attributeFieldName, fieldValue.MultipleCharacterWildcard()).Compile();
                case ValuePredicateType.EndsWith:
                    throw new NotSupportedException("Need to configure Examine to support start with wildcards");
                    return c.Must().EntityType<TypedEntity>().Must().Field(attributeFieldName, fieldValue.MultipleCharacterWildcard()).Compile();                    
                default:
                    throw new InvalidOperationException(
                        "This linq provider doesn't support a ClauseType of {0} for field {1}".InvariantFormat(
                            node.ValueExpression.ClauseType.ToString(), fieldName));
            }
        }

        private static Guid GetIdValue(FieldPredicateExpression node)
        {
            var idValue = Guid.Empty;
            if (node.ValueExpression.Type.IsAssignableFrom(typeof(HiveId)))
            {
                idValue = (Guid)((HiveId)node.ValueExpression.Value).Value;
            }
            else
            {
                Guid.TryParse(node.ValueExpression.Value.ToString(), out idValue);
            }
            return idValue;
        }

        public override IQuery VisitBinary(BinaryExpression node)
        {
            var left = (LuceneSearchCriteria)Visit(node.Left);
            var right = (LuceneSearchCriteria)Visit(node.Right);

            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return left.Join(right, BooleanClause.Occur.MUST);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return left.Join(right, BooleanClause.Occur.SHOULD);
            }

            throw new InvalidOperationException("This provider only supports binary expressions with And, AndAlso, Or, OrElse expression types. ExpressionType was {0}".InvariantFormat(node.NodeType.ToString()));
        }

        #endregion

    }
}
