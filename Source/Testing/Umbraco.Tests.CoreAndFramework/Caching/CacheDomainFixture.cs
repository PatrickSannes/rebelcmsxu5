namespace Umbraco.Tests.CoreAndFramework.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using NUnit.Framework;
    using Umbraco.Framework;
    using Umbraco.Framework.Caching;
    using Umbraco.Framework.Data;
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;
    using Umbraco.Framework.Linq.QueryModel;
    using Umbraco.Framework.Persistence.Model.Constants;
    using Umbraco.Hive.Caching;

    [TestFixture]
    public class CacheDomainFixture
    {
        [Test]
        public void WhenCreatingSlidingStaticPolicySlidingExpiryIsValid()
        {
            var policy = new StaticCachePolicy(TimeSpan.FromSeconds(1.5d));
            var firstExpiryCheck = policy.GetExpiryDate();
            var firstNowCheck = DateTimeOffset.Now;
            Assert.That(firstExpiryCheck.Subtract(firstNowCheck).TotalSeconds, Is.InRange(1.4d, 1.5d));
            Assert.That(policy.IsExpired, Is.False);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.That(policy.GetExpiryDate(), Is.EqualTo(firstExpiryCheck));
            var debugNowCheck = DateTimeOffset.Now;
            var debugExpiryCheck = policy.GetExpiryDate();
            Assert.That(debugExpiryCheck.Subtract(debugNowCheck).TotalSeconds, Is.InRange(0.4d, 0.5d));
            Assert.That(policy.IsExpired, Is.False);
            Thread.Sleep(TimeSpan.FromSeconds(0.5d));
            Assert.That(policy.IsExpired, Is.True);
        }

        [Test]
        public void CacheKeySerializesToJson()
        {
            var key = CacheKey.Create<string>("my-1");
            var keyString = key.ToString();
            var keyJson = key.ToJson();

            Assert.That(keyString, Is.EqualTo(keyJson));
        }

        [Test]
        public void HiveRelationCacheKeyToFromJson()
        {
            var guid = Guid.NewGuid();
            var key = CacheKey.Create(new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Entity, new HiveId(guid), Direction.Siblings, FixedRelationTypes.ApplicationRelationType));
            var keyJson = key.ToJson();
            var keyBack = (CacheKey<HiveRelationCacheKey>)keyJson;
            var keyJsonTwice = keyBack.ToJson();
            Assert.That(keyBack, Is.Not.Null);
            Assert.That(keyJson, Is.EqualTo(keyJsonTwice));
        }

        [Test]
        public void HiveQueryCacheKeyToFromJson()
        {
            var guid = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            var resultFilterClause = new ResultFilterClause(typeof(string), ResultFilterType.Any, 0);
            var hiveId = new HiveId(guid);
            var expected = new HiveId(guid2);
            var fullHiveId = new HiveId(new Uri("content://blah/"), "nhibernate", new HiveIdValue(guid3));
            var fromClause = new FromClause(HiveId.Empty.ToString(), HierarchyScope.AncestorsOrSelf, FixedStatusTypes.Published, new[] { hiveId, expected, fullHiveId });
            var fieldPredicateExpression = new FieldPredicateExpression("title", ValuePredicateType.Equal, "blah");
            var fieldPredicateExpression2 = new FieldPredicateExpression("title", ValuePredicateType.Equal, "blah");
            var binary = Expression.MakeBinary(ExpressionType.ExclusiveOr, fieldPredicateExpression, fieldPredicateExpression2);
            var aSortClause = new SortClause(new FieldSelectorExpression("tag"), SortDirection.Descending, 2);

            //var key = new HiveQueryCacheKey(new QueryDescription(resultFilterClause, fromClause, fieldPredicateExpression, Enumerable.Empty<SortClause>()));
            var key = CacheKey.Create(new HiveQueryCacheKey(new QueryDescription(resultFilterClause, fromClause, fieldPredicateExpression, aSortClause.AsEnumerableOfOne())));
            var keyJson = key.ToJson();
            var keyBack = (CacheKey<HiveQueryCacheKey>)keyJson;
           // var keyJsonTwice = keyBack.ToJson();
            Assert.That(keyBack, Is.Not.Null);
            //Assert.That(keyJson, Is.EqualTo(keyJsonTwice));
            Assert.That(keyBack.Original.ResultFilter.ResultType, Is.EqualTo(typeof(string)));
            Assert.That(keyBack.Original.SortClauses.Count(), Is.EqualTo(key.Original.SortClauses.Count()));
            Assert.That(keyBack.Original.SortClauses.FirstOrDefault().Direction, Is.EqualTo(SortDirection.Descending));
            Assert.That(keyBack.Original.SortClauses.FirstOrDefault().Priority, Is.EqualTo(2));
            Assert.That(keyBack.Original.From.HierarchyScope, Is.EqualTo(key.Original.From.HierarchyScope));
            Assert.That(keyBack.Original.From.RequiredEntityIds.Count(), Is.EqualTo(3));
            Assert.That(keyBack.Original.From.RequiredEntityIds.FirstOrDefault(), Is.EqualTo(hiveId));
            Assert.That(keyBack.Original.From.RequiredEntityIds.FirstOrDefault().Value.Type, Is.EqualTo(HiveIdValueTypes.Guid));
            Assert.That(keyBack.Original.From.RequiredEntityIds.ElementAt(1), Is.EqualTo(expected));
            Assert.That(keyBack.Original.From.RequiredEntityIds.ElementAt(1).Value.Type, Is.EqualTo(HiveIdValueTypes.Guid));

            Assert.That(keyBack.Original.From.RequiredEntityIds.ElementAt(2), Is.EqualTo(fullHiveId));
            Assert.That(keyBack.Original.From.RequiredEntityIds.ElementAt(2).Value.Type, Is.EqualTo(HiveIdValueTypes.Guid));
            Assert.That(keyBack.Original.From.RequiredEntityIds.ElementAt(2).ProviderId, Is.EqualTo("nhibernate"));
            Assert.That(keyBack.Original.From.RequiredEntityIds.ElementAt(2).ProviderGroupRoot.ToString(), Is.EqualTo(fullHiveId.ProviderGroupRoot.ToString()));
        }

        [Test]
        public void CacheKeyEquals()
        {
            var one = CacheKey.Create<string>("this");
            var two = CacheKey.Create<string>("this");
            var three = (CacheKey)(CacheKey<string>)two.ToJson();

            Assert.That(one, Is.EqualTo(two));
            Assert.That(two, Is.EqualTo(three));
            Assert.That(two.GetHashCode(), Is.EqualTo(three.GetHashCode()));
        }
    }
}