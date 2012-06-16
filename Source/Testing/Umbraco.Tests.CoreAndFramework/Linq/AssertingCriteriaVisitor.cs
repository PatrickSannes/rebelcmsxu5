using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Umbraco.Framework;

namespace Umbraco.Tests.CoreAndFramework.Linq
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaTranslation;

    public class AssertingCriteriaVisitor<TModel> : AbstractCriteriaVisitor<Expression>
    {
        public static void Check(QueryTestDescription<TModel> tester, Expression rewrittenQuery)
        {
            var assertingVisitor = new AssertingCriteriaVisitor<TModel>(tester);
            assertingVisitor.Visit(rewrittenQuery);
            Assert.IsTrue(assertingVisitor.FoundMatch, "Found no expression for {0}.\nFound: {1}".InvariantFormat(tester.ToString(), assertingVisitor.MatchesFound));
        }

        private readonly QueryTestDescription<TModel> _tester;
        private readonly StringBuilder _expressionsFound = new StringBuilder();


        public bool FoundMatch { get; set; }
        public string MatchesFound { get { return _expressionsFound.ToString(); } }

        public AssertingCriteriaVisitor(QueryTestDescription<TModel> tester)
        {
            _tester = tester;
        }

        #region Overrides of AbstractCriteriaVisitor<Expression<Func<T,bool>>>

        public override Expression Visit(Expression criteria)
        {
            _expressionsFound.AppendFormat("\nExpression: Type {0}", criteria.GetType().Name);

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
            _expressionsFound.AppendFormat("\n{0}", node);

            foreach (var schemaPredicateExpression in _tester.Schemas)
            {
                if (node.SelectorExpression.Name == schemaPredicateExpression.SelectorExpression.Name
                    && node.ValueExpression.Value.Equals(schemaPredicateExpression.ValueExpression.Value) 
                    && node.ValueExpression.ClauseType == schemaPredicateExpression.ValueExpression.ClauseType)
                {
                    FoundMatch = true;
                    break;
                }
            }

            return node;
        }

        public override Expression VisitFieldPredicate(FieldPredicateExpression node)
        {
            _expressionsFound.AppendFormat("\n{0}", node);

            foreach (var fieldPredicateExpression in _tester.Fields)
            {
                if (node.SelectorExpression.FieldName == fieldPredicateExpression.SelectorExpression.FieldName
                    && node.ValueExpression.Value.Equals(fieldPredicateExpression.ValueExpression.Value)
                    && node.ValueExpression.ClauseType == fieldPredicateExpression.ValueExpression.ClauseType)

                    if ((fieldPredicateExpression.SelectorExpression.ValueKey != null && node.SelectorExpression.ValueKey == fieldPredicateExpression.SelectorExpression.ValueKey)
                        || fieldPredicateExpression.SelectorExpression.ValueKey == null)
                    {
                        FoundMatch = true;
                        break;
                    }
            }

            return node;
        }

        #endregion
    }
}
