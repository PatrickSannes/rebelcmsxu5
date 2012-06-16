using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Criterion;

using Umbraco.Framework.Persistence.RdbmsModel;
using Attribute = Umbraco.Framework.Persistence.RdbmsModel.Attribute;

namespace Umbraco.Framework.Persistence.NHibernate.Linq
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaTranslation;

    public class NhQueryOverVisitor : AbstractCriteriaVisitor<QueryOver<NodeVersion>>
    {
        private QueryOver<NodeVersion> _queryBuilder;

        #region Overrides of AbstractCriteriaVisitor<IQueryOver<NodeVersion>>

        public override QueryOver<NodeVersion> VisitNoCriteriaPresent()
        {
            return QueryOver.Of<NodeVersion>().Select(x => x.Id);
        }

        public override QueryOver<NodeVersion> VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            var schemaPropertyname = node.SelectorExpression.Name;
            var fieldValue = node.ValueExpression.Value.ToString();

            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    return QueryOver.Of<NodeVersion>()
                        .JoinQueryOver<AttributeSchemaDefinition>(x => x.AttributeSchemaDefinition)
                        .Where(x => x.Alias == fieldValue)
                        .Select(x => x.Id);
                case ValuePredicateType.NotEqual:
                    return QueryOver.Of<NodeVersion>()
                        .JoinQueryOver<AttributeSchemaDefinition>(x => x.AttributeSchemaDefinition)
                        .Where(x => x.Alias != fieldValue)
                        .Select(x => x.Id);
                default:
                    throw new InvalidOperationException(
                        "Cannot query an item by schema alias by any other operator than == or !=");
            }
        }

        public override QueryOver<NodeVersion> VisitFieldPredicate(FieldPredicateExpression node)
        {
            var fieldName = node.SelectorExpression.FieldName;
            var valueKey = node.SelectorExpression.ValueKey;
            var fieldValue = node.ValueExpression.Value;

            switch (fieldName.ToLowerInvariant())
            {
                case "id":
                    Guid idValue = GetIdValue(node);

                    switch (node.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.Equal:
                            return QueryOver.Of<NodeVersion>().Where(x => x.Node.Id == idValue).Select(x => x.Id);
                        case ValuePredicateType.NotEqual:
                            return QueryOver.Of<NodeVersion>().Where(x => x.Node.Id != idValue).Select(x => x.Id);;
                        default:
                            throw new InvalidOperationException(
                                "Cannot query an item by id by any other operator than == or !=");
                    }
            }

            NodeVersion aliasNodeVersion = null;
            Attribute aliasAttribute = null;
            AttributeDefinition aliasAttributeDefinition = null;
            AttributeStringValue aliasStringValue = null;
            AttributeLongStringValue aliasLongStringValue = null;
            NodeRelation aliasNodeRelation = null;
            AttributeDateValue aliasDateValue = null;

            //TODO: This is going to need to lookup more than string values

            var queryString = QueryOver.Of<NodeVersion>(() => aliasNodeVersion)
                .JoinQueryOver<Attribute>(() => aliasNodeVersion.Attributes, () => aliasAttribute)
                .JoinQueryOver<AttributeDefinition>(() => aliasAttribute.AttributeDefinition, () => aliasAttributeDefinition)
                .Left.JoinQueryOver<AttributeStringValue>(() => aliasAttribute.AttributeStringValues, () => aliasStringValue)
                .Left.JoinQueryOver<AttributeLongStringValue>(() => aliasAttribute.AttributeLongStringValues, () => aliasLongStringValue)
                .Left.JoinQueryOver(() => aliasAttribute.AttributeDateValues, () => aliasDateValue)
                //select the field name...
                .Where(x => aliasAttributeDefinition.Alias == fieldName);
            
            if (!valueKey.IsNullOrWhiteSpace())
            {
                //if the value key is specified, then add that to the query
                queryString = queryString.And(() => aliasStringValue.ValueKey == valueKey || aliasLongStringValue.ValueKey == valueKey).Select(x => x.Id);
            }


            //now, select the field value....
            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    var sqlCeCompatibleQuery =
                        Restrictions.Or(
                            Restrictions.Eq(
                                Projections.Property<AttributeStringValue>(x => aliasStringValue.Value), fieldValue),
                            Restrictions.Like(
                                Projections.Property<AttributeLongStringValue>(x => aliasLongStringValue.Value),
                                fieldValue as string,
                                MatchMode.Exact));

                    return queryString.And(sqlCeCompatibleQuery).Select(x => x.Id);
                case ValuePredicateType.NotEqual:
                    return queryString.And(x => aliasStringValue.Value != fieldValue).Select(x => x.Id);
                case ValuePredicateType.MatchesWildcard:
                case ValuePredicateType.Contains:
                    return queryString.And(Restrictions.Like(Projections.Property<AttributeStringValue>(x => aliasStringValue.Value), fieldValue as string, MatchMode.Anywhere)).Select(x => x.Id);
                case ValuePredicateType.StartsWith:
                    return queryString.And(Restrictions.Like(Projections.Property<AttributeStringValue>(x => aliasStringValue.Value), fieldValue as string, MatchMode.Start)).Select(x => x.Id);
                case ValuePredicateType.EndsWith:
                    return queryString.And(Restrictions.Like(Projections.Property<AttributeStringValue>(x => aliasStringValue.Value), fieldValue as string, MatchMode.End)).Select(x => x.Id);
                case ValuePredicateType.LessThanOrEqual:
                    return
                        queryString.And(
                            Restrictions.Le(Projections.Property<AttributeDateValue>(x => aliasDateValue.Value),
                                            (DateTimeOffset) fieldValue));
                case ValuePredicateType.GreaterThanOrEqual:
                    return
                        queryString.And(
                            Restrictions.Ge(Projections.Property<AttributeDateValue>(x => aliasDateValue.Value),
                                            (DateTimeOffset)fieldValue));
                default:
                    throw new InvalidOperationException(
                        "This linq provider doesn't support a ClauseType of {0} for field {1}".InvariantFormat(
                            node.ValueExpression.ClauseType.ToString(), fieldName));
            }
        }

        private static Guid GetIdValue(FieldPredicateExpression node)
        {
            var idValue = Guid.Empty;
            if (node.ValueExpression.Type.IsAssignableFrom(typeof (HiveId)))
            {
                idValue = (Guid)((HiveId)node.ValueExpression.Value).Value;
            }
            else
            {
                Guid.TryParse(node.ValueExpression.Value.ToString(), out idValue);
            }
            return idValue;
        }

        public override QueryOver<NodeVersion> VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return QueryOver.Of<NodeVersion>()
                            .Where(Restrictions.Conjunction()
                                .Add(Subqueries.PropertyIn(Projections.Property<NodeVersion>(x => x.Id).PropertyName, left.DetachedCriteria))
                                .Add(Subqueries.PropertyIn(Projections.Property<NodeVersion>(x => x.Id).PropertyName, right.DetachedCriteria)))
                            .Select(x => x.Id);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    {
                        return QueryOver.Of<NodeVersion>()
                            .Where(Restrictions.Disjunction()
                                .Add(Subqueries.PropertyIn(Projections.Property<NodeVersion>(x => x.Id).PropertyName, left.DetachedCriteria))
                                .Add(Subqueries.PropertyIn(Projections.Property<NodeVersion>(x => x.Id).PropertyName, right.DetachedCriteria)))
                            .Select(x => x.Id);
                    }
            }

            throw new InvalidOperationException("This provider only supports binary expressions with And, AndAlso, Or, OrElse expression types. ExpressionType was {0}".InvariantFormat(node.NodeType.ToString()));
        }

        #endregion
    }
}