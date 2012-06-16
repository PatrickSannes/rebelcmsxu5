using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Cms.Web.System;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics.Expressions;
using Umbraco.Framework.Expressions.Remotion;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Hive.Tasks;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders
{
    using System.Threading;
    using Umbraco.Framework.Diagnostics;
    using Umbraco.Framework.Linq;

    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Persistence.Model.Versioning;

    public abstract class AbstractProviderQueryTests
    {
        [TestFixtureSetUp]
        public void SetupFixture()
        {
            DataHelper.SetupLog4NetForTests();
        }

        protected virtual string DynamicAttributeAliasForQuerying
        {
            get { return "aliasForQuerying"; }
        }

        protected virtual string AttributeAlias2ForQuerying
        {
            get { return HiveModelCreationHelper.DefAlias2WithType1; }
        }

        protected virtual string AttributeAlias1ForQuerying
        {
            get { return HiveModelCreationHelper.DefAlias1WithType1; }
        }

        protected virtual string SchemaAliasForQuerying
        {
            get { return "schema-alias1"; }
        }

        [Test]
        public void CountUsesSpecifiedRevision()
        {
            // Arrange
            LogHelper.TraceIfEnabled(typeof(QueryExtensions), "In CountUsesSpecifiedRevision");
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var twoRevisionsOfOneEntity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);
            AddRevision(twoRevisionsOfOneEntity, FixedStatusTypes.Published);
            AddRevision(twoRevisionsOfOneEntity, FixedStatusTypes.Published);

            var revisionStatusType = new RevisionStatusType("custom", "custom for test");
            var oneRevisionOfAnEntity = CreateEntityForTest(Guid.NewGuid(), Guid.NewGuid(), ProviderSetup);
            AddRevision(oneRevisionOfAnEntity, revisionStatusType);

            var anotherRevisionOfSameType = CreateEntityForTest(Guid.NewGuid(), Guid.NewGuid(), ProviderSetup);
            AddRevision(anotherRevisionOfSameType, revisionStatusType);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var countOfCustomType = uow.Repositories.OfRevisionType(revisionStatusType.Alias).Count();
                var countOfPublished = uow.Repositories.OfRevisionType(FixedStatusTypes.Published).Count();

                // Assert
                Assert.That(countOfPublished, Is.EqualTo(1));
                Assert.That(countOfCustomType, Is.EqualTo(2));

                Thread.Sleep(500); // Let profiler catch up
            }
        }

        [Test]
        public void WhenExecutingCount_WithoutRevisionSpecified_OnlyPublishedResultsAreIncluded()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            AddRevision(entity, FixedStatusTypes.Published);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var result = uow.Repositories.Count(x => x.Id == (HiveId)newGuid);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }
        }

        private void AddRevision(TypedEntity entity, RevisionStatusType revisionStatusType)
        {
            using (var uow = GroupUnitFactory.Create())
            {
                // Make a new revision that is published
                var revision = new Revision<TypedEntity>(entity);
                revision.MetaData.StatusType = revisionStatusType;
                uow.Repositories.Revisions.AddOrUpdate(revision);
                uow.Complete();
            }
        }

        [Test]
        public void Temp_TypedEntity_WithDynamicQuery()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var dynQuery = DynamicMemberMetadata.GetAsPredicate(DynamicAttributeAliasForQuerying + " == @0", "my-new-value");
                var query = uow.Repositories.OfRevisionType("created").Where(dynQuery).Cast<TypedEntity>();

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void UserGroup_ByName_EqualsOperator()
        {
            // Arrange
            var permission = new Lazy<Permission, PermissionMetadata>(() => new ViewPermission(), new PermissionMetadata(new Dictionary<string, object>()));
            var userGroup = CoreCmsData.RequiredCoreUserGroups(Enumerable.Repeat(permission, 1)).FirstOrDefault(x => x.Name == "Administrator");
            Assert.NotNull(userGroup);

            using (var uow = GroupUnitFactory.Create())
            {
                uow.Repositories.AddOrUpdate(new SystemRoot());
                uow.Repositories.AddOrUpdate(FixedEntities.UserGroupVirtualRoot);
                uow.Repositories.AddOrUpdate(userGroup);
                uow.Complete();
            }

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var getAdminByName = uow.Repositories.Query<UserGroup>().FirstOrDefault(x => x.Name == "Administrator");
                Assert.NotNull(getAdminByName);
                Assert.That(userGroup.Id.Value, Is.EqualTo(getAdminByName.Id.Value));
            }
        }

        [Test]
        public void WhenEntitiesAreQueried_ResultsArePutInScopedCache()
        {
            // Arrange
            var permission = new Lazy<Permission, PermissionMetadata>(() => new ViewPermission(), new PermissionMetadata(new Dictionary<string, object>()));
            var userGroup = CoreCmsData.RequiredCoreUserGroups(Enumerable.Repeat(permission, 1)).FirstOrDefault(x => x.Name == "Administrator");
            Assert.NotNull(userGroup);
            using (var uow = GroupUnitFactory.Create())
            {
                uow.Repositories.AddOrUpdate(new SystemRoot());
                uow.Repositories.AddOrUpdate(FixedEntities.UserGroupVirtualRoot);
                uow.Repositories.AddOrUpdate(userGroup);
                uow.Complete();
            }

            // Assert - check single result
            using (var uow = GroupUnitFactory.Create())
            {
                // Cause the task to be fired
                Expression<Func<UserGroup, bool>> expression = x => x.Name == "Administrator";
                var getAdminByName = uow.Repositories.Query<UserGroup>().FirstOrDefault(expression);
                Assert.NotNull(getAdminByName);

                // Generate what should be an exact-same QueryDescription for the above query, to check the cache
                var executor = new Executor(uow.Repositories.QueryableDataSource, Queryable<UserGroup>.GetBinderFromAssembly());
                var queryable = new Queryable<UserGroup>(executor);
                queryable.FirstOrDefault(expression);
                var description = executor.LastGeneratedDescription;

                // Assert the task has been fired
                Assert.That(uow.UnitScopedCache.GetOrCreate(new QueryDescriptionCacheKey(description), () => null), Is.Not.Null);
            }

            // Assert - check many results
            using (var uow = GroupUnitFactory.Create())
            {
                // Cause the task to be fired
                Expression<Func<UserGroup, bool>> expression = x => x.Name == "Administrator";
                var getAdminByName = uow.Repositories.Query<UserGroup>().Where(expression).ToList();
                Assert.NotNull(getAdminByName.FirstOrDefault());

                // Generate what should be an exact-same QueryDescription for the above query, to check the cache
                var executor = new Executor(uow.Repositories.QueryableDataSource, Queryable<UserGroup>.GetBinderFromAssembly());
                var queryable = new Queryable<UserGroup>(executor);
                queryable.Where(expression).ToList();
                var description = executor.LastGeneratedDescription;

                // Assert the task has been fired
                Assert.That(uow.UnitScopedCache.GetOrCreate(new QueryDescriptionCacheKey(description), () => null), Is.Not.Null);
            }
        }

        [Test]
        public void TypedEntity_Published_ByEntitSchemaAlias_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateContentForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().OfRevisionType(FixedStatusTypes.Published).Where(x => x.EntitySchema.Alias == SchemaAliasForQuerying);

                // Assert
                Assert.AreEqual(1, query.Count());
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
                Assert.AreEqual("schema-alias1", item.EntitySchema.Alias);
            }
        }

        [Test]
        public void TypedEntity_ByEntitSchemaAlias_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.EntitySchema.Alias == SchemaAliasForQuerying);

                // Assert
                Assert.AreEqual(1, query.Count());
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
                Assert.AreEqual("schema-alias1", item.EntitySchema.Alias);
            }
        }

        [Test]
        public void UserGroup_ByName_IncludingInferredSchemaType_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            var userGroup = new UserGroup() {Name = "Anonymous", Id = new HiveId(newGuid)};

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(new SystemRoot());
                uow.EntityRepository.AddOrUpdate(FixedEntities.UserGroupVirtualRoot);
                uow.EntityRepository.AddOrUpdate(userGroup);
                uow.Complete();
            }

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var checkSchemaExists = uow.Repositories.Schemas.Get<EntitySchema>(new UserGroupSchema().Id);
                Assert.NotNull(checkSchemaExists);

                var genericQuery = uow.Repositories.QueryContext.Query<TypedEntity>()
                    .Where(x => x.Attribute<string>(NodeNameAttributeDefinition.AliasValue) == "Anonymous")
                    .ToList()
                    .Select(x => x.Id);

                // Assert
                Assert.AreEqual(1, genericQuery.Count());

                var queryAll = uow.Repositories.QueryContext.Query<UserGroup>().ToList();
                Assert.That(queryAll.Count(), Is.EqualTo(1));

                var queryAll2 = uow.Repositories.Query<UserGroup>().ToList();
                Assert.That(queryAll2.Count(), Is.EqualTo(1));

                var query = uow.Repositories.QueryContext.Query<UserGroup>()
                    .Where(x => x.Name == "Anonymous").ToList();

                // Assert
                Assert.AreEqual(1, query.Count());
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
                Assert.AreEqual(new UserGroupSchema().Alias, item.EntitySchema.Alias);
            }
        }

        [Test]
        public void TypedEntity_ById_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Id == (HiveId)newGuid);

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void TypedEntity_ById_EqualsOperator_UsingLinq()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = from x in uow.Repositories.QueryContext.Query()
                            where x.Id == (HiveId)newGuid
                            select x;

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void TypedEntity_ByAttributeValue_NotEqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring");

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuidRedHerring, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void WhenTypedEntity_QueriedWithStringEquals_AndOrderBy_ResultsAreOrdered()
        {
            var item1Id = Guid.NewGuid();
            var item2Id = Guid.NewGuid();
            var item3Id = Guid.NewGuid();
            var parentId = Guid.NewGuid();

            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            // Create schema
            var schema = Hive.Cms().NewContentType<EntitySchema, IContentStore>("withTitle")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("random", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("tag", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("bodyText", type => type.UseExistingType("richTextEditor"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            Assert.True(schema.Success);

            var item1 = new Content();
            item1.SetupFromSchema(schema.Item);
            item1.Id = new HiveId(item1Id);
            item1["title"] = "Item1";
            item1["random"] = "Random3";
            item1["tag"] = "apple";

            var item2 = new Content();
            item2.SetupFromSchema(schema.Item);
            item2.Id = new HiveId(item2Id);
            item2["title"] = "Item2";
            item2["random"] = "Random1";
            item2["tag"] = "blueberry";

            var item3 = new Content();
            item3.SetupFromSchema(schema.Item);
            item3.Id = new HiveId(item3Id);
            item3["title"] = "Item3";
            item3["random"] = "Random2";
            item3["tag"] = "apple";

            var writerResult = Hive.AutoCommitTo<IContentStore>(x =>
                {
                    x.Repositories.AddOrUpdate(item1);
                    x.Repositories.AddOrUpdate(item2);
                    x.Repositories.AddOrUpdate(item3);
                });

            Assert.True(writerResult.WasCommitted);

            // Check can get the items normally
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                Assert.True(uow.Repositories.Exists<Content>(item1.Id));
                Assert.True(uow.Repositories.Exists<Content>(item2.Id));
                Assert.True(uow.Repositories.Exists<Content>(item3.Id));
            }

            // query all with sortorder - first check is actually order of insertion anyway
            var allQuery_NaturalSort = Hive.QueryContent().OrderBy(x => x.Attribute<string>("title")).ToArray();
            Assert.That(allQuery_NaturalSort.Any());
            Assert.That(allQuery_NaturalSort[0]["title"], Is.EqualTo("Item1"));
            Assert.That(allQuery_NaturalSort[1]["title"], Is.EqualTo("Item2"));
            Assert.That(allQuery_NaturalSort[2]["title"], Is.EqualTo("Item3"));

            var allQuerySortByTag = Hive.QueryContent().OrderBy(x => x.Attribute<string>("tag")).ToArray();
            Assert.That(allQuerySortByTag.Any());
            Assert.That(allQuerySortByTag[0]["tag"], Is.EqualTo("apple"));
            Assert.That(allQuerySortByTag[0]["random"], Is.EqualTo("Random3"));
            Assert.That(allQuerySortByTag[1]["tag"], Is.EqualTo("apple"));
            Assert.That(allQuerySortByTag[1]["random"], Is.EqualTo("Random2"));
            Assert.That(allQuerySortByTag[2]["tag"], Is.EqualTo("blueberry"));
            Assert.That(allQuerySortByTag[2]["random"], Is.EqualTo("Random1"));

            var allQuerySortByTagThenRandom = Hive.QueryContent().OrderBy(x => x.Attribute<string>("tag")).ThenBy(x => x.Attribute<string>("random")).ToArray();
            Assert.That(allQuerySortByTagThenRandom.Any());
            Assert.That(allQuerySortByTagThenRandom[0]["tag"], Is.EqualTo("apple"));
            Assert.That(allQuerySortByTagThenRandom[0]["random"], Is.EqualTo("Random2"));
            Assert.That(allQuerySortByTagThenRandom[1]["tag"], Is.EqualTo("apple"));
            Assert.That(allQuerySortByTagThenRandom[1]["random"], Is.EqualTo("Random3"));
            Assert.That(allQuerySortByTagThenRandom[2]["tag"], Is.EqualTo("blueberry"));
            Assert.That(allQuerySortByTagThenRandom[2]["random"], Is.EqualTo("Random1"));

            // query invoking the executesingle methods
            var firstByTagDescending = Hive.QueryContent().OrderByDescending(x => x.Attribute<string>("tag")).FirstOrDefault();
            Assert.NotNull(firstByTagDescending);
            Assert.That(firstByTagDescending["tag"], Is.EqualTo("blueberry"));

            var singleByTagDescending = Hive.QueryContent().OrderByDescending(x => x.Attribute<string>("tag")).SingleOrDefault(x => x.Attribute<string>("random") == "Random2");
            Assert.NotNull(singleByTagDescending);
            Assert.That(singleByTagDescending["tag"], Is.EqualTo("apple"));
        }

        [Test]
        public void TypedEntity_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring");

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void TypedEntity_ByAttributeSubValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName") == "my-test-route");

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void TypedEntity_Count_WithAndAlsoBinary_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring" && x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                Assert.AreEqual(1, query.Count());
            }
        }

        [Test]
        public void TypedEntity_Count_WithOrElseBinary_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = RepositoryGroupExtensions.Create((GroupUnitFactory)GroupUnitFactory))
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring" || x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                Assert.AreEqual(2, query.Count());
            }
        }

        [Test]
        public void TypedEntity_Count_WithComplexBinary_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => (x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring" || x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1") && x.Id == (HiveId)newGuid);

                // Assert
                Assert.AreEqual(1, query.Count());
            }
        }

        [Test]
        public void TypedEntity_Count_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                // The mocked entities have attributes with alias-1 and my-test-value1, and we've added two of them in SetupTestData
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                Assert.AreEqual(2, query.Count());
            }
        }

        [Test]
        public void UsingDefaultEnumerator_OnEntityRepositoryGroup_TypedEntity_SingleOrDefault_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring");

                // Assert
                var singleOrDefault = query.SingleOrDefault();
                Assert.IsNotNull(singleOrDefault);
                Assert.AreEqual(newGuid, (Guid)singleOrDefault.Id.Value);

                // Now do another query which should return two, and ensure SingleOrDefault chucks an error our way
                var queryToFail = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                try
                {
                    var resultToFail = queryToFail.SingleOrDefault();
                    Assert.Fail("SingleOrDefault did not throw an error; result could should have been 2");
                }
                catch (InvalidOperationException)
                {
                    /* Do nothing */
                }
            }
        }

        [Test]
        public void TypedEntity_SingleOrDefault_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring");

                // Assert
                var singleOrDefault = query.SingleOrDefault();
                Assert.IsNotNull(singleOrDefault);
                Assert.AreEqual(newGuid, (Guid)singleOrDefault.Id.Value);

                // Now do another query which should return two, and ensure SingleOrDefault chucks an error our way
                var queryToFail = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                try
                {
                    var resultToFail = queryToFail.SingleOrDefault();
                    Assert.Fail("SingleOrDefault did not throw an error; result could should have been 2");
                }
                catch (InvalidOperationException)
                {
                    /* Do nothing */
                }
            }
        }

        protected abstract ProviderSetup ProviderSetup { get; }
        protected abstract GroupUnitFactory GroupUnitFactory { get; }

        private IHiveManager _hiveManager;
        protected IHiveManager Hive
        {
            get
            {
                if (_hiveManager != null) return _hiveManager;
                var wildcardUriMatch = new WildcardUriMatch(GroupUnitFactory.IdRoot);
                var abstractProviderBootstrapper = ProviderSetup.Bootstrapper ?? new NoopProviderBootstrapper();
                var providerUnitFactory = new ReadonlyProviderUnitFactory(ProviderSetup.UnitFactory.EntityRepositoryFactory);
                var readonlyProviderSetup = new ReadonlyProviderSetup(providerUnitFactory, ProviderSetup.ProviderMetadata, ProviderSetup.FrameworkContext, abstractProviderBootstrapper, 0);
                var providerMappingGroup = new ProviderMappingGroup("default", wildcardUriMatch, readonlyProviderSetup, ProviderSetup, ProviderSetup.FrameworkContext);
                _hiveManager = new HiveManager(providerMappingGroup, ProviderSetup.FrameworkContext);
                return _hiveManager;
            }
        }

        [TearDown]
        protected virtual void BaseTearDown()
        {
            if (_hiveManager != null)
            {
                _hiveManager.Dispose();
                _hiveManager = null;
            }
        }

        protected virtual TypedEntity CreateEntityForTest(Guid newGuid, Guid newGuidRedHerring, ProviderSetup providerSetup)
        {
            return HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, providerSetup);
        }

        protected virtual TypedEntity CreateContentForTest(Guid newGuid, Guid newGuidRedHerring, ProviderSetup providerSetup)
        {
            return HiveModelCreationHelper.SetupTestContentData(newGuid, newGuidRedHerring, providerSetup);
        }
    }
}