using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.Configuration.Languages;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Dictionary;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Testing;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.Providers.IO;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Framework.Persistence;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class DictionaryHelperTests
    {
        private IHiveManager _hiveManager;
        private IEnumerable<LanguageElement> _languages;

        [Test]
        public void GetDictionaryItem_Returns_Found_Result()
        {
            var dictionaryHelper = new DictionaryHelper(_hiveManager, _languages);

            Assert.IsTrue(dictionaryHelper.GetDictionaryItem("test1", "en-GB").Found);
        }

        [Test]
        public void GetDictionaryItemValue_Returns_Defailt_Value_If_Item_UnPublished()
        {
            var dictionaryHelper = new DictionaryHelper(_hiveManager, _languages);

            Assert.AreEqual("[test2/test4]", dictionaryHelper.GetDictionaryItemValue("test2/test4", null));
        }

        [SetUp]
        public void Setup()
        {
            // Setup languages
            _languages = new List<LanguageElement>
            {
                new LanguageElement
                {
                    IsoCode = "en-GB",
                    Name = "English (United Kingdom)",
                    Fallbacks = new FallbackCollection
                    {
                        new FallbackElement { IsoCode = "en-US" }
                    }
                },
                new LanguageElement
                {
                    IsoCode = "en-US",
                    Name = "English (United States)",
                    Fallbacks = new FallbackCollection()
                }
            };

            // Setup hive
            var context = new FakeFrameworkContext();
            _hiveManager = FakeHiveCmsManager.NewWithNhibernate(new[] { CreateFakeDictionaryMappingGroup(context) }, context);

            var root = new TypedEntity
            {
                Id = FixedHiveIds.DictionaryVirtualRoot,
                EntitySchema = FixedSchemas.DictionaryRootSchema,
                UtcCreated = DateTime.Now,
                UtcModified = DateTime.Now,
                UtcStatusChanged = DateTime.Now
            };

            var item1 = CreateDictionatyItem("test1", new Dictionary<string, string> { { "en-GB", "Hello GB" }, { "en-US", "Hello US" } });
            var item2 = CreateDictionatyItem("test2", new Dictionary<string, string> { { "en-GB", "World GB" }, { "en-US", "World US" } });
            var item3 = CreateDictionatyItem("test3", new Dictionary<string, string> { { "en-GB", "Something GB" }, { "en-US", "Something US" } });

            // Act
            var writer = _hiveManager.GetWriter<IDictionaryStore>();
            using (var uow = writer.Create<IDictionaryStore>())
            {
                // Add entities
                uow.Repositories.AddOrUpdate(root);
                uow.Repositories.Revisions.AddOrUpdate(item1);
                uow.Repositories.Revisions.AddOrUpdate(item2);
                uow.Repositories.Revisions.AddOrUpdate(item3);

                // Add all relations
                uow.Repositories.AddRelation(FixedHiveIds.DictionaryVirtualRoot, item1.Item.Id, FixedRelationTypes.DefaultRelationType, 0);
                uow.Repositories.AddRelation(FixedHiveIds.DictionaryVirtualRoot, item2.Item.Id, FixedRelationTypes.DefaultRelationType, 0);
                uow.Repositories.AddRelation(item2.Item.Id, item3.Item.Id, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();
            }
        }

        #region Helper Methods

        private ProviderMappingGroup CreateFakeDictionaryMappingGroup(FakeFrameworkContext frameworkContext)
        {
            var helper = new NhibernateTestSetupHelper(frameworkContext);
            var uriMatch = new WildcardUriMatch(new Uri("dictionary://"));
            var persistenceMappingGroup = new ProviderMappingGroup(
                "dictionary",
                uriMatch,
                helper.ReadonlyProviderSetup,
                helper.ProviderSetup,
                frameworkContext);

            return persistenceMappingGroup;
        }

        private Revision<TypedEntity> CreateDictionatyItem(string key, Dictionary<string, string> translations)
        {
            var schema = FixedSchemas.DictionaryItemSchema;
            var nameAttributeDefinition = schema.AttributeDefinitions.SingleOrDefault(x => x.Alias == NodeNameAttributeDefinition.AliasValue);
            var translationAttributeDefinition = schema.AttributeDefinitions.SingleOrDefault(x => x.Alias == DictionaryItemSchema.TranslationsAlias);

            var entity = new TypedEntity
            {
                Id = new HiveId(new Uri("dictionary://"), null, new HiveIdValue(Guid.NewGuid())),
                EntitySchema = FixedSchemas.DictionaryItemSchema,
                UtcCreated = DateTime.Now,
                UtcModified = DateTime.Now,
                UtcStatusChanged = DateTime.Now
            };

            var nameAttribute = new TypedAttribute(nameAttributeDefinition);
            nameAttribute.Values.Add("UrlName", key);
            nameAttribute.Values.Add("Name", key);
            entity.Attributes.Add(nameAttribute);

            var translationAttribute = new TypedAttribute(translationAttributeDefinition);

            foreach (var translation in translations)
                translationAttribute.Values.Add(translation.Key, translation.Value);

            entity.Attributes.Add(translationAttribute);

            var revision = new Revision<TypedEntity>(entity) { MetaData = { StatusType = FixedStatusTypes.Published } };
            return revision;
        }
        
        #endregion
    }
}
