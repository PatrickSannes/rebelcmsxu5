using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Web.Editors.Extenders;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs.PropertyEditors;

namespace Umbraco.Tests.Cms.Editors
{
    [TestFixture]
    public class PublishControllerTests : AbstractContentControllerTest
    {
        [TestFixtureSetUp]
        public static void TestSetup()
        {
            TestHelper.SetupLog4NetForTests();
        }

        /// <summary>
        /// initialize all tests, this puts required data into Hive
        /// </summary>
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            AddRequiredDataToRepository();
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void Publish_Single()
        {
            var entity = CreateEntityRevision(new RegexPropertyEditor());

            var controller = new PublishController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), GetBackOfficeRequestContext(), false);

            var result = controller.PublishForm(new PublishModel
                {
                    Id = entity.Item.Id,
                    IncludeChildren = false,
                    IncludeUnpublishedChildren = false
                }) as CustomJsonResult;


            //Assert

            Assert.IsNotNull(result);
            var json = JObject.Parse(result.OutputJson());
            Assert.AreEqual(true, json["success"].Value<bool>());

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var publishedEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(entity.Item.Id);
                Assert.AreEqual(FixedStatusTypes.Published.Alias, publishedEntity.MetaData.StatusType.Alias);
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void Publish_All_Children_That_Are_Not_Already_Published()
        {
            var entity = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity1 = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity2 = CreateEntityRevision(new RegexPropertyEditor());

            using (var writer = UmbracoApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                var subRev1 = subEntity1.CopyToNewRevision();
                subRev1.Item.RelationProxies.EnlistParent(entity.Item, FixedRelationTypes.DefaultRelationType);
                var subRev2 = subEntity2.CopyToNewRevision();
                subRev2.Item.RelationProxies.EnlistParent(entity.Item, FixedRelationTypes.DefaultRelationType);
                writer.Repositories.Revisions.AddOrUpdate(subRev1);
                writer.Repositories.Revisions.AddOrUpdate(subRev2);
                writer.Complete();
            }

            var controller = new PublishController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), GetBackOfficeRequestContext(), false);

            var result = controller.PublishForm(new PublishModel
            {
                Id = entity.Item.Id,
                IncludeChildren = true,
                IncludeUnpublishedChildren = true
            }) as CustomJsonResult;


            //Assert

            Assert.IsNotNull(result);
            var json = JObject.Parse(result.OutputJson());
            Assert.AreEqual(true, json["success"].Value<bool>());

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var publishedEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(entity.Item.Id);
                Assert.AreEqual(FixedStatusTypes.Published.Alias, publishedEntity.MetaData.StatusType.Alias);
                
                var publishedSubEntity1 = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(subEntity1.Item.Id);
                Assert.AreEqual(FixedStatusTypes.Published.Alias, publishedSubEntity1.MetaData.StatusType.Alias);
                
                var publishedSubEntity2 = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(subEntity2.Item.Id);
                Assert.AreEqual(FixedStatusTypes.Published.Alias, publishedSubEntity2.MetaData.StatusType.Alias);
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void Publish_All_Children_That_Are_Already_Published()
        {
            var entity = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity1 = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity2 = CreateEntityRevision(new RegexPropertyEditor());

            using (var writer = UmbracoApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                var subRev1 = subEntity1.CopyToNewRevision(FixedStatusTypes.Published);
                subRev1.Item.RelationProxies.EnlistParent(entity.Item, FixedRelationTypes.DefaultRelationType);
                var subRev2 = subEntity2.CopyToNewRevision(FixedStatusTypes.Draft);
                subRev2.Item.RelationProxies.EnlistParent(entity.Item, FixedRelationTypes.DefaultRelationType);
                writer.Repositories.Revisions.AddOrUpdate(subRev1);
                writer.Repositories.Revisions.AddOrUpdate(subRev2);
                writer.Complete();
            }

            var controller = new PublishController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), GetBackOfficeRequestContext(), false);

            var result = controller.PublishForm(new PublishModel
            {
                Id = entity.Item.Id,
                IncludeChildren = true,
                IncludeUnpublishedChildren = false
            }) as CustomJsonResult;

            
            //Assert

            Assert.IsNotNull(result);
            var json = JObject.Parse(result.OutputJson());
            Assert.AreEqual(true, json["success"].Value<bool>());

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var publishedEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(entity.Item.Id);
                Assert.AreEqual(FixedStatusTypes.Published.Alias, publishedEntity.MetaData.StatusType.Alias);
                var publishedSubEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(subEntity1.Item.Id);
                Assert.AreEqual(FixedStatusTypes.Published.Alias, publishedSubEntity.MetaData.StatusType.Alias);
                Assert.Greater(publishedSubEntity.MetaData.UtcStatusChanged, subEntity1.MetaData.UtcStatusChanged);
                var unPublishedSubEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(subEntity2.Item.Id);
                Assert.AreEqual(FixedStatusTypes.Draft.Alias, unPublishedSubEntity.MetaData.StatusType.Alias);
            }
        }
    }
}
