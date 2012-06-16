using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Hive.Configuration;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    using NUnit.Framework;
    using Umbraco.Framework;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
    using Umbraco.Hive;
    using Umbraco.Hive.RepositoryTypes;

    [TestFixture]
    public class HighLevelApiFixture
    {
        protected IHiveManager Hive;
        private NhibernateTestSetupHelper _nhibernateTestSetup;

        [TestFixtureSetUp]
        public void Setup()
        {
            _nhibernateTestSetup = new NhibernateTestSetupHelper();

            Hive =
                new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                _nhibernateTestSetup.ReadonlyProviderSetup,
                                _nhibernateTestSetup.ProviderSetup,
                                _nhibernateTestSetup.FakeFrameworkContext)
                        },
                    _nhibernateTestSetup.FakeFrameworkContext);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            ClearCaches();
            _nhibernateTestSetup.Dispose();
            Hive.Dispose();
        }

        public void ClearCaches()
        {
            _nhibernateTestSetup.SessionForTest.Clear();
            Hive.Context.GenerationScopedCache.IfNotNull(x => x.Clear());
        }

        [Test]
        public void CreateNormalSchema()
        {
            var schema = Hive.CreateSchema<EntitySchema, IContentStore>("mySchema");
            Assert.NotNull(schema);
        }

        [Test]
        public void SchemaPartBuilder_CanCreateAttributeTypes_UsingCurrentRegistry()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());
            var innerBuilder = new BuilderStarter<AttributeType, IContentStore>(Hive);
            var type = innerBuilder.UseExistingType("richTextEditor");
            Assert.NotNull(type);
            Assert.That(type.Item, Is.Not.Null);
        }

        [Test]
        public void SchemaPartBuilder_CanCreateAttributeTypes_UsingSpecificRegistry()
        {
            var registry = new CmsAttributeTypeRegistry();
            string key = "richTextEditor";
            var theRealType = registry.GetAttributeType(key);
            var innerBuilder = new BuilderStarter<AttributeType, IContentStore>(Hive);
            var type = innerBuilder.UseExistingType(registry, key);
            Assert.NotNull(type);
            Assert.That(type.Item, Is.Not.Null);
            Assert.That(type.Item.Alias, Is.EqualTo(theRealType.Alias));
            Assert.That(type.Item.Id, Is.EqualTo(theRealType.Id));
        }

        [Test]
        public void SchemaBuilder_CanCreateEntitySchema_WithLongTypeAndGroupDefinition()
        {
            var schema = Hive
                .NewSchema<EntitySchema, IContentStore>("mySchema")
                .Define("title", new AttributeType("textbox", "Text box", "who cares", new StringSerializationType()), new AttributeGroup("tab1", "tab1", 0))
                .Commit();

            if (schema.Errors.Any())
            {
                Assert.Fail(schema.Errors.FirstOrDefault().ToString());
            }

            Assert.True(schema.Success);
            Assert.NotNull(schema.Item);

            ClearCaches();
            var schemaReloaded = AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, schema.Item.Id);
            Assert.That(schemaReloaded.AttributeDefinitions.Any());
            Assert.That(schemaReloaded.AttributeDefinitions[0].Alias, Is.EqualTo("title"));
        }



        [Test]
        public void SchemaBuilder_CanCreateEntitySchema_WithAttributeTypeAndGroupSubBuilders()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            var schema = Hive
                .NewSchema<EntitySchema, IContentStore>("mySchema")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("bodyText", type => type.UseExistingType("richTextEditor"), FixedGroupDefinitions.GeneralGroup)
                .Define("extraBodyText", type => type.UseExistingType("richTextEditor"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            if (schema.Errors.Any())
            {
                Assert.Fail(schema.Errors.FirstOrDefault().ToString());
            }

            Assert.True(schema.Success);
            Assert.NotNull(schema.Item);

            ClearCaches();
            var schemaReloaded = AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, schema.Item.Id);
            Assert.That(schemaReloaded.AttributeDefinitions.Any());
            Assert.That(schemaReloaded.AttributeDefinitions.Count(), Is.EqualTo(3));
            Assert.That(schemaReloaded.AttributeDefinitions[0].Alias, Is.EqualTo("title"));
            Assert.NotNull(schemaReloaded.AttributeDefinitions["title"]);
            Assert.NotNull(schemaReloaded.AttributeDefinitions[0].AttributeType);
            Assert.That(schemaReloaded.AttributeDefinitions[0].AttributeType.Alias, Is.EqualTo("singleLineTextBox"));
            Assert.That(schemaReloaded.AttributeDefinitions[1].AttributeType.Alias, Is.EqualTo("richTextEditor"));
            Assert.That(schemaReloaded.AttributeDefinitions[2].AttributeType.Alias, Is.EqualTo("richTextEditor"));
        }

        public static T AssertSchemaPartExists<T, TProviderFilter>(IHiveManager hiveManager, HiveId id)
            where TProviderFilter : class, IProviderTypeFilter
            where T : AbstractSchemaPart
        {
            using (var uow = hiveManager.OpenReader<TProviderFilter>())
            {
                var item = uow.Repositories.Schemas.Get<T>(id);
                Assert.NotNull(item);
                return item;
            }
        }
    }

    [TestFixture]
    public class HighLevelApiCmsExtensionsFixture
    {
        protected IHiveManager Hive;
        private NhibernateTestSetupHelper _nhibernateTestSetup;

        [TestFixtureSetUp]
        public void Setup()
        {
            _nhibernateTestSetup = new NhibernateTestSetupHelper();

            Hive = new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                _nhibernateTestSetup.ReadonlyProviderSetup,
                                _nhibernateTestSetup.ProviderSetup,
                                _nhibernateTestSetup.FakeFrameworkContext)
                        },
                    _nhibernateTestSetup.FakeFrameworkContext);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            ClearCaches();
            _nhibernateTestSetup.Dispose();
            Hive.Dispose();
        }

        public void ClearCaches()
        {
            _nhibernateTestSetup.SessionForTest.Clear();
            Hive.Context.GenerationScopedCache.IfNotNull(x => x.Clear());
        }


        [Test]
        public void CreateContentType()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var doctype = Hive.Cms().NewContentType<EntitySchema, IContentStore>("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            if (doctype.Errors.Any())
            {
                Assert.Fail(doctype.Errors.FirstOrDefault().ToString());
            }

            Assert.True(doctype.Success);
            Assert.NotNull(doctype.Item);

            ClearCaches();
            var schemaReloaded = HighLevelApiFixture.AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, doctype.Item.Id);
        }
    }
}
