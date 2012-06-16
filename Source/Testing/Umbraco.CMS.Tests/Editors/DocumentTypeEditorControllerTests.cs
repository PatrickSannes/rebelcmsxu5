using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.System;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Cms.Web.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs.PropertyEditors;

namespace Umbraco.Tests.Cms.Editors
{
    [TestFixture]
    public class DocumentTypeEditorControllerTests : AbstractContentControllerTest
    {
     
        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void DocumentTypeEditorControllerTests_DocumentType_Create()
        {
            //Arrange
            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), GetBackOfficeRequestContext(), false);

            //Act
            var model = new CreateDocumentTypeModel
                            {
                                Name = "Hello",
                                SelectedDocumentTypeId = FixedHiveIds.ContentRootSchema,
                                CreateTemplate = false
                            };

            var result = controller.CreateNewForm(model);

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);
            //get the new id from the route values
            var newId = ((RedirectToRouteResult)result).RouteValues["id"];
            Assert.AreEqual(typeof(HiveId), newId.GetType());

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var entity = uow.Repositories.Schemas.Get<EntitySchema>((HiveId)newId);
                if (entity == null)
                    Assert.Fail("no entity found");

                Assert.AreEqual("hello", entity.Alias);
                Assert.AreEqual("Hello", entity.Name.Value);
                Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(entity.UtcCreated) < new TimeSpan(0, 1, 0));
                Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(entity.UtcModified) < new TimeSpan(0, 1, 0));
                
                Assert.IsTrue(entity.AttributeGroups.Any(x => x.Alias == FixedGroupDefinitions.GeneralGroupAlias));
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void DocumentTypeEditorControllerTests_DocumentType_Saved()
        {
            //Arrange

            var schema = CreateNewSchema();
            var schema1 = CreateNewSchema(alias: "schema1");
            var schema2 = CreateNewSchema(alias: "schema2");
            
            var template1 = new HiveId(new Uri("storage://templates"), "templates", new HiveIdValue("home-page.cshtml"));
            var template2 = new HiveId(new Uri("storage://templates"), "templates", new HiveIdValue("faq-page.cshtml"));

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello"},
                                                  {"Alias", "hello"},
                                                  {"Icon", "myicon"},
                                                  {"Thumbnail", "mythumbnail"},
                                                  {"Description", "my description"},
                                                  {"DefaultTemplateId", template1.ToString()},
                                                  {"AllowedTemplates", string.Concat(template1.ToString(),",", template2.ToString()) },
                                                  {"AllowedChildren", string.Concat(schema1.Id.ToString(),",", schema2.Id.ToString())}
                                              }, GetBackOfficeRequestContext(), false);

            //Act

            var result = controller.EditForm(schema.Id);

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var entity = uow.Repositories.Schemas.Get<EntitySchema>(schema.Id);
                if (entity == null)
                    Assert.Fail("no entity found");

                Assert.AreEqual(schema.UtcCreated, entity.UtcCreated);
                Assert.IsTrue(schema.UtcModified < entity.UtcModified);
                Assert.AreEqual("hello", entity.Alias);
                Assert.AreEqual("Hello", entity.Name.Value);
                Assert.AreEqual("myicon", entity.GetXmlConfigProperty("icon"));
                Assert.AreEqual("mythumbnail", entity.GetXmlConfigProperty("thumb"));
                Assert.AreEqual("my description", entity.GetXmlConfigProperty("description"));
                Assert.AreEqual(template1.ToString(), entity.GetXmlConfigProperty("default-template"));
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void DocumentTypeEditorControllerTests_Create_New_Wizard_Step_Bound_And_Validated()
        {
            //Arrange

            var parentDocTypeId = Guid.NewGuid();
            var selectedDocTypeId = Guid.NewGuid();
            var createModel = new CreateDocumentTypeModel
                                  {
                                      Name = "test",
                                      //CreateTemplate = true,
                                      //ParentId = parentDocTypeId,
                                      SelectedDocumentTypeId = new HiveId(selectedDocTypeId)
                                  };

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  //{ "ParentId", parentDocTypeId.ToString("N") },
                                                  { "Name", "test" },
                                                  { "SelectedDocumentTypeId", selectedDocTypeId.ToString("N") },
                                                  { "CreateTemplate", true.ToString() }
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.CreateNewForm(createModel);
            var model = (CreateDocumentTypeModel)result.Model;

            //Assert

            Assert.IsTrue(controller.ModelState.IsValidField("Name"));
            Assert.IsTrue(controller.ModelState.IsValidField("ParentId"));
            Assert.IsTrue(controller.ModelState.IsValidField("SelectedDocumentTypeId"));
            Assert.IsTrue(controller.ModelState.IsValidField("CreateTemplate"));

            Assert.AreEqual("test", model.Name);
            //Assert.AreEqual(parentDocTypeId, model.ParentId);
            Assert.AreEqual(selectedDocTypeId, (Guid)model.SelectedDocumentTypeId.Value);
            //Assert.AreEqual(true, model.CreateTemplate);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void DocumentTypeEditorControllerTests_Create_New_Wizard_Step_Bound_And_Invalidated()
        {
            //Arrange

            var parentDocTypeId = Guid.NewGuid();
            var selectedDocTypeId = Guid.NewGuid();
            var createModel = new CreateDocumentTypeModel
                                  {
                                      Name = "",
                                      //CreateTemplate = true,
                                      //ParentId = parentDocTypeId,
                                      SelectedDocumentTypeId = new HiveId(selectedDocTypeId)
                                  };

            var controller = new ContentEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  //{ "ParentId", parentDocTypeId.ToString("N") },
                                                  { "Name", "" },
                                                  { "SelectedDocumentTypeId", selectedDocTypeId.ToString("N") },
                                                  { "CreateTemplate", true.ToString() }
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.CreateNewForm(createModel);

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField("Name"));

        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Tab_Definitions_Bound_And_Validated()
        {
            //Arrange

            var schema = CreateNewSchema();
            var grp1 = new AttributeGroup("testtab1", "Tab 1", 0);
            var grp2 = new AttributeGroup("testtab2", "Tab 2", 1);
            schema.AttributeGroups.Add(grp1);
            schema.AttributeGroups.Add(grp2);
            UmbracoApplicationContext.AddPersistenceData(schema);


            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {string.Format("{0}.Name", grp1.Id.GetHtmlId()), "new name 1"},
                                                  {string.Format("{0}.SortOrder", grp1.Id.GetHtmlId()), "100"},
                                                  {string.Format("{0}.Name", grp2.Id.GetHtmlId()), "new name 2"},
                                                  {string.Format("{0}.SortOrder", grp2.Id.GetHtmlId()), "200"},
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            var tab1 = model.DefinedTabs.Single(x => x.Id == grp1.Id);
            var tab2 = model.DefinedTabs.Single(x => x.Id == grp2.Id);
            Assert.AreEqual("new name 1", tab1.Name);
            Assert.AreEqual("new name 2", tab2.Name);
            Assert.AreEqual(100, tab1.SortOrder);
            Assert.AreEqual(200, tab2.SortOrder);
            Assert.IsTrue(controller.ModelState.IsValidField(string.Format("{0}.Name", grp1.Id.GetHtmlId())));
            Assert.IsTrue(controller.ModelState.IsValidField(string.Format("{0}.Name", grp2.Id.GetHtmlId())));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Tab_Definitions_Bound_And_Invalidated()
        {
            //Arrange

            var schema = CreateNewSchema();
            var grp1 = new AttributeGroup("testtab1", "Tab 1", 0);
            var grp2 = new AttributeGroup("testtab2", "Tab 2", 1);
            schema.AttributeGroups.Add(grp1);
            schema.AttributeGroups.Add(grp2);
            UmbracoApplicationContext.AddPersistenceData(schema);

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {string.Format("{0}.Name", grp1.Id.GetHtmlId()), ""},
                                                  {string.Format("{0}.SortOrder", grp1.Id.GetHtmlId()), "100"},
                                                  {string.Format("{0}.Name", grp2.Id.GetHtmlId()), ""},
                                                  {string.Format("{0}.SortOrder", grp2.Id.GetHtmlId()), "200"},
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            var tab1 = model.DefinedTabs.Single(x => x.Id == grp1.Id);
            var tab2 = model.DefinedTabs.Single(x => x.Id == grp2.Id);
            Assert.AreEqual(100, tab1.SortOrder);
            Assert.AreEqual(200, tab2.SortOrder);
            Assert.IsFalse(controller.ModelState.IsValidField(string.Format("{0}.Name", grp1.Id.GetHtmlId())));
            Assert.IsFalse(controller.ModelState.IsValidField(string.Format("{0}.Name", grp2.Id.GetHtmlId())));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Add_Tab_Button_Validates_Tab_Name()
        {
            //Arrange

            var schema = CreateNewSchema();
            UmbracoApplicationContext.AddPersistenceData(schema);

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"submit.Tab", "New Tab"},
                                                  {"NewTabName", "my new tab"}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsTrue(controller.ModelState.IsValidField("NewTabName"));
        }


        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Add_Tab_Button_With_Duplicate_Name_Invalidates()
        {

            //Arrange

            var schema = CreateNewSchema();
            schema.AttributeGroups.Add(new AttributeGroup("tab1", "Tab 1", 0) { Id = new HiveId(Guid.NewGuid()) });
            schema.AttributeGroups.Add(new AttributeGroup("tab2", "Tab 2", 1) { Id = new HiveId(Guid.NewGuid()) });
            UmbracoApplicationContext.AddPersistenceData(schema);

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"submit.Tab", "New Tab"},
                                                  {"NewTabName", "Tab 2"}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField("NewTabName"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Add_Tab_Button_Creates_Tab()
        {
            //Arrange

            var schema = CreateNewSchema();
            var tabCount = schema.AttributeGroups.Count();
            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello"},
                                                  {"Alias", "hello"},                                                  
                                                  {"submit.Tab", "New Tab"},
                                                  {"NewTabName", "my new tab"}
                                              }, GetBackOfficeRequestContext(), false);

            //Act

            var result = controller.EditForm(schema.Id);

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var entity = uow.Repositories.Schemas.Get<EntitySchema>(schema.Id);
                if (entity == null)
                    Assert.Fail("no entity found");

                Assert.AreEqual(tabCount + 1, entity.AttributeGroups.Count());
                Assert.IsTrue(entity.AttributeGroups.Any(x => x.Alias == "myNewTab"));
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_IsCreatingNewProperty_Flag_Creates_Property()
        {
            //Arrange

            var schema = CreateNewSchema();
            var propertyCount = schema.AttributeDefinitions.Count();
            var tab = schema.AttributeGroups.Single(x => x.Alias == "tab-1");
            var maxSortOrder = schema.AttributeDefinitions.Where(x => x.AttributeGroup.Id == tab.Id).Max(x => x.Ordinal);
            var attributeType = schema.AttributeTypes.Single(x => x.Alias == "test");
            
            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello"},
                                                  {"Alias", "hello"},
                                                  {"IsCreatingNewProperty", "true"},
                                                  {"NewProperty.Description", "a new property"},
                                                  {"NewProperty.Name", "Test"},
                                                  {"NewProperty.Alias", "test"},
                                                  {"NewProperty.TabId", tab.Id.ToString()},
                                                  {"NewProperty.DataTypeId", attributeType.Id.ToString()}
                                              }, GetBackOfficeRequestContext(), false);

            //Act

            var result = controller.EditForm(schema.Id);

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                
                var entity = uow.Repositories.Schemas.Get<EntitySchema>(schema.Id);
                var newProp = entity.AttributeDefinitions.Single(x => x.Alias == "test");                
                Assert.AreEqual(propertyCount + 1, entity.AttributeDefinitions.Count());
                Assert.AreEqual(maxSortOrder + 1, newProp.Ordinal);
            }
        }

        /// <summary>
        /// When the add new tab button is clicked, the new tab name property is validated
        /// </summary>
        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Add_Tab_Button_Invalidates_Empty_Tab_Name()
        {
            //Arrange

            var schema = CreateNewSchema();
            UmbracoApplicationContext.AddPersistenceData(schema);

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"submit.Tab", "New Tab"},
                                                  {"NewTabName", ""}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField("NewTabName"));
        }

        /// <summary>
        /// When the IsCreatingNewProperty flag is set to true on the model, then the NewProperty property will be validated and when they 
        /// are empty, there will be validation errors.
        /// </summary>
        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_IsCreatingNewProperty_Flag_Validates_NewProperty_Values()
        {
            //Arrange

            var schema = CreateNewSchema();
            UmbracoApplicationContext.AddPersistenceData(schema);
            var attributeType = schema.AttributeTypes.Single(x => x.Alias == "test");

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello"},
                                                  {"IsCreatingNewProperty", "true"},
                                                  {"NewProperty.Name", "Test"},
                                                  {"NewProperty.Alias", "test"},
                                                  {"NewProperty.DataTypeId", attributeType.Id.ToString()},
                                                  {"NewProperty.SortOrder", "1"}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsTrue(controller.ModelState.IsValidField("NewProperty.Name"));
            Assert.IsTrue(controller.ModelState.IsValidField("NewProperty.Alias"));
            Assert.IsTrue(controller.ModelState.IsValidField("NewProperty.DataTypeId"));
            Assert.IsTrue(controller.ModelState.IsValidField("NewProperty.SortOrder"));
        }

        /// <summary>
        /// When the IsCreatingNewProperty flag is set to true on the model, then the NewProperty property will be validated and when they 
        /// are empty, there will be validation errors.
        /// </summary>
        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_IsCreatingNewProperty_Flag_Invalidates_Empty_NewProperty_Values()
        {
            //Arrange

            var schema = CreateNewSchema();
            UmbracoApplicationContext.AddPersistenceData(schema);

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello"},
                                                  {"IsCreatingNewProperty", "true"},
                                                  {"NewProperty.Name", ""},
                                                  {"NewProperty.Alias", ""},
                                                  {"NewProperty.DataTypeId", ""},
                                                  {"NewProperty.SortOrder", ""}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField("NewProperty.Name"));
            Assert.IsFalse(controller.ModelState.IsValidField("NewProperty.Alias"));
            Assert.IsFalse(controller.ModelState.IsValidField("NewProperty.DataTypeId"));
            Assert.IsFalse(controller.ModelState.IsValidField("NewProperty.SortOrder"));
        }

        /// <summary>
        /// Because of the nature of checkboxes, they won't post a value unless checked, if all are unchecked then
        /// not value at all is posted, we need to make sure in this scenario that it will still model bind
        /// </summary>
        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Ensure_Allowed_Templates_Are_Nulled_When_Posting_Empty_List()
        {
            //Arrange

            var schema = CreateNewSchema();
            schema.SetXmlConfigProperty("allowed-templates", new[] { new HiveId("asdfasdf.cshtml").ToString() });
            UmbracoApplicationContext.AddPersistenceData(schema);

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsFalse(model.AllowedTemplates.Any(x => x.Selected)); //all items should be not selected
        }

        /// <summary>
        /// Because of the nature of checkboxes, they won't post a value unless checked, if all are unchecked then
        /// not value at all is posted, we need to make sure in this scenario that it will still model bind
        /// </summary>
        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Ensure_Allowed_Children_Are_Nulled_When_Posting_Empty_List()
        {
            //Arrange

            var schema = CreateNewSchema();
            schema.SetXmlConfigProperty("allowed-children", new[] { new HiveId(Guid.NewGuid()).ToString() });
            UmbracoApplicationContext.AddPersistenceData(schema);

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsFalse(model.AllowedChildren.Any(x => x.Selected)); //all items should be not selected
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Invalid_Model_State_When_Missing_Required_Values()
        {
            //Arrange

            var schema = CreateNewSchema();
            UmbracoApplicationContext.AddPersistenceData(schema);

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", ""},
                                                  {"Alias", ""},
                                                  {"Icon", ""},
                                                  {"Thumbnail", ""}                                                  
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.IsFalse(controller.ModelState.IsValidField("Name"));
            Assert.IsFalse(controller.ModelState.IsValidField("Alias"));
            Assert.IsFalse(controller.ModelState.IsValidField("Icon"));
            Assert.IsFalse(controller.ModelState.IsValidField("Thumbnail"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_All_Standard_Values_Bound()
        {
          
            var schema = CreateNewSchema();
            var schema1 = CreateNewSchema(alias:"schema1");
            var schema2 = CreateNewSchema(alias:"schema2");

            var template1 = new HiveId(new Uri("storage://templates"), "templates", new HiveIdValue("home-page.cshtml"));
            var template2 = new HiveId(new Uri("storage://templates"), "templates", new HiveIdValue("faq-page.cshtml"));

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello"},
                                                  {"Alias", "hello"},
                                                  {"Icon", "myicon"},
                                                  {"Thumbnail", "mythumbnail"},
                                                  {"Description", "my description"},
                                                  {"DefaultTemplateId", template1.ToString()},
                                                  {"AllowedTemplates", string.Concat(template1.ToString(),",", template2.ToString()) },
                                                  {"AllowedChildren", string.Concat(schema1.Id.ToString(),",", schema2.Id.ToString())}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.AreEqual("Hello", model.Name);
            Assert.AreEqual("hello", model.Alias);
            Assert.AreEqual("myicon", model.Icon);
            Assert.AreEqual("mythumbnail", model.Thumbnail);
            Assert.AreEqual("my description", model.Description);
            Assert.AreEqual(template1, model.DefaultTemplateId);

            Assert.AreEqual(2, model.AllowedTemplates.Where(x => x.Selected).Count());
            Assert.AreEqual(template2.ToString(), model.AllowedTemplates.ElementAt(0).Value);
            Assert.AreEqual(template1.ToString(), model.AllowedTemplates.ElementAt(1).Value);

            var allowedChildren = model.AllowedChildren.Where(x => x.Selected).ToArray();
            Assert.AreEqual(2, allowedChildren.Count());
            Assert.AreEqual(schema1.Id.ToString(), allowedChildren.First().Value);
            Assert.AreEqual(schema2.Id.ToString(), allowedChildren.Last().Value);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_New_Property_Values_Bound()
        {
       
            var schema = CreateNewSchema();
            var attributeType = schema.AttributeTypes.Single(x => x.Alias == "test");

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"NewProperty.Name", "Hello"},
                                                  {"NewProperty.Alias", "hello"},
                                                  {"NewProperty.DataTypeId", attributeType.Id.ToString()},
                                                  {"NewProperty.TabId", schema.AttributeGroups.First().Id.ToString() },
                                                  {"NewProperty.Description", "my description"}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.AreEqual("Hello", model.NewProperty.Name);
            Assert.AreEqual("hello", model.NewProperty.Alias);
            Assert.AreEqual(attributeType.Id, model.NewProperty.DataTypeId);
            Assert.AreEqual(schema.AttributeGroups.First().Id, model.NewProperty.TabId);
            Assert.AreEqual("my description", model.NewProperty.Description);

        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Custom_Property_Bound_And_Validated()
        {
        
            var schema = CreateNewSchema();
            var attributeType = schema.AttributeTypes.Single(x => x.Alias == "test");
            var customAttrDef = schema.AttributeDefinitions.First(x => x.AttributeType == attributeType);
            var propHtmlId = customAttrDef.Id.GetHtmlId();

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {propHtmlId + ".Name", "Hello"},
                                                  {propHtmlId + ".Alias", "hello"},
                                                  {propHtmlId + ".DataTypeId", attributeType.Id.ToString()},
                                                  {propHtmlId + ".TabId", "8"},
                                                  {propHtmlId + ".Description", "hello, this is a description"},
                                                  {propHtmlId + ".SortOrder", "2"},
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            var customProp = model.Properties.Single(x => x.Id == customAttrDef.Id);
            Assert.IsTrue(controller.ModelState.IsValidField(propHtmlId + ".Name"));
            Assert.AreEqual("Hello", customProp.Name);
            Assert.IsTrue(controller.ModelState.IsValidField(propHtmlId + ".Alias"));
            Assert.AreEqual("hello", customProp.Alias);
            Assert.IsTrue(controller.ModelState.IsValidField(propHtmlId + ".DataTypeId"));
            Assert.AreEqual(attributeType.Id, customProp.DataTypeId);
            Assert.IsTrue(controller.ModelState.IsValidField(propHtmlId + ".TabId"));
            Assert.AreEqual(new HiveId(8), customProp.TabId);
            Assert.IsTrue(controller.ModelState.IsValidField(propHtmlId + ".Description"));
            Assert.AreEqual("hello, this is a description", model.Properties.Last().Description);
            Assert.IsTrue(controller.ModelState.IsValidField(propHtmlId + ".SortOrder"));
            Assert.AreEqual(2, customProp.SortOrder);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DocumentTypeEditorControllerTests_Custom_Property_Bound_And_Invalidated()
        {
        
            var schema = CreateNewSchema();
            var propHtmlId = schema.AttributeDefinitions.Last().Id.GetHtmlId();

            var controller = new DocumentTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {propHtmlId + ".Name", ""},
                                                  {propHtmlId + ".Alias", ""},
                                                  {propHtmlId + ".DataTypeId", ""},                                                  
                                                  {propHtmlId + ".SortOrder", "f"},
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.EditForm(schema.Id);
            var model = (DocumentTypeEditorModel)result.Model;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField(propHtmlId + ".Name"));
            Assert.IsFalse(controller.ModelState.IsValidField(propHtmlId + ".Alias"));
            Assert.IsFalse(controller.ModelState.IsValidField(propHtmlId + ".DataTypeId"));
            Assert.IsFalse(controller.ModelState.IsValidField(propHtmlId + ".SortOrder"));
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

    }
}
