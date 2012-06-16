using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Editors.Extenders;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Security;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs.PropertyEditors;
using Umbraco.Hive;
namespace Umbraco.Tests.Cms.Editors
{


    [TestFixture]
    public class ContentEditorControllerTests : AbstractContentControllerTest
    {
        [TestFixtureSetUp]
        public static void TestSetup()
        {
            TestHelper.SetupLog4NetForTests();
        }
        
        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Create_New_Wizard_Step_Bound_And_Validated()
        {
            //Arrange

            var selectedDocTypeId = new HiveId("content", "", new HiveIdValue(Guid.NewGuid()));
            var createModel = new CreateContentModel { Name = "test", SelectedDocumentTypeId = selectedDocTypeId };
            // Get the parent content schema
            using (var writer = UmbracoApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                var contentSchemaRoot = writer.Repositories.Schemas.Get<EntitySchema>(FixedHiveIds.ContentRootSchema);
                //create doc type in persistence layer
                var schema = HiveModelCreationHelper.CreateEntitySchema("test", "Test", new AttributeDefinition[] { });
                schema.Id = selectedDocTypeId;
                schema.RelationProxies.EnlistParent(contentSchemaRoot, FixedRelationTypes.DefaultRelationType);
                writer.Repositories.Schemas.AddOrUpdate(schema);
                writer.Complete();
            }
            
            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  { "Name", "test" },
                                                  { "SelectedDocumentTypeId", selectedDocTypeId.ToString() }
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.CreateNewForm(createModel);
            var model = (CreateContentModel)result.Model;

            //Assert

            Assert.IsTrue(controller.ModelState.IsValidField("Name"),
                string.Join("; ", controller.ModelState["Name"].Errors.Select(x => x.ErrorMessage)));
            Assert.IsTrue(controller.ModelState.IsValidField("SelectedDocumentTypeId"), 
                string.Join("; ", controller.ModelState["SelectedDocumentTypeId"].Errors.Select(x => x.ErrorMessage)));

            Assert.AreEqual("test", model.Name);
            Assert.AreEqual((Guid)selectedDocTypeId.Value, (Guid)model.SelectedDocumentTypeId.Value);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Create_New_Wizard_Step_Bound_And_Invalidated()
        {
            //Arrange

            var selectedDocTypeId = Guid.NewGuid();
            var createModel = new CreateContentModel { Name = "", SelectedDocumentTypeId = new HiveId(selectedDocTypeId) };

            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  { "Name", "" },
                                                  { "SelectedDocumentTypeId", selectedDocTypeId.ToString("N") }
                                              }, GetBackOfficeRequestContext());

            //Act
            var result = (ViewResult)controller.CreateNewForm(createModel);

            //Assert
            Assert.IsFalse(controller.ModelState.IsValidField("Name"));
            Assert.IsFalse(controller.ModelState.IsValidField("SelectedDocumentTypeId"));

        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Mandatory_Property_Bound_And_Validated()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new MandatoryPropertyEditor());
            var customAttribute = contentEntity.Item.Attributes.Last();
            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {            
                                                  { customAttribute.Id.GetHtmlId() + ".Value", "test"}
                                              }, GetBackOfficeRequestContext());


            //Act

            var result = (ViewResult)controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);
            var model = (ContentEditorModel)result.Model;

            //Assert

            Assert.IsTrue(controller.ModelState.IsValidField(customAttribute.Id.GetHtmlId() + ".Value"));
            Assert.AreEqual("test", model.Properties.Single(x => x.Alias == customAttribute.AttributeDefinition.Alias).PropertyEditorModel.Value);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Mandatory_Property_Bound_And_Invalidated()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new MandatoryPropertyEditor());
            var customAttribute = contentEntity.Item.Attributes.Last();
            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {            
                                                  { customAttribute.Id.GetHtmlId() + ".Value", ""}
                                              }, GetBackOfficeRequestContext());


            //Act

            var result = (ViewResult)controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);
            var model = (ContentEditorModel)result.Model;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField(customAttribute.Id.GetHtmlId() + ".Value"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Regex_Property_Bound_And_Validated()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new RegexPropertyEditor());
            var customAttribute = contentEntity.Item.Attributes.Last();
            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  { customAttribute.Id.GetHtmlId() + ".Value", "123"}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);
            var model = (ContentEditorModel)result.Model;

            //Assert

            Assert.IsTrue(controller.ModelState.IsValidField(customAttribute.Id.GetHtmlId() + ".Value"));
            Assert.AreEqual("123", model.Properties.Single(x => x.Alias == customAttribute.AttributeDefinition.Alias).PropertyEditorModel.Value);
        }

        

     

       

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Regex_Property_Bound_And_Invalidated()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new RegexPropertyEditor());
            var customAttribute = contentEntity.Item.Attributes.Last();
            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  //there will be 2 attributes, one for node name and a custom one
                                                  { customAttribute.Id.GetHtmlId() + ".Value", "asd"}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);
            var model = (ContentEditorModel)result.Model;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField(customAttribute.Id.GetHtmlId() + ".Value"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Content_Saved()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new RegexPropertyEditor());

            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  { "Name", "Demo"},
                                                  { contentEntity.Item.Attributes[NodeNameAttributeDefinition.AliasValue].Id.GetHtmlId() + ".Name", "test"},
                                                  { contentEntity.Item.Attributes[NodeNameAttributeDefinition.AliasValue].Id.GetHtmlId() + ".UrlName", "test"},
                                                  { contentEntity.Item.Attributes["bodyText"].Id.GetHtmlId() + ".Value", "1234"},
                                                  { contentEntity.Item.Attributes["siteName"].Id.GetHtmlId() + ".Value", "4321"},                                                  
                                                  { "submit.Save", "Save"} //set save flag
                                              }, GetBackOfficeRequestContext(), false);

            //Act

            var result = controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var snapshot = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(contentEntity.Item.Id);
                if (snapshot == null)
                    Assert.Fail("no snapshot found");

                Assert.AreNotEqual(contentEntity.MetaData.Id, snapshot.Revision.MetaData.Id);
                Assert.IsTrue(contentEntity.MetaData.UtcCreated < snapshot.Revision.MetaData.UtcCreated);
                var contentViewModel = UmbracoApplicationContext.FrameworkContext.TypeMappers.Map<EntitySnapshot<TypedEntity>, ContentEditorModel>(snapshot);
                Assert.AreEqual(null, contentViewModel.UtcPublishedDate);
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Content_Unpublished()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new RegexPropertyEditor(),
                c =>
                {
                    c.MetaData.UtcStatusChanged = DateTime.Now;
                    c.MetaData.StatusType = FixedStatusTypes.Published;
                });

            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  { "Name", "hello"},
                                                  { "submit.Unpublish", "Unpublish"} //set unpublish flag
                                              }, GetBackOfficeRequestContext(), false);

            //Act

            var result = controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var snapshot = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(contentEntity.Item.Id);
                if (snapshot == null)
                    Assert.Fail("no snapshot found");

                var contentViewModel = UmbracoApplicationContext.FrameworkContext.TypeMappers.Map<EntitySnapshot<TypedEntity>, ContentEditorModel>(snapshot);
                Assert.AreEqual(null, contentViewModel.UtcPublishedDate);
                var lastUnpublished = snapshot.GetLatestDate(FixedStatusTypes.Unpublished);
                Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(lastUnpublished) < new TimeSpan(0, 1, 0));
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Content_Published()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new RegexPropertyEditor(),
                c =>
                {
                    c.MetaData.UtcStatusChanged = DateTime.Now;
                    c.MetaData.StatusType = FixedStatusTypes.Draft;
                });

            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  { "Name", "hello"},
                                                  { "submit.Publish", "Publish"} //set Publish flag
                                              }, GetBackOfficeRequestContext(), false);

            //Act

            var result = controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var snapshot = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(contentEntity.Item.Id);
                if (snapshot == null)
                    Assert.Fail("no snapshot found");

                var contentViewModel = UmbracoApplicationContext.FrameworkContext.TypeMappers.Map<EntitySnapshot<TypedEntity>, ContentEditorModel>(snapshot);
                Assert.IsNotNull(contentViewModel.UtcPublishedDate);
                Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(contentViewModel.UtcPublishedDate.Value) < new TimeSpan(0, 1, 0));
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTests_Invalid_Model_State_When_Missing_Required_Values()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new RegexPropertyEditor());

            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  { "Name", ""}
                                              }, GetBackOfficeRequestContext());

            //Act
            var result = (ViewResult)controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);
            var model = (ContentEditorModel)result.Model;

            //Assert
            Assert.IsFalse(controller.ModelState.IsValidField("Name"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTest_All_Standard_Values_Bound()
        {
            //Arrange

            var contentEntity = CreateEntityRevision(new RegexPropertyEditor());

            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  { "Name", "Demo"},
                                                  { "SelectedTemplateId", "1042" }, //i know that 1042 is in our mocked template resolver
                                                  { "UtcPublishScheduled", "2013-01-01" },
                                                  { "UtcUnpublishScheduled", "2014-01-01" }
                                              }, GetBackOfficeRequestContext());

            //Act
            var result = (ViewResult)controller.EditForm(contentEntity.Item.Id, contentEntity.MetaData.Id);
            var model = (ContentEditorModel)result.Model;

            //Assert
            Assert.AreEqual("Demo", model.Name);
            //Assert.AreEqual(new HiveId(1042), model.SelectedTemplateId);
            Assert.AreEqual(new DateTime(2013, 1, 1, 0, 0, 0), model.UtcPublishScheduled);
            Assert.AreEqual(new DateTime(2014, 1, 1, 0, 0, 0), model.UtcUnpublishScheduled);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void ContentEditorControllerTest_Actions_Secured_By_Permissions()
        {
            //Arrange
            var backOfficeRequestContext = GetBackOfficeRequestContext();
            var controller = new ContentEditorController(backOfficeRequestContext);
            controller.InjectDependencies(GetBackOfficeRequestContext());

            //TODO: There may be a better way of automatically getting a single controller variable using the controller extenders,
            //but likely that's just testing the action invoker, so for now here's the individual controllers

            var copyController = new MoveCopyController(backOfficeRequestContext);
            copyController.InjectDependencies(GetBackOfficeRequestContext());

            var sortController = new SortController(backOfficeRequestContext);
            sortController.InjectDependencies(GetBackOfficeRequestContext());

            var publishController = new PublishController(backOfficeRequestContext);
            publishController.InjectDependencies(GetBackOfficeRequestContext());

            var hos = new HostnameController(backOfficeRequestContext);
            hos.InjectDependencies(GetBackOfficeRequestContext());

            var rollback = new RollbackController(backOfficeRequestContext);
            rollback.InjectDependencies(GetBackOfficeRequestContext());

            var permissions = new PermissionsController(backOfficeRequestContext);
            permissions.InjectDependencies(GetBackOfficeRequestContext());

            //Assert
            Assert.IsTrue(ActionIsSecuredByPermission(controller, "CreateNew", FixedPermissionIds.Create));
            Assert.IsTrue(ActionIsSecuredByPermission(controller, "Edit", FixedPermissionIds.Update));
            Assert.IsTrue(ActionIsSecuredByPermission(controller, "Delete", FixedPermissionIds.Delete));
            Assert.IsTrue(ActionIsSecuredByPermission(copyController, "Copy", FixedPermissionIds.Copy));
            Assert.IsTrue(ActionIsSecuredByPermission(copyController, "Move", FixedPermissionIds.Move));
            Assert.IsTrue(ActionIsSecuredByPermission(sortController, "Sort", FixedPermissionIds.Sort));
            Assert.IsTrue(ActionIsSecuredByPermission(publishController, "Publish", FixedPermissionIds.Publish));
            Assert.IsTrue(ActionIsSecuredByPermission(permissions, "Permissions", FixedPermissionIds.Permissions));
            Assert.IsTrue(ActionIsSecuredByPermission(rollback, "Rollback", FixedPermissionIds.Rollback));

            // TODO: (APN @ Matt) the assertion for action name doesn't take into account two methods with the same name
            // but differing parameter counts, so this one fails
            // NOTE: (MPB) Have renamed post action to HostnameForm to get test passing for now, not sure if that is enough
            // or whether assertion method should allow you to query for a specific method signature?
            Assert.IsTrue(ActionIsSecuredByPermission(hos, "Hostname", FixedPermissionIds.Hostnames));
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

        #region Private methods
        
       

        private bool ActionIsSecuredByPermission(Controller controller, string actionName, string permissionId)
        {
            var methodInfo = controller.GetType().GetMethod(actionName);
            if (methodInfo == null)
                throw new InvalidOperationException("Action named '" + actionName + "' not found.");

            var attributes = methodInfo.GetCustomAttributes(typeof(UmbracoAuthorizeAttribute), true);
            if (attributes.Length == 0)
                return false;

            return attributes.Cast<UmbracoAuthorizeAttribute>().Any(x => x.Permissions.Length > 0 && x.Permissions.Contains(permissionId));
        }

        #endregion
    }
}
