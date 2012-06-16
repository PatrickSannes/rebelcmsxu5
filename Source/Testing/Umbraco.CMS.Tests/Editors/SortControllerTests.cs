using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Web.Editors.Extenders;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs.PropertyEditors;

namespace Umbraco.Tests.Cms.Editors
{
    [TestFixture]
    public class SortControllerTests : AbstractContentControllerTest
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
        public void ContentEditorControllerTests_Sort_Invalidated()
        {
            //Arrange

            var parentEntity = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity1 = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity2 = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity3 = CreateEntityRevision(new RegexPropertyEditor());
            //setup the relations
            using (var writer = UmbracoApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                writer.Repositories.AddRelation(parentEntity.Item, subEntity1.Item, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(parentEntity.Item, subEntity2.Item, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(parentEntity.Item, subEntity3.Item, FixedRelationTypes.DefaultRelationType, 0);
                writer.Complete();
            }

            //var controller = new ContentEditorController(GetBackOfficeRequestContext());
            var controller = new SortController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), GetBackOfficeRequestContext());

            //Act

            //a model not containing any of the id's or sort indexes
            var result = controller.SortForm(new SortModel());
            var model = new BendyObject(result.Data);

            //Assert

            dynamic check = model.AsDynamic();

            Assert.AreEqual("ValidationError", check.failureType);
            Assert.AreEqual("false", check.success);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Sort_Success()
        {
            //Arrange

            var parentEntity = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity1 = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity2 = CreateEntityRevision(new RegexPropertyEditor());
            var subEntity3 = CreateEntityRevision(new RegexPropertyEditor());
            //setup the relations
            using (var writer = UmbracoApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                writer.Repositories.AddRelation(parentEntity.Item, subEntity1.Item, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(parentEntity.Item, subEntity2.Item, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(parentEntity.Item, subEntity3.Item, FixedRelationTypes.DefaultRelationType, 0);
                writer.Complete();
            }

            var controller = new SortController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), GetBackOfficeRequestContext(), false);

            //Act

            //a model not containing any of the id's or sort indexes
            var result = controller.SortForm(new SortModel
                {
                    ParentId = parentEntity.Item.Id,
                    Items = new[]
                        {
                            new SortItem {Id = subEntity1.Item.Id, SortIndex = 2},
                            new SortItem {Id = subEntity2.Item.Id, SortIndex = 1},
                            new SortItem {Id = subEntity3.Item.Id, SortIndex = 0},
                        }
                }) as CustomJsonResult;

            //Assert

            Assert.IsNotNull(result);

            var json = JObject.Parse(result.OutputJson());
            Assert.AreEqual(true, json["success"].Value<bool>());

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var children = uow.Repositories.GetChildRelations(parentEntity.Item, FixedRelationTypes.DefaultRelationType).ToArray();

                Assert.That(children.Length, Is.EqualTo(3));

                Assert.AreEqual(0, children.Where(x => x.DestinationId == subEntity3.Item.Id).Single().Ordinal);
                Assert.AreEqual(1, children.Where(x => x.DestinationId == subEntity2.Item.Id).Single().Ordinal);
                Assert.AreEqual(2, children.Where(x => x.DestinationId == subEntity1.Item.Id).Single().Ordinal);
            }
        }
    }
}