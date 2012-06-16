using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Packages.DevDataset;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Cms.Web.System;
using Umbraco.Cms.Web.Tasks;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.NHibernate;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Examine;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.CoreAndFramework.Cms
{
    [TestFixture]
    public class DataInstallTests //: AbstractPartialTrustFixture<DataInstallTests>
    {
        [TestFixtureSetUp]
        public void SetupLog4net()
        {
            TestHelper.SetupLog4NetForTests();
        }

        [Test]
        public void DataInstallTest_CoreData_Examine()
        {
            //Arrange

            var examineTestSetup = new ExamineTestSetupHelper();

            var storageProvider = new IoHiveTestSetupHelper(examineTestSetup.FrameworkContext);

            var hiveManager =
                new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                examineTestSetup.ReadonlyProviderSetup,
                                examineTestSetup.ProviderSetup,
                                examineTestSetup.FrameworkContext),
                            storageProvider.CreateGroup("uploader", "storage://file-uploader")

                        },
                    examineTestSetup.FrameworkContext);

            RunTest(hiveManager, examineTestSetup.FrameworkContext, () =>
            {
                hiveManager.Context.GenerationScopedCache.Clear();
            });

            hiveManager.Dispose();
        }

        [Test]
        public void DataInstallTest_CoreData_NHibernate()
        {
            //Arrange

            var nhibernateTestSetup = new NhibernateTestSetupHelper();

            var storageProvider = new IoHiveTestSetupHelper(nhibernateTestSetup.FakeFrameworkContext);

            var hiveManager =
                new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                nhibernateTestSetup.ReadonlyProviderSetup,
                                nhibernateTestSetup.ProviderSetup,
                                nhibernateTestSetup.FakeFrameworkContext),
                            storageProvider.CreateGroup("uploader", "storage://file-uploader")
                        },
                    nhibernateTestSetup.FakeFrameworkContext);

            RunTest(hiveManager, nhibernateTestSetup.FakeFrameworkContext, () =>
                {
                    nhibernateTestSetup.SessionForTest.Clear();
                    hiveManager.Context.GenerationScopedCache.Clear();
                });

            hiveManager.Dispose();
        }

        private void RunTest(
            HiveManager hiveManager,
            FakeFrameworkContext frameworkContext,
            Action installCallback = null)
        {
            var attributeTypeRegistry = new CmsAttributeTypeRegistry();
            AttributeTypeRegistry.SetCurrent(attributeTypeRegistry);
            var appContext = new FakeUmbracoApplicationContext(hiveManager, false);
            var mockedPropertyEditorFactory = new MockedPropertyEditorFactory(appContext);
            var resolverContext = new MockedMapResolverContext(frameworkContext, hiveManager, mockedPropertyEditorFactory, new MockedParameterEditorFactory());
            var webmModelMapper = new CmsModelMapper(resolverContext);
            frameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { webmModelMapper, new FrameworkModelMapper(frameworkContext) }));

            var devDataset = DemoDataHelper.GetDemoData(appContext, attributeTypeRegistry);

            //Seup permissions
            var permissions = new Permission[] { new SavePermission(), new PublishPermission(), new HostnamesPermission(), new CopyPermission(), new MovePermission() }
                .Select(x => new Lazy<Permission, PermissionMetadata>(() => x, new PermissionMetadata(new Dictionary<string, object>
                    {
                        {"Id", x.Id},
                        {"Name", x.Name},
                        {"Type", x.Type}
                    }))).ToArray();

            var coreDataInstallTask = new EnsureCoreDataTask(frameworkContext, hiveManager, permissions);
            var devDatasetInstallTask = new DevDatasetInstallTask(frameworkContext, mockedPropertyEditorFactory, hiveManager, attributeTypeRegistry);

            //Act

            coreDataInstallTask.InstallOrUpgrade();
            if (installCallback != null) installCallback();
            devDatasetInstallTask.InstallOrUpgrade();
            if (installCallback != null) installCallback();

            //Assert

            var totalSchemaCount = CoreCmsData.RequiredCoreSchemas().Count() + devDataset.DocTypes.Count() + 1; // +1 for SystemRoot schema
            var totalEntityCount =
                CoreCmsData.RequiredCoreUserGroups(permissions).Count() +
                CoreCmsData.RequiredCoreRootNodes().Count() +
                devDataset.ContentData.Count();
            var totalAttributeTypeCount = CoreCmsData.RequiredCoreSystemAttributeTypes().Count() + CoreCmsData.RequiredCoreUserAttributeTypes().Count();
            DoCoreAssertions(hiveManager, totalSchemaCount, totalEntityCount, totalAttributeTypeCount, 2, permissions);
        }

        private void DoCoreAssertions(IHiveManager hiveManager, int totalSchemaCount, int totalEntityCount, int totalAttributeTypeCount, int mediaSchemaCount, IEnumerable<Lazy<Permission, PermissionMetadata>> permissions)
        {
            //Assert

            using (var uow = hiveManager.OpenReader<IContentStore>())
            {
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedHiveIds.SystemRoot));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.ContentVirtualRoot.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.ContentRecycleBin.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.MediaVirtualRoot.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.MediaRecycleBin.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.UserGroupVirtualRoot.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.UserVirtualRoot.Id));

                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.ContentRootSchema.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.MediaRootSchema.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.User.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.UserGroup.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.MediaFolderSchema.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.MediaImageSchema.Id));




                var schemas = uow.Repositories.Schemas.GetAll<EntitySchema>().ToArray();
                //ensure that schemas have relations on them
                var mediaImage = schemas.Where(x => x.Alias == "mediaImage").Single();
                Assert.True(mediaImage.RelationProxies.IsConnected);
                Assert.AreEqual(FixedHiveIds.MediaRootSchema.Value, mediaImage.RelationProxies.Single().Item.SourceId.Value);

                var mediaSchemas = uow.Repositories.Schemas.GetEntityByRelationType<EntitySchema>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.MediaRootSchema);
                Assert.AreEqual(mediaSchemaCount, mediaSchemas.Count());


                //ensure that the built in attribute types are there and set correctly
                var attributeTypes = uow.Repositories.Schemas.GetAll<AttributeType>().ToArray();
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == StringAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == BoolAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == DateTimeAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == IntegerAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == TextAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == NodeNameAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == SelectedTemplateAttributeType.AliasValue));
                // TODO: Add other in built attribute types

                //now make sure that the render types are set
                var inbuiltString = attributeTypes.Single(x => x.Alias == StringAttributeType.AliasValue);
                Assert.AreEqual(CorePluginConstants.TextBoxPropertyEditorId, inbuiltString.RenderTypeProvider);
                var inbuiltText = attributeTypes.Single(x => x.Alias == TextAttributeType.AliasValue);
                Assert.AreEqual(CorePluginConstants.TextBoxPropertyEditorId, inbuiltText.RenderTypeProvider);
                var inbuiltDateTime = attributeTypes.Single(x => x.Alias == DateTimeAttributeType.AliasValue);
                Assert.AreEqual(CorePluginConstants.DateTimePickerPropertyEditorId, inbuiltDateTime.RenderTypeProvider);
                var inbuiltBool = attributeTypes.Single(x => x.Alias == BoolAttributeType.AliasValue);
                Assert.AreEqual(CorePluginConstants.TrueFalsePropertyEditorId, inbuiltBool.RenderTypeProvider);
                // TODO: Add other in built attribute types


                // Check totals
                Assert.AreEqual(totalSchemaCount, schemas.Count());
                var entities = uow.Repositories.GetAll<TypedEntity>().ToArray();
                Assert.AreEqual(totalEntityCount, entities.Count());

                //ensure they're all published
                foreach(var e in entities.Where(x => x.IsContent(uow) || x.IsMedia((uow))))
                {
                    var snapshot = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(e.Id);
                    Assert.AreEqual(FixedStatusTypes.Published.Alias, snapshot.Revision.MetaData.StatusType.Alias);
                }

                // Admin user is not longer created as part of the data install task
                //var adminUser = entities.SingleOrDefault(x => x.EntitySchema.Id.Value == FixedHiveIds.UserSchema.Value);
                //Assert.IsNotNull(adminUser);
                //Assert.IsTrue(adminUser.AttributeGroups.All(x => x != null));
                //Assert.IsTrue(adminUser.AttributeGroups.Any());
                //Assert.IsTrue(adminUser.EntitySchema.AttributeGroups.All(x => x != null));
                //Assert.IsTrue(adminUser.EntitySchema.AttributeGroups.Any());
                //Assert.IsTrue(adminUser.EntitySchema.AttributeDefinitions.Select(x => x.AttributeGroup).All(x => x != null));

                var distinctTypesByAlias = attributeTypes.DistinctBy(x => x.Alias).ToArray();
                var actualCount = attributeTypes.Count();
                var distinctCount = distinctTypesByAlias.Count();
                var actualCountById = attributeTypes.DistinctBy(x => x.Id).Count();
                var allWithAliasAndId = string.Join("\n", attributeTypes.OrderBy(x => x.Alias).Select(x => x.Alias + ": " + x.Id.Value));
                Assert.That(actualCount, Is.EqualTo(distinctCount),
                    "Duplicate AttributeTypes were created: {0} distinct by alias, {1} total loaded from provider, {2} distinct by id. All:{3}".InvariantFormat(distinctCount, actualCount, actualCountById, allWithAliasAndId));

                Assert.AreEqual(totalAttributeTypeCount, actualCount);

                //ensure the default templates are set
                var contentRoot = new Uri("content://");
                var homePage = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1048)));
                var templateRoot = new Uri("storage://templates");
                var templateProvider = "templates";

                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Homepage.cshtml")).ToString(),
                    homePage.Attributes[SelectedTemplateAttributeDefinition.AliasValue].DynamicValue.ToString());

                var installingModules = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1049)));
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Textpage.cshtml")).ToString(),
                    installingModules.Attributes[SelectedTemplateAttributeDefinition.AliasValue].DynamicValue.ToString());

                var faq = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1059)));
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Faq.cshtml")).ToString(),
                    faq.Attributes[SelectedTemplateAttributeDefinition.AliasValue].DynamicValue.ToString());

                //ensure the allowed templates are set
                Assert.AreEqual(1, homePage.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Count());
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Homepage.cshtml")).ToString(),
                    homePage.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Single().ToString());
                Assert.AreEqual(1, installingModules.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Count());
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Textpage.cshtml")).ToString(),
                    installingModules.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Single().ToString());
                Assert.AreEqual(1, faq.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Count());
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Faq.cshtml")).ToString(),
                    faq.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Single().ToString());

                //ensure the allowed children are set
                var allowedChildrenOfHomepage = homePage.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").ToArray();
                Assert.AreEqual(2, allowedChildrenOfHomepage.Count());

                // Check installing-modules is an allowed child of homepage
                Assert.That(allowedChildrenOfHomepage.Select(x => x.Value), Has.Some.EqualTo(installingModules.EntitySchema.Id.Value));

                var faqCat = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1060)));
                Assert.AreEqual(1, faq.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").Count());
                Assert.IsTrue(faqCat.EntitySchema.Id.EqualsIgnoringProviderId(faq.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").Single()));

                var faqQuestion = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1067)));
                Assert.AreEqual(1, faqCat.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").Count());
                Assert.IsTrue(faqQuestion.EntitySchema.Id.EqualsIgnoringProviderId(faqCat.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").Single()));

                var userGroups = uow.Repositories.GetAll<UserGroup>()
                    .Where(x => x.EntitySchema.Alias == UserGroupSchema.SchemaAlias);
                Assert.AreEqual(CoreCmsData.RequiredCoreUserGroups(permissions).Count(), userGroups.Count());
                var adminUserGroup = userGroups.First();
                Assert.AreEqual(1, adminUserGroup.RelationProxies.GetChildRelations(FixedRelationTypes.PermissionRelationType).Count());
                Assert.AreEqual(permissions.Count(),
                    adminUserGroup.RelationProxies.GetChildRelations(FixedRelationTypes.PermissionRelationType).Single().Item.MetaData.Count());

            }

            // Check same method coverage on GroupUnit<T>
            using (var uow = hiveManager.OpenWriter<IContentStore>())
            {
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedHiveIds.SystemRoot));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.ContentRootSchema.Id));
                var schemas = uow.Repositories.Schemas.GetAll<EntitySchema>().ToArray();
                Assert.AreEqual(totalSchemaCount, schemas.Count());
                var mediaImage = schemas.Where(x => x.Alias == "mediaImage").Single();
                Assert.True(mediaImage.RelationProxies.IsConnected);
                Assert.AreEqual(FixedHiveIds.MediaRootSchema.Value, mediaImage.RelationProxies.Single().Item.SourceId.Value);

                var entities = uow.Repositories.GetAll<TypedEntity>().ToArray();
                Assert.AreEqual(totalEntityCount, entities.Count());
                var attributeTypes = uow.Repositories.Schemas.GetAll<AttributeType>().ToArray();
                Assert.AreEqual(totalAttributeTypeCount, attributeTypes.Count());
            }
        }

        public void TestSetup()
        {
            return;
        }

        public void TestTearDown()
        {
            return;
        }
    }
}
