namespace Umbraco.Tests.CoreAndFramework.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using NUnit.Framework;
    using Umbraco.Framework;
    using Umbraco.Framework.Caching;
    using Umbraco.Framework.Data;
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;
    using Umbraco.Framework.Linq.QueryModel;
    using Umbraco.Framework.Persistence.Model.Constants;
    using Umbraco.Hive.Caching;

    public abstract class AbstractCacheProviderFixture
    {
        #region Properties

        protected AbstractCacheProvider CacheProvider { get; set; }

        #endregion

        #region Public Methods

        [Test]
        public void SlidingExpiredItemIsRemoved()
        {
            var stringKey = CacheKey.Create<string>("hello");
            var myObject1 = CacheProvider.GetOrCreate(stringKey,
                () => new CacheValueOf<TestCacheObject>(new TestCacheObject("hello-1"), new StaticCachePolicy(TimeSpan.FromSeconds(1))));

            Assert.NotNull(myObject1);

            Assert.That(myObject1.AlreadyExisted, Is.False);

            Assert.That(myObject1.WasInserted, Is.True);

            Assert.NotNull(myObject1.Value);

            Assert.NotNull(CacheProvider.GetValue(stringKey));

            Assert.NotNull(CacheProvider.Get(stringKey));

            Thread.Sleep(TimeSpan.FromSeconds(1.2d));

            Assert.Null(CacheProvider.Get(stringKey));
        }

        [Test]
        public void CanAddItemToCacheWithComplexKey()
        {
            var myObject1 = new TestCacheObject("test-1");
            var myObject2 = new TestCacheObject("test-2");
            var myObject3 = new TestCacheObject("test-3");

            CacheProvider.AddOrChangeValue(CacheKey.Create<string>("my-1"), myObject1);
            CacheProvider.AddOrChangeValue(CacheKey.Create<string>("my-2"), myObject2);
            CacheProvider.AddOrChangeValue(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob"), myObject3);

            var retrieve1 = CacheProvider.GetValue(CacheKey.Create<string>("my-1"));
            var retrieve1typed = CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<string>("my-1"));
            var retrieve2 = CacheProvider.GetValue(CacheKey.Create<string>("my-2"));
            var retrieve3 = CacheProvider.GetValue(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob"));

            Assert.IsNull(CacheProvider.GetValue(CacheKey.Create<string>("ah")));
            Assert.IsNull(CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<string>("ah")));
            Assert.That(retrieve1, Is.EqualTo(myObject1));
            Assert.That(retrieve1typed.Text, Is.EqualTo(myObject1.Text));
            Assert.That(retrieve2, Is.EqualTo(myObject2));
            Assert.That(retrieve3, Is.EqualTo(myObject3));
        }

        [Test]
        public void CanRemoveItemFromCacheWithDelegate()
        {
            var myObject1 = new TestCacheObject("test-1");
            var myObject2 = new TestCacheObject("bob");
            var myObject3 = new TestCacheObject("frank");

            CacheProvider.AddOrChangeValue(CacheKey.Create<string>("my-1"), myObject1);
            CacheProvider.AddOrChangeValue(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob"), myObject2);
            CacheProvider.AddOrChangeValue(CacheKey.Create<StrongClassKey>(x => x.MyName = "frank"), myObject3);

            var resultFilterClause = new ResultFilterClause(typeof(string), ResultFilterType.Any, 0);
            var fromClause = new FromClause(HiveId.Empty.ToString(), HierarchyScope.AncestorsOrSelf, FixedStatusTypes.Published);
            var fieldPredicateExpression = new FieldPredicateExpression("title", ValuePredicateType.Equal, "blah");


            //var key = new HiveQueryCacheKey(new QueryDescription(resultFilterClause, fromClause, fieldPredicateExpression, Enumerable.Empty<SortClause>()));
            var key = CacheKey.Create(new HiveQueryCacheKey(new QueryDescription(resultFilterClause, fromClause, fieldPredicateExpression, Enumerable.Empty<SortClause>())));
            CacheProvider.AddOrChangeValue(key, myObject3);

            Assert.NotNull(CacheProvider.GetValue(CacheKey.Create<string>("my-1")));
            Assert.NotNull(CacheProvider.GetValue(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob")));
            Assert.NotNull(CacheProvider.GetValue(CacheKey.Create<string>("my-1")));
            Assert.NotNull(CacheProvider.GetValue(key));

            CacheProvider.RemoveWhereKeyMatches<string>(x => x == "my-1");
            CacheProvider.RemoveWhereKeyMatches<StrongClassKey>(x => x.MyName == "bob");
            CacheProvider.RemoveWhereKeyMatches<HiveQueryCacheKey>(x => x.From.HierarchyScope == HierarchyScope.AncestorsOrSelf);

            Assert.Null(CacheProvider.Get(CacheKey.Create<string>("my-1")));
            Assert.Null(CacheProvider.GetValue(CacheKey.Create<string>("my-1")));
            Assert.Null(CacheProvider.Get(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob")));
            Assert.NotNull(CacheProvider.Get(CacheKey.Create<StrongClassKey>(x => x.MyName = "frank")));
        }

        [TestFixtureSetUp]
        public abstract void SetUp();

        [TestFixtureTearDown]
        public void TearDown()
        {
            CacheProvider.Dispose();
        }

        [Test]
        public void WhenAddingNonExistantItemResultShowsItemWasCreated()
        {
            bool check = false;

            var result = CacheProvider.GetOrCreate(
                CacheKey.Create<string>("hello"),
                () =>
                    {
                        check = true;
                        return new CacheValueOf<TestCacheObject>(new TestCacheObject("whatever"));
                    });

            Assert.That(check, Is.True);
            Assert.That(result.AlreadyExisted, Is.False);
            Assert.That(result.WasInserted, Is.True);
            Assert.That(result.WasUpdated, Is.False);
            Assert.That(result.ExistsButWrongType, Is.False);
            Assert.That(result.Value.Item.Text, Is.EqualTo("whatever"));
        }

        #endregion

        public class StrongClassKey : AbstractEquatableObject<StrongClassKey> 
        {
            #region Public Properties

            public string MyName { get; set; }

            #endregion

            #region Overrides of AbstractEquatableObject<StrongClassKey>

            /// <summary>
            /// Gets the natural id members.
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
            {
                yield return this.GetPropertyInfo(x => x.MyName);
            }

            #endregion
        }

        private class TestCacheObject
        {
            #region Constructors and Destructors

            public TestCacheObject(string text)
            {
                Date = DateTime.Now;
                Text = text;
            }

            #endregion

            #region Public Properties

            public DateTime Date { get; set; }
            public string Text { get; set; }

            #endregion
        }
    }
}