using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;

namespace Umbraco.Hive.Providers.Membership.Linq
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaTranslation;

    public class MembershipQueryVisitor : AbstractCriteriaVisitor<MembershipProviderQuery>
    {

        #region Overrides of AbstractCriteriaVisitor<IQueryOver<NodeVersion>>

        public override MembershipProviderQuery VisitNoCriteriaPresent()
        {
            return new MembershipProviderQuery(MembershipQueryType.None);
        }

        public override MembershipProviderQuery VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            throw new NotImplementedException();

            //var schemaPropertyname = node.SelectorExpression.Name;
            //var fieldValue = node.ValueExpression.Value.ToString();

            //switch (node.ValueExpression.ClauseType)
            //{
            //    case ValuePredicateType.Equal:                    
            //    case ValuePredicateType.NotEqual:                    
            //    default:
            //        throw new InvalidOperationException(
            //            "Cannot query an item by schema alias by any other operator than == or !=");
            //}
        }

        public override MembershipProviderQuery VisitFieldPredicate(FieldPredicateExpression node)
        {
            var fieldName = node.SelectorExpression.FieldName;
            var valueKey = node.SelectorExpression.ValueKey;
            var fieldValue = node.ValueExpression.Value.ToString();

            switch (fieldName.ToLowerInvariant())
            {
                case "id":
                    var idValue = GetIdValue(node);

                    switch (node.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.Equal:
                            return new MembershipProviderQuery(MembershipQueryType.ById, idValue);
                        case ValuePredicateType.NotEqual:
                            Func<IEnumerable<MembershipUser>, IEnumerable<MembershipUser>> filter = x => x.Where(m => m.ProviderUserKey != idValue);
                            return new MembershipProviderQuery(MembershipQueryType.Custom, filter);
                        default:
                            throw new InvalidOperationException(
                                "Cannot query an item by id by any other operator than == or !=");
                    }
            }

            if (!valueKey.IsNullOrWhiteSpace())
            {
                throw new NotImplementedException();
            }
            
            //now, select the field value....
            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    if (fieldName.InvariantEquals(MemberSchema.EmailAlias))
                    {                        
                        return new MembershipProviderQuery(MembershipQueryType.ByEmail, fieldValue);
                    }
                    if (fieldName.InvariantEquals(MemberSchema.UsernameAlias))
                    {
                        return new MembershipProviderQuery(MembershipQueryType.ByUsername, fieldValue);
                    }
                    throw new NotSupportedException("The Membership wrapper Hive provider does not support querying fields other than Email and Username");
                case ValuePredicateType.NotEqual:                    
                    if (fieldName.InvariantEquals(MemberSchema.EmailAlias))
                    {
                        Func<IEnumerable<MembershipUser>, IEnumerable<MembershipUser>> filter = x => x.Where(m => m.Email != fieldValue);
                        return new MembershipProviderQuery(MembershipQueryType.Custom, filter);
                    }
                    if (fieldName.InvariantEquals(MemberSchema.UsernameAlias))
                    {
                        Func<IEnumerable<MembershipUser>, IEnumerable<MembershipUser>> filter = x => x.Where(m => m.UserName != fieldValue);
                        return new MembershipProviderQuery(MembershipQueryType.Custom, filter);
                    }
                    throw new NotSupportedException("The Membership wrapper Hive provider does not support querying fields other than Email and Username");                                
                case ValuePredicateType.MatchesWildcard:
                case ValuePredicateType.Contains:              
                    if (fieldName.InvariantEquals(MemberSchema.EmailAlias))
                    {                        
                        return new MembershipProviderQuery(MembershipQueryType.ByEmail, fieldValue, ValuePredicateType.Contains);
                    }
                    if (fieldName.InvariantEquals(MemberSchema.UsernameAlias))
                    {
                        return new MembershipProviderQuery(MembershipQueryType.ByUsername, fieldValue, ValuePredicateType.Contains);
                    }
                    throw new NotSupportedException("The Membership wrapper Hive provider does not support querying fields other than Email and Username");     
                case ValuePredicateType.StartsWith:
                    if (fieldName.InvariantEquals(MemberSchema.EmailAlias))
                    {
                        return new MembershipProviderQuery(MembershipQueryType.ByEmail, fieldValue, ValuePredicateType.StartsWith);
                    }
                    if (fieldName.InvariantEquals(MemberSchema.UsernameAlias))
                    {
                        return new MembershipProviderQuery(MembershipQueryType.ByUsername, fieldValue, ValuePredicateType.StartsWith);
                    }
                    throw new NotSupportedException("The Membership wrapper Hive provider does not support querying fields other than Email and Username");     
                case ValuePredicateType.EndsWith:
                    if (fieldName.InvariantEquals(MemberSchema.EmailAlias))
                    {
                        return new MembershipProviderQuery(MembershipQueryType.ByEmail, fieldValue, ValuePredicateType.EndsWith);
                    }
                    if (fieldName.InvariantEquals(MemberSchema.UsernameAlias))
                    {
                        return new MembershipProviderQuery(MembershipQueryType.ByUsername, fieldValue, ValuePredicateType.EndsWith);
                    }
                    throw new NotSupportedException("The Membership wrapper Hive provider does not support querying fields other than Email and Username");               
                default:
                    throw new InvalidOperationException(
                        "This linq provider doesn't support a ClauseType of {0} for field {1}".InvariantFormat(
                            node.ValueExpression.ClauseType.ToString(), fieldName));
            }
        }

        private static object GetIdValue(FieldPredicateExpression node)
        {
            var idValue = node.ValueExpression.Type.IsAssignableFrom(typeof(HiveId)) 
                                 ? ((HiveId)node.ValueExpression.Value).Value.Value 
                                 : node.ValueExpression.Value;
            return idValue;
        }

        public override MembershipProviderQuery VisitBinary(BinaryExpression node)
        {
            throw new NotSupportedException("The Membership wrapper provider does not support binary query operators");

            //var left = Visit(node.Left);
            //var right = Visit(node.Right);

            //switch (node.NodeType)
            //{
            //    case ExpressionType.And:
            //    case ExpressionType.AndAlso:
            //        throw new NotSupportedException("The Membership wrapper provider does not support binary query operators");
            //    case ExpressionType.Or:
            //    case ExpressionType.OrElse:
            //        throw new NotSupportedException("The Membership wrapper provider does not support binary query operators");
            //}         
        }

        #endregion

    }
}
