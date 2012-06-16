using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
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
    public class DataTypeEditorControllerTests : AbstractContentControllerTest
    {
        
        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void DataTypeEditorControllerTests_DataType_Create()
        {
            //Arrange

            //create data type in persistence layer
            var propEditor = new MandatoryPropertyEditor();
            var dataTypeEntity = HiveModelCreationHelper.CreateAttributeType("test", "Test", "");
            dataTypeEntity.RenderTypeProvider = propEditor.Id.ToString();

            var controller = new DataTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello World"},
                                                   {"PropertyEditorId", FixedPropertyEditors.GetPropertyEditorDefinitions().First().Metadata.Id.ToString()},
                                                   { "submit.Save", "Save"} //set save flag
                                              }, GetBackOfficeRequestContext(), false);

            //Act

            var result = controller.CreateForm();

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);
            //get the new id from the route values
            var newId = ((RedirectToRouteResult)result).RouteValues["id"];
            Assert.AreEqual(typeof(HiveId), newId.GetType());

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var latestEntity = uow.Repositories.Schemas.Get<AttributeType>((HiveId)newId);

                Assert.AreEqual("Hello World", latestEntity.Name.Value);
                Assert.AreEqual(FixedPropertyEditors.GetPropertyEditorDefinitions().First().Metadata.Id.ToString(), latestEntity.RenderTypeProvider);
                Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(latestEntity.UtcCreated) < new TimeSpan(0, 1, 0));
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void DataTypeEditorControllerTests_DataType_Saved()
        {
            //Arrange

            //create data type in persistence layer
            var propEditor = new MandatoryPropertyEditor();
            var dataTypeEntity = HiveModelCreationHelper.CreateAttributeType("test", "Test", "");
            dataTypeEntity.RenderTypeProvider = propEditor.Id.ToString();
            UmbracoApplicationContext.AddPersistenceData(dataTypeEntity);

            var controller = new DataTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello World"},
                                                   {"PropertyEditorId", "5A379AF0-0256-4BE9-9D01-F149603DB257"},
                                                   { "submit.Save", "Save"} //set save flag
                                              }, GetBackOfficeRequestContext(), false);

            //Act

            var result = controller.EditForm(dataTypeEntity.Id);

            //Assert

            Assert.IsTrue(result is RedirectToRouteResult);

            using (var uow = UmbracoApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var latestEntity = uow.Repositories.Schemas.Get<AttributeType>(dataTypeEntity.Id);
                Assert.IsTrue(dataTypeEntity.UtcModified < latestEntity.UtcModified);                
            }
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void DataTypeEditorControllerTests_Create_New_Wizard_Step_Bound_And_Validated()
        {
            //Arrange

            var propEditor = new MandatoryPropertyEditor();
            var createModel = new CreateDataTypeModel { Name = "test", PropertyEditorId = propEditor.Id };

            //create data type in persistence layer
            var dataTypeEntity = HiveModelCreationHelper.CreateAttributeType("test", "Test", "");
            dataTypeEntity.RenderTypeProvider = propEditor.Id.ToString();
            UmbracoApplicationContext.AddPersistenceData(dataTypeEntity);

            var controller = new DataTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(GetBackOfficeRequestContext());

            //Act

            var result = (ViewResult)controller.CreateNewForm(createModel);
            var model = (CreateDataTypeModel)result.Model;

            //Assert

            Assert.IsTrue(controller.ModelState.IsValidField("Name"));
            Assert.IsTrue(controller.ModelState.IsValidField("PropertyEditorId"));

            Assert.AreEqual("test", model.Name);
            Assert.AreEqual(propEditor.Id, model.PropertyEditorId);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.ContentEditing)]
        public void DataTypeEditorControllerTests_Create_New_Wizard_Step_Bound_And_Invalidated()
        {
            //Arrange

            var createModel = new CreateDataTypeModel { Name = "", PropertyEditorId = Guid.Empty };

            //create data type in persistence layer
            var dataTypeEntity = HiveModelCreationHelper.CreateAttributeType("test", "Test", "");
            UmbracoApplicationContext.AddPersistenceData(dataTypeEntity);

            var controller = new DataTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(GetBackOfficeRequestContext());

            //Act
            var result = (ViewResult)controller.CreateNewForm(createModel);

            //Assert
            Assert.IsFalse(controller.ModelState.IsValidField("Name"));

        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DataTypeEditorControllerTests_Invalid_Model_State_When_Missing_Required_Values()
        {
            //Arrange

            //create data type in persistence layer
            var propEditor = new MandatoryPropertyEditor();
            var dataTypeEntity = HiveModelCreationHelper.CreateAttributeType("test", "Test", "");
            dataTypeEntity.RenderTypeProvider = propEditor.Id.ToString();
            UmbracoApplicationContext.AddPersistenceData(dataTypeEntity);

            var controller = new DataTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", ""},
                                                   {"PropertyEditorId", ""}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = controller.EditForm(dataTypeEntity.Id) as ViewResult;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField("Name"));
            Assert.IsFalse(controller.ModelState.IsValidField("PropertyEditorId"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DataTypeEditorControllerTests_All_Standard_Values_Bound()
        {
            //Arrange

            //create data type in persistence layer
            var propEditor = new MandatoryPropertyEditor();
            var dataTypeEntity = HiveModelCreationHelper.CreateAttributeType("test", "Test", "");
            dataTypeEntity.RenderTypeProvider = propEditor.Id.ToString();
            UmbracoApplicationContext.AddPersistenceData(dataTypeEntity);

            var controller = new DataTypeEditorController(GetBackOfficeRequestContext());

            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"Name", "Hello World"},
                                                   {"PropertyEditorId", "5A379AF0-0256-4BE9-9D01-F149603DB257"}
                                              }, GetBackOfficeRequestContext());

            //Act
            var result = controller.EditForm(dataTypeEntity.Id) as ViewResult;
            var modelState = controller.ModelState;

            //Assert

            Assert.AreEqual("Hello World", controller.ModelState["Name"].Value.AttemptedValue);
            Assert.IsTrue(controller.ModelState.IsValidField("Name"));
            Assert.AreEqual("5A379AF0-0256-4BE9-9D01-F149603DB257", controller.ModelState["PropertyEditorId"].Value.AttemptedValue);
            Assert.IsTrue(controller.ModelState.IsValidField("PropertyEditorId"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DataTypeEditorControllerTests_Invalid_PreValueModel_Data_Will_Result_In_Invalid_ModelState()
        {
            //Arrange

            //create data type in persistence layer
            var propEditor = new MandatoryPropertyEditor();
            var dataTypeEntity = HiveModelCreationHelper.CreateAttributeType("test", "Test", "");
            dataTypeEntity.RenderTypeProvider = propEditor.Id.ToString();
            UmbracoApplicationContext.AddPersistenceData(dataTypeEntity);

            var controller = new DataTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"PreValueEditorModel.Value", ""}
                                              }, GetBackOfficeRequestContext());

            //Act

            var result = controller.EditForm(dataTypeEntity.Id) as ViewResult;

            //Assert

            Assert.IsFalse(controller.ModelState.IsValidField("PreValueEditorModel.Value"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.DataTypeEditing)]
        public void DataTypeEditorControllerTests_PreValueModel_Data_Will_Be_Updated_With_Posted_Values()
        {
            //Arrange

            //create data type in persistence layer
            var propEditor = new MandatoryPropertyEditor();
            var dataTypeEntity = HiveModelCreationHelper.CreateAttributeType("test", "Test", "");
            dataTypeEntity.RenderTypeProvider = propEditor.Id.ToString();
            UmbracoApplicationContext.AddPersistenceData(dataTypeEntity);

            var controller = new DataTypeEditorController(GetBackOfficeRequestContext());
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>
                                              {
                                                  {"PreValueEditorModel.Value", "hello"}
                                              }, GetBackOfficeRequestContext());

            //Act
            var result = controller.EditForm(dataTypeEntity.Id) as ViewResult;

            //Assert
            Assert.IsTrue(controller.ModelState.IsValidField("PreValueEditorModel.Value"));
            Assert.AreEqual("hello", controller.ModelState["PreValueEditorModel.Value"].Value.AttemptedValue);
        }


    }
}
