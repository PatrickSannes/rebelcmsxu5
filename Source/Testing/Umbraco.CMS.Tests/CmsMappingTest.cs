using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.PropertyEditors.DateTimePicker;
using Umbraco.Cms.Web.PropertyEditors.ListPicker;
using Umbraco.Cms.Web.PropertyEditors.Numeric;
using Umbraco.Cms.Web.PropertyEditors.TreeNodePicker;
using Umbraco.Cms.Web.PropertyEditors.TrueFalse;
using Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.NodeName;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;
using Umbraco.Tests.Extensions.Stubs.PropertyEditors;
using Umbraco.Cms.Web.Trees;

namespace Umbraco.Tests.Cms
{
    /// <summary>
    /// Summary description for CmsMappingTest
    /// </summary>
    [TestFixture]
    public class CmsMappingTest
    {
        private FakeFrameworkContext _fakeFrameworkContext;
        private FakeUmbracoApplicationContext _appContext;
        private MockedMapResolverContext _resolverContext;
        private CmsModelMapper _cmsModelMapper;

        private IReadonlyEntityRepositoryGroup<IContentStore> _readonlySession;
        private IReadonlySchemaRepositoryGroup<IContentStore> _readonlySchemaSession;
        private IEntityRepositoryGroup<IContentStore> _repository;
        private ISchemaRepositoryGroup<IContentStore> _schemaSession;

        private IDependencyResolver _dependencyResolver;
        private HashSet<AttributeGroup> _createdTabs = new HashSet<AttributeGroup>();

        [TestFixtureSetUp]
        public void TestInit()
        {

            _fakeFrameworkContext = new FakeFrameworkContext();
                
            //mock hive
            var hive = MockHiveManager.GetManager(_fakeFrameworkContext)
                .MockContentStore(out _readonlySession, out _readonlySchemaSession, out _repository, out _schemaSession);
            
            //need to stub the repo to return the correct groups for mapping to resolve
            _readonlySchemaSession.Get<AttributeGroup>(Arg.Any<bool>(), Arg.Any<HiveId[]>())
                .Returns(x => _createdTabs.Select(tab => _cmsModelMapper.Map<AttributeGroup>(tab))
                                  .Where(a => ((HiveId[])x[1]).Contains(a.Id))
                                  .ToArray());
            
            _appContext = new FakeUmbracoApplicationContext(hive, false);

            _resolverContext = new MockedMapResolverContext(_fakeFrameworkContext, hive, new MockedPropertyEditorFactory(_appContext), new MockedParameterEditorFactory());

            //mappers
            _cmsModelMapper = new CmsModelMapper(_resolverContext);
            var persistenceToRenderModelMapper = new RenderTypesModelMapper(_resolverContext);
            _fakeFrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { _cmsModelMapper, persistenceToRenderModelMapper, new FrameworkModelMapper(_fakeFrameworkContext) }));

            //mock dependency resolver
            _dependencyResolver = Substitute.For<IDependencyResolver>();
            DependencyResolver.SetResolver(_dependencyResolver);
            _dependencyResolver.GetService(typeof(IUmbracoApplicationContext)).Returns(_appContext);
            _dependencyResolver.GetService(typeof(IRoutingEngine)).Returns(Substitute.For<IRoutingEngine>());

        }

        [Test]
        public void Ensure_Correct_Object_Instances_In_Graph_When_Mapped_To_Typed_Entity()
        {
            var input = CreateContentEditorModel<ContentEditorModel>("test1", "test2", "test3");
            var output = _cmsModelMapper.Map<TypedEntity>(input);

            output.AssignIds(() => new HiveId(Guid.NewGuid()));

            var entityGroups = output.AttributeGroups.ToArray();
            var entityAttributeDefGroups = output.Attributes.Select(x => x.AttributeDefinition.AttributeGroup).ToArray();
            var schemaGroups = output.EntitySchema.AttributeGroups.ToArray();
            var schemaDefGroups = output.EntitySchema.AttributeDefinitions.Select(x => x.AttributeGroup).ToArray();
            foreach (var g in schemaDefGroups)
            {
                var found = schemaGroups.Single(x => x.Id == g.Id);
                Assert.IsTrue(ReferenceEquals(g, found));
            }
            foreach (var g in entityGroups)
            {
                var found = schemaGroups.Single(x => x.Id == g.Id);
                Assert.IsTrue(ReferenceEquals(g, found));
            }
            foreach (var g in entityAttributeDefGroups)
            {
                var found = schemaGroups.Single(x => x.Id == g.Id);
                Assert.IsTrue(ReferenceEquals(g, found));
            }

            var entityAttributeDefs = output.Attributes.Select(x => x.AttributeDefinition).ToArray();
            var schemaAttributeDefs = output.EntitySchema.AttributeDefinitions.ToArray();
            foreach(var d in entityAttributeDefs)
            {
                var found = schemaAttributeDefs.Single(x => x.Id == d.Id);
                Assert.IsTrue(ReferenceEquals(d, found));
            }

            var entityAttributeTypes = output.Attributes.Select(x => x.AttributeDefinition.AttributeType).ToArray();
            var schemaAttributeTypes = output.EntitySchema.AttributeTypes.ToArray();
            var schemaAttDefAttributeTypes = output.EntitySchema.AttributeDefinitions.Select(x => x.AttributeType).ToArray();
            foreach (var a in entityAttributeTypes)
            {
                var found = schemaAttributeTypes.Single(x => x.Id == a.Id);
                Assert.IsTrue(ReferenceEquals(a, found));
            }
            foreach (var a in schemaAttDefAttributeTypes)
            {
                var found = schemaAttributeTypes.Single(x => x.Id == a.Id);
                Assert.IsTrue(ReferenceEquals(a, found));
            }
        }

        [Test]
        public void Ensure_Correct_Object_Instances_In_Graph_When_Mapped_To_Entity_Schema()
        {
            var input = CreateDocumentTypeEditorModel("test1", "test2", "test3");
            var output = _cmsModelMapper.Map<EntitySchema>(input);

            output.AssignIds(() => new HiveId(Guid.NewGuid()));            

            var schemaGroups = output.AttributeGroups.ToArray();
            var schemaDefGroups = output.AttributeDefinitions.Select(x => x.AttributeGroup).ToArray();
            foreach(var g in schemaDefGroups)
            {
                var found = schemaGroups.Single(x => x.Id == g.Id);
                Assert.IsTrue(ReferenceEquals(g, found));
            }
            foreach(var a in output.AttributeDefinitions.Select(x => x.AttributeType))
            {
                var found = output.AttributeTypes.Single(x => x.Id == a.Id);
                Assert.IsTrue(ReferenceEquals(a, found));
            }
        }

        [Test]
        public void CmsMappping_DataTypeEditorModel_To_AttributeType()
        {
            var input = CreateDataTypeEditorModel();
            var output = _cmsModelMapper.Map<AttributeType>(input);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Name, output.Name.ToString());
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.PropertyEditorId.ToString(), output.RenderTypeProvider);
            Assert.AreEqual(input.PreValueEditorModel.GetSerializedValue(), output.RenderTypeProviderConfig);
            Assert.AreEqual(new LongStringSerializationType().GetType(), output.SerializationType.GetType());
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
        }

        #region DataType <-> AttributeType

        [Test]
        public void CmsMappping_DataType_To_AttributeType()
        {
            var input = CreateDataType();
            var output = _cmsModelMapper.Map<AttributeType>(input);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Name, output.Name.ToString());
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.PropertyEditor.Id.ToString(), output.RenderTypeProvider);
            Assert.AreEqual(input.Prevalues, output.RenderTypeProviderConfig);
            Assert.AreEqual(new LongStringSerializationType().GetType(), output.SerializationType.GetType());
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
        }

        [Test]
        public void CmsMappping_AttributeType_To_DataType()
        {
            var input = CreateAttributeType();

            var output = _cmsModelMapper.Map<DataType>(input);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Name.ToString(), output.Name);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.RenderTypeProvider, output.PropertyEditor.Id.ToString());
            Assert.AreEqual(input.RenderTypeProviderConfig, output.Prevalues);
            Assert.AreEqual(input.SerializationType.GetType(), new StringSerializationType().GetType());
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
            Assert.AreEqual(new MandatoryPropertyEditor().GetType(), output.PropertyEditor.GetType());
        } 

        #endregion

        [Test]
        public void CmsMappping_HashSet_Tab_To_HashSet_Tab()
        {
            var input = new HashSet<Tab>(new[] {CreateTab(), CreateTab(), CreateTab()});
            var output = _cmsModelMapper.Map<HashSet<Tab>>(input);
            Assert.AreEqual(input.Count, output.Count);
        }

        [Test]
        public void CmsMappping_EntitySchema_To_DocumentTypeInfo()
        {
            var input = CreateEntitySchema();
            var output = _cmsModelMapper.Map<DocumentTypeInfo>(input);
            Assert.AreEqual(input.GetXmlConfigProperty("description"), output.Description);
            Assert.AreEqual(input.GetXmlConfigProperty("icon"), output.Icon);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Name.ToString(), output.Name);
            Assert.AreEqual(input.GetXmlConfigProperty("thumb"), output.Thumbnail);
        }

        [Test]
        public void CmsMappping_Tab_To_Tab()
        {
            var input = CreateTab();
            var output = _cmsModelMapper.Map<Tab>(input);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Name, output.Name);
            Assert.AreEqual(input.SortOrder, output.SortOrder);
        }

        [Test]
        public void CmsMappping_DocumentTypeEditorModel_To_New_UserGroupEditorModel()
        {
            var input = CreateDocumentTypeEditorModel(UserGroupSchema.UsersAlias);
            var output = _cmsModelMapper.Map<UserGroupEditorModel>(input);

            AssertDocumentTypeEditorModelToContentEditorModelBaseTypes(input, output);

        }

        [Test]
        public void CmsMappping_DocumentTypeEditorModel_To_New_UserEditorModel()
        {
            var input = CreateDocumentTypeEditorModel(UserSchema.ApplicationsAlias, UserSchema.CommentsAlias, UserSchema.EmailAlias, UserSchema.IsApprovedAlias,
                UserSchema.LastActivityDateAlias, UserSchema.LastLoginDateAlias, UserSchema.LastPasswordChangeDateAlias, UserSchema.PasswordAlias,
                UserSchema.PasswordAnswerAlias, UserSchema.PasswordQuestionAlias, UserSchema.PasswordSaltAlias, UserSchema.PersistLoginAlias,
                UserSchema.SchemaAlias, UserSchema.SessionTimeoutAlias, UserSchema.StartContentHiveIdAlias, UserSchema.StartMediaHiveIdAlias, UserSchema.UsernameAlias);
            input.Properties.Single(x => x.Alias == UserSchema.SessionTimeoutAlias).DataType.InternalPropertyEditor = new NumericEditor();            
            var output = _cmsModelMapper.Map<UserEditorModel>(input);

            AssertDocumentTypeEditorModelToContentEditorModelBaseTypes(input, output);
            Assert.AreEqual(null, output.Email);
            Assert.AreEqual(false, output.IsApproved);
            Assert.AreEqual(null, output.LastActivityDate);
            Assert.AreEqual(null, output.LastLoginDate);
            Assert.AreEqual(null, output.LastPasswordChangeDate);
            Assert.AreEqual(null, output.Password);
            Assert.AreEqual(0, output.SessionTimeout);
            Assert.AreEqual(HiveId.Empty, output.StartContentHiveId);
            Assert.AreEqual(HiveId.Empty, output.StartMediaHiveId);
        }

        [Test]
        public void CmsMappping_DocumentTypeEditorModel_To_New_ContentEditorModel()
        {
            var input = CreateDocumentTypeEditorModel();
            var output = _cmsModelMapper.Map<ContentEditorModel>(input);

            AssertDocumentTypeEditorModelToContentEditorModelBaseTypes(input, output);
        }

        private static void AssertDocumentTypeEditorModelToContentEditorModelBaseTypes(DocumentTypeEditorModel input, BasicContentEditorModel output)
        {
            Assert.AreNotEqual(input.Id, output.Id);
            Assert.AreEqual(input.Properties.Count, output.Properties.Count);
            Assert.AreEqual(1, output.Properties.Where(x => x.DocTypeProperty.DataType.InternalPropertyEditor is IContentAwarePropertyEditor).Count());
            var contentAwarePropEditor = output.Properties.Where(x => x.DocTypeProperty.DataType.InternalPropertyEditor is IContentAwarePropertyEditor).Single().DocTypeProperty.DataType.InternalPropertyEditor as IContentAwarePropertyEditor;
            Assert.IsTrue(contentAwarePropEditor.IsDocumentTypeAvailable);
            Assert.AreEqual(input.Id, output.DocumentTypeId);
            Assert.AreEqual(input.Name, output.DocumentTypeName);
            Assert.AreEqual(input.Properties.Select(x => x.TabId).Distinct().Count(), output.Tabs.Count);

            Assert.AreNotEqual(input.ActiveTabIndex, output.ActiveTabIndex);
            Assert.AreNotEqual(input.Name, output.Name);
            Assert.AreNotEqual(input.ParentId, output.ParentId);
            Assert.AreNotEqual(input.CreatedBy, output.CreatedBy);
            Assert.AreNotEqual(input.UpdatedBy, output.UpdatedBy);
            Assert.AreNotEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreNotEqual(input.UtcModified, output.UtcModified);
        }

        [Test]
        public void CmsMappping_HashSet_DocumentTypeProperty_To_HashSet_ContentProperty()
        {
            var input = new HashSet<DocumentTypeProperty>(new[] {CreateDocumentTypeProperty(), CreateDocumentTypeProperty(), CreateDocumentTypeProperty()});
            var output = _cmsModelMapper.Map<HashSet<ContentProperty>>(input);

            Assert.AreEqual(input.Count, output.Count);
        }

        [Test]
        public void CmsMappping_DocumentTypeProperty_To_New_ContentProperty()
        {
            var input = CreateDocumentTypeProperty(new TestContentAwarePropertyEditor());

            var output = _cmsModelMapper.Map<ContentProperty>(input);

            Assert.AreNotEqual(input.Id, output.Id);
            Assert.AreEqual(input.Name, output.Name);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input, output.DocTypeProperty);
            Assert.AreEqual(input.Id, output.DocTypePropertyId);
            Assert.AreEqual(input.SortOrder, output.SortOrder);
            Assert.AreEqual(input.TabId, output.TabId);
            Assert.AreNotEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreNotEqual(input.UtcModified, output.UtcModified);

            Assert.IsTrue(output.DocTypeProperty.DataType.PropertyEditor is IContentAwarePropertyEditor);
            Assert.IsTrue(((IContentAwarePropertyEditor)output.DocTypeProperty.DataType.PropertyEditor).IsContentPropertyAvailable);
        }
        
        [Test]
        public void CmsMappping_DataType_To_DataTypeEditorModel()
        {
            var input = new DataType(HiveId.ConvertIntToGuid(1235), "Test", "test", new MandatoryPropertyEditor(), "")
                {
                    CreatedBy = "Shannon",
                    ParentId = HiveId.ConvertIntToGuid(9),
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now
                };

            var output = _cmsModelMapper.Map<DataTypeEditorModel>(input);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.GetPreValueModel().GetType(), output.PreValueEditorModel.GetType());
            Assert.AreEqual(input.Name, output.Name);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.UpdatedBy, output.UpdatedBy);
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
            Assert.AreEqual(input.ParentId, output.ParentId);
            var preValModel = (MandatoryPreValueModel)output.PreValueEditorModel;
            Assert.AreEqual(((MandatoryPreValueModel)input.GetPreValueModel()).Value, preValModel.Value);
            Assert.AreEqual((Guid?)input.PropertyEditor.Id, output.PropertyEditorId);
        }
        
        
        #region TypedEntity <-> RVM.Content

        [Test]
        public void CmsMapping_TypedEntity_MapsTo_Content()
        {
           
            // Arrange
            var generalGroup = FixedGroupDefinitions.GeneralGroup;
            var entityParent = HiveModelCreationHelper.MockTypedEntity(true);
            entityParent.Attributes.SetValueOrAdd(new SelectedTemplateAttribute(new HiveId("RunwayHomepage.cshtml"), generalGroup));

            var entityChild = HiveModelCreationHelper.MockTypedEntity(true);
            entityChild.Attributes.SetValueOrAdd(new SelectedTemplateAttribute(new HiveId("RunwayHomepage.cshtml"), generalGroup));

            var entityGrandChild = HiveModelCreationHelper.MockTypedEntity(true);
            entityGrandChild.Attributes.SetValueOrAdd(new SelectedTemplateAttribute(new HiveId("RunwayHomepage.cshtml"), generalGroup));

            entityParent.RelationProxies.EnlistChild(entityChild, FixedRelationTypes.DefaultRelationType);
            entityChild.RelationProxies.EnlistChild(entityGrandChild, FixedRelationTypes.DefaultRelationType);

            //In order for this test to work, we need to stub the Repository.GetChildRelations method to return the correct data
            _readonlySession.GetChildRelations(entityParent.Id, FixedRelationTypes.DefaultRelationType)
                .ReturnsForAnyArgs(new[] {entityParent.RelationProxies.GetChildRelations(FixedRelationTypes.DefaultRelationType).Single().Item});

            // GetXXRelations and RenderViewModelExtensions internally can call Get so this needs mocking too
            _readonlySession
                .Get<TypedEntity>(true, Arg.Any<HiveId[]>())
                .Returns(MockHiveManager.MockReturnForGet<TypedEntity>());

            // Act
            var mapped = _fakeFrameworkContext.TypeMappers.Map<Content>(entityParent);
            var bendy = mapped.Bend();

            // Assert
            Assert.AreEqual(entityParent.Id, mapped.Id);
            Assert.AreEqual(entityParent.Attributes.Count, mapped.Attributes.Count(), "Different number of fields");
            Assert.IsTrue(mapped.Attributes.Count() > 0, "Not more than 0 fields");

            foreach (var field in mapped.Attributes)
            {
                Assert.IsFalse(string.IsNullOrEmpty(field.AttributeDefinition.Alias), "Field alias is empty");
                Assert.IsFalse(string.IsNullOrEmpty(field.AttributeDefinition.Name), "Field name is empty");
            }

            Assert.IsNotNull(mapped.ContentType, "ContentType is null");
            Assert.AreEqual(entityParent.EntitySchema.Id, mapped.ContentType.Id, "ContentType Id differs from schema");

            foreach (var fieldDefinition in mapped.ContentType.AttributeDefinitions)
            {
                Assert.IsFalse(string.IsNullOrEmpty(fieldDefinition.Alias), "FieldDefinition alias is empty");
                Assert.IsFalse(string.IsNullOrEmpty(fieldDefinition.Name), "FieldDefinition name is empty");
            }

            Assert.IsNotNull(bendy, "AsDynamic() is null");
            Assert.IsNotNull(bendy.Children, "Children is null");
            Assert.IsTrue(Enumerable.Count(bendy.Children) > 0, "Children.Count() is not > 0");
            Assert.IsTrue(mapped.ChildContent().Count() > 0, "Children.Count() is not > 0");

            // Establish that mapped.ChildContent() is the same as bendy.Children
            Assert.That(mapped.ChildContent().Count(), Is.EqualTo(Enumerable.Count(bendy.Children)));

            // NB this was here as of 16/Oct but I've disabled as bendy.Children performs further queries, it's not mapped from
            // the variable "mapped" so even if it was accurately mocked to return the same items, it's probably just testing
            // the mocking rather than the mapping (APN)
            //foreach (var child in bendy.Children)
            //{
            //    //Assert.IsTrue(child is Content, "Child is not of type Content but is {0}", child.GetType());
            //    Assert.AreEqual(entityChild.Id, child.Id);
            //    Assert.AreEqual(entityChild.Attributes[NodeNameAttributeDefinition.AliasValue].Values["Name"], child.Name);
            //    Assert.AreEqual(entityChild.Attributes["alias-1"].DynamicValue, child.Alias1);
            //    Assert.AreEqual(entityChild.Attributes["alias-2"].DynamicValue, child.Alias2);
            //    Assert.AreEqual(entityChild.Attributes["alias-3"].DynamicValue, child.Alias3);
            //    Assert.AreEqual(entityChild.Attributes["alias-4"].DynamicValue, child.Alias4);
            //}


            Assert.IsNotNull(mapped.CurrentTemplate, "Current template null");

            //BUG: There is currently no mapping for this
            Assert.IsNotNull(mapped.AlternativeTemplates, "AlternativeTemplates is null");
        }
        
        #endregion

        #region DataTypeEditorModel <-> AttributeType

        /// <summary>
        /// 
        /// </summary>
        [TestCase("Test")]
        [TestCase("test-my-hyphenated")]
        public void CmsMapping_DataTypeEditorModel_To_AttributeType(string dataTypeName)
        {
            //Arrange

            var propertyEditor = new MandatoryPropertyEditor();
            var model = new DataTypeEditorModel(new HiveId(Guid.NewGuid()))
                {
                    CreatedBy = "Shannon",
                    Name = dataTypeName,
                    ParentId = new HiveId(-1),
                    PreValueEditorModel = propertyEditor.CreatePreValueEditorModel(),
                    PropertyEditorId = propertyEditor.Id,
                    UpdatedBy = "Shannon",
                    UtcCreated = DateTimeOffset.UtcNow,
                    UtcModified = DateTimeOffset.UtcNow
                };

            //Act

            var attributeDef = _fakeFrameworkContext.TypeMappers.Map<DataTypeEditorModel, AttributeType>(model);

            //Assert
            Assert.AreEqual(model.Alias, attributeDef.Alias);
            Assert.AreEqual(model.Name, attributeDef.Name.ToString());
            Assert.AreEqual(model.PreValueEditorModel.GetSerializedValue(), attributeDef.RenderTypeProviderConfig);
            Assert.AreEqual(model.PropertyEditorId.ToString(), attributeDef.RenderTypeProvider);
            Assert.AreEqual(model.UtcCreated, attributeDef.UtcCreated);
            Assert.AreEqual(model.UtcModified, attributeDef.UtcModified);

            //NOTE: This doesn't need to map anywhere
            //Assert.AreEqual(model.ParentId, attributeDef.Name);

            //BUG: these do not map anywhere
            //Assert.AreEqual(model.CreatedBy, attributeDef.CreatedBy);
            //Assert.AreEqual(model.UpdatedBy, attributeDef.UpdatedBy);

        }

        [Test]
        public void CmsMapping_AttributeType_To_DataTypeEditorModel()
        {
            var input = CreateAttributeType();
            var output = _cmsModelMapper.Map<DataTypeEditorModel>(input);

            Assert.AreEqual(input.Name.ToString(), output.Name);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(typeof(MandatoryPreValueModel), output.PreValueEditorModel.GetType());
            Assert.AreEqual(input.RenderTypeProvider, output.PropertyEditorId.ToString());
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
            
            //BUG: there doesn't appear to be anywhere to track the author
            //Assert.AreEqual("", dataTypeViewModel.UpdatedBy);            
        }
        
        #endregion

        #region BasicContentEditorModel Types <-> Revision<TypedEntity> / Revision<TypedEntity> Types

        [Test]
        public void CmsMapping_Revision_TypedEntity_To_ContentEditorModel()
        {
            var te = CreateTypedEntity<TypedEntity>();
            te.AssignIds(() => new HiveId(Guid.NewGuid()));
            var input = new Revision<TypedEntity>(te)
            {
                MetaData = new RevisionData(new HiveId(Guid.NewGuid()), FixedStatusTypes.Published)
                {
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now,
                    UtcStatusChanged = DateTime.Now
                }
            };
            var output = _cmsModelMapper.Map<ContentEditorModel>(input);

            AssertTypedEntityToContentEditorModelMappings(input.Item, output);
            Assert.AreEqual(input.MetaData.Id, output.RevisionId);
        }

        [Test]
        public void CmsMapping_EntitySnapshot_TypedEntity_To_ContentEditorModel()
        {
            var te = CreateTypedEntity<TypedEntity>();
            te.AssignIds(() => new HiveId(Guid.NewGuid()));
            var rData = new RevisionData(new HiveId(Guid.NewGuid()), FixedStatusTypes.Published)
                {
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now,
                    UtcStatusChanged = DateTime.Now
                };
            var input = new EntitySnapshot<TypedEntity>(new Revision<TypedEntity>(te)
                {
                    MetaData = rData
                }, new[] { rData });
            var output = _cmsModelMapper.Map<ContentEditorModel>(input);
            
            AssertTypedEntityToContentEditorModelMappings(input.Revision.Item, output);
            Assert.AreEqual(input.Revision.MetaData.UtcStatusChanged, output.UtcPublishedDate);

        }

        [Test]
        public void CmsMapping_UserGroup_To_UserGroupEditorModel()
        {
            var customAttributes = new[]
                {
                    UserGroupSchema.UsersAlias
                };
            var input = CreateTypedEntity<UserGroup>(true, customAttributes);
            input.AssignIds(() => new HiveId(Guid.NewGuid()));
            input.Attributes[NodeNameAttributeDefinition.AliasValue].AttributeDefinition.AttributeType.RenderTypeProvider = new MandatoryPropertyEditor().Id.ToString();
            input.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue = "Test" + Guid.NewGuid();
            //input.Attributes[UserGroupSchema.UsersAlias].DynamicValue = new[] { new SelectListItem() { Value = "test" + Guid.NewGuid() } };
            //input.Attributes[UserGroupSchema.UsersAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new ListPickerEditor().Id.ToString();

            var output = _cmsModelMapper.Map<UserGroupEditorModel>(input);
            AssertTypedEntityToContentEditorModelMappings(input, output);
            Assert.AreEqual(input.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue, output.Name);
            //NOTE: we're not testing this property because the list picker prop editor isn't behaving. SD.
            //Assert.AreEqual(Enumerable.Count(input.Attributes[UserGroupSchema.UsersAlias].DynamicValue), output.Users.Count());         
        }

        [Test]
        public void CmsMapping_User_To_UserEditorModel()
        {
            var customAttributes = new[] {UserSchema.ApplicationsAlias, UserSchema.CommentsAlias, UserSchema.EmailAlias, UserSchema.IsApprovedAlias,
                UserSchema.LastActivityDateAlias, UserSchema.LastLoginDateAlias, UserSchema.LastPasswordChangeDateAlias, UserSchema.PasswordAlias,
                UserSchema.PasswordAnswerAlias, UserSchema.PasswordQuestionAlias, UserSchema.PasswordSaltAlias, UserSchema.PersistLoginAlias,
                UserSchema.SchemaAlias, UserSchema.SessionTimeoutAlias, UserSchema.StartContentHiveIdAlias, UserSchema.StartMediaHiveIdAlias, UserSchema.UsernameAlias};
            var input = CreateTypedEntity<User>(true, customAttributes);
            input.AssignIds(() => new HiveId(Guid.NewGuid()));
            input.Attributes[NodeNameAttributeDefinition.AliasValue].AttributeDefinition.AttributeType.RenderTypeProvider = new MandatoryPropertyEditor().Id.ToString();
            input.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue = "Test" + Guid.NewGuid();

            input.Attributes[UserSchema.EmailAlias].DynamicValue = "test" + Guid.NewGuid();
            input.Attributes[UserSchema.EmailAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new MandatoryPropertyEditor().Id.ToString();
            
            input.Attributes[UserSchema.IsApprovedAlias].DynamicValue = true.ToString();
            input.Attributes[UserSchema.IsApprovedAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new TrueFalseEditor().Id.ToString();
            
            input.Attributes[UserSchema.LastActivityDateAlias].DynamicValue = DateTime.Now;
            input.Attributes[UserSchema.LastActivityDateAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new DateTimePickerEditor().Id.ToString();

            input.Attributes[UserSchema.LastLoginDateAlias].DynamicValue = DateTime.Now;
            input.Attributes[UserSchema.LastLoginDateAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new DateTimePickerEditor().Id.ToString();

            input.Attributes[UserSchema.LastPasswordChangeDateAlias].DynamicValue = DateTime.Now;
            input.Attributes[UserSchema.LastPasswordChangeDateAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new DateTimePickerEditor().Id.ToString();

            input.Attributes[UserSchema.PasswordAlias].DynamicValue = "test" + Guid.NewGuid();
            input.Attributes[UserSchema.PasswordAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new MandatoryPropertyEditor().Id.ToString();

            input.Attributes[UserSchema.SessionTimeoutAlias].DynamicValue = new Random().Next(0, 1000);
            input.Attributes[UserSchema.SessionTimeoutAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new NumericEditor().Id.ToString();

            input.Attributes[UserSchema.StartContentHiveIdAlias].DynamicValue = new HiveId(Guid.NewGuid()).ToString();
            input.Attributes[UserSchema.StartContentHiveIdAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new TreeNodePicker(Enumerable.Empty<Lazy<TreeController, TreeMetadata>>()).Id.ToString();

            input.Attributes[UserSchema.StartMediaHiveIdAlias].DynamicValue = new HiveId(Guid.NewGuid()).ToString();
            input.Attributes[UserSchema.StartMediaHiveIdAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new TreeNodePicker(Enumerable.Empty<Lazy<TreeController, TreeMetadata>>()).Id.ToString();

            input.Attributes[UserSchema.UsernameAlias].DynamicValue = "test" + Guid.NewGuid();
            input.Attributes[UserSchema.UsernameAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new MandatoryPropertyEditor().Id.ToString();

            //NOTE: we're not testing this property because the list picker prop editor isn't behaving. SD.
            //input.Attributes[UserSchema.ApplicationsAlias].DynamicValue = new[] {new SelectListItem() {Value = "test" + Guid.NewGuid()}};
            //input.Attributes[UserSchema.ApplicationsAlias].AttributeDefinition.AttributeType.RenderTypeProvider = new ListPickerEditor().Id.ToString();
            
            var output = _cmsModelMapper.Map<UserEditorModel>(input);
            AssertTypedEntityToContentEditorModelMappings(input, output);
            Assert.AreEqual(input.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue, output.Name);
            Assert.AreEqual(input.Attributes[UserSchema.EmailAlias].DynamicValue, output.Email);
            Assert.AreEqual(input.Attributes[UserSchema.IsApprovedAlias].DynamicValue, output.IsApproved.ToString());
            Assert.AreEqual(input.Attributes[UserSchema.LastActivityDateAlias].DynamicValue, output.LastActivityDate);
            Assert.AreEqual(input.Attributes[UserSchema.LastLoginDateAlias].DynamicValue, output.LastLoginDate);
            Assert.AreEqual(input.Attributes[UserSchema.LastPasswordChangeDateAlias].DynamicValue, output.LastPasswordChangeDate);
            Assert.AreEqual(input.Attributes[UserSchema.PasswordAlias].DynamicValue, output.Password);
            Assert.AreEqual(input.Attributes[UserSchema.SessionTimeoutAlias].DynamicValue, output.SessionTimeout);
            Assert.AreEqual(input.Attributes[UserSchema.StartContentHiveIdAlias].DynamicValue, output.StartContentHiveId.ToString());
            Assert.AreEqual(input.Attributes[UserSchema.StartMediaHiveIdAlias].DynamicValue, output.StartMediaHiveId.ToString());
            Assert.AreEqual(input.Attributes[UserSchema.UsernameAlias].DynamicValue, output.Username);
            //Assert.AreEqual(Enumerable.Count(input.Attributes[UserSchema.ApplicationsAlias].DynamicValue), output.Applications.Count());
        }

        [Test]
        public void CmsMapping_TypedEntity_To_ContentEditorModel()
        {
            var input = CreateTypedEntity<TypedEntity>();
            input.AssignIds(() => new HiveId(Guid.NewGuid()));
            input.Attributes[NodeNameAttributeDefinition.AliasValue].Values.Add("Name", "Test" + Guid.NewGuid());

            var output = _cmsModelMapper.Map<ContentEditorModel>(input);

            AssertTypedEntityToContentEditorModelMappings(input, output);
            Assert.AreEqual(input.Attributes[NodeNameAttributeDefinition.AliasValue].Values["Name"], output.Name);

            Assert.AreEqual(HiveId.Empty, output.RevisionId);
            Assert.AreEqual(null, output.UtcPublishedDate);
        }

        private void AssertTypedEntityToContentEditorModelMappings(TypedEntity input, BasicContentEditorModel output)
        {
            //BUG: mapping for this doesn't exist
            //Assert.AreEqual("", output.CreatedBy);
            Assert.AreEqual(input.EntitySchema.Id, output.DocumentTypeId);
            Assert.AreEqual(input.EntitySchema.Name.ToString(), output.DocumentTypeName);
            Assert.AreEqual(input.Id, output.Id);            
            Assert.AreEqual(input.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType).First().Item.SourceId, output.ParentId);
            Assert.AreEqual(input.Attributes.Count(), output.Properties.Count());
            
            Assert.AreEqual(input.AttributeGroups.Count(), output.Tabs.Count());
            //BUG: mapping for this doesn't exist
            //Assert.AreEqual("", output.UpdatedBy);
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
            
            //BUG: mapping for this doesn't exist
            //Assert.AreEqual("", output.UtcPublishScheduled);
            //Assert.AreEqual("", output.UtcUnpublishScheduled);
        }

        [Test]
        public void CmsMapping_ContentEditorModel_To_TypedEntity()
        {
         
            var input = CreateContentEditorModel<ContentEditorModel>();
            input.UtcModified = DateTime.Now;
            input.UtcCreated = DateTime.Now;
            var output = _cmsModelMapper.Map<TypedEntity>(input);

            AssertContentEditorModelBaseToTypedEntity(input, output);
        }

        [Test]
        public void CmsMapping_UserGroupEditorModel_To_UserGroup()
        {

            var input = CreateContentEditorModel<UserGroupEditorModel>(NodeNameAttributeDefinition.AliasValue, UserGroupSchema.UsersAlias);
            input.UtcModified = DateTime.Now;
            input.UtcCreated = DateTime.Now;
            var output = _cmsModelMapper.Map<UserGroup>(input);

            AssertContentEditorModelBaseToTypedEntity(input, output);

            Assert.AreEqual(null, output.Name);
        }

        [Test]
        public void CmsMapping_UserEditorModel_To_User()
        {

            var input = CreateContentEditorModel<UserEditorModel>(NodeNameAttributeDefinition.AliasValue, UserSchema.ApplicationsAlias, UserSchema.CommentsAlias, UserSchema.EmailAlias, UserSchema.IsApprovedAlias,
                UserSchema.LastActivityDateAlias, UserSchema.LastLoginDateAlias, UserSchema.LastPasswordChangeDateAlias, UserSchema.PasswordAlias,
                UserSchema.PasswordAnswerAlias, UserSchema.PasswordQuestionAlias, UserSchema.PasswordSaltAlias, UserSchema.PersistLoginAlias,
                UserSchema.SchemaAlias, UserSchema.SessionTimeoutAlias, UserSchema.StartContentHiveIdAlias, UserSchema.StartMediaHiveIdAlias, UserSchema.UsernameAlias);
            input.UtcModified = DateTime.Now;
            input.UtcCreated = DateTime.Now;
            var output = _cmsModelMapper.Map<User>(input);

            AssertContentEditorModelBaseToTypedEntity(input, output);

            Assert.AreEqual(null, output.Name);
            Assert.AreEqual(null, output.Email);
            Assert.AreEqual(false, output.IsApproved);
            Assert.AreEqual(default(DateTimeOffset), output.LastActivityDate);
            Assert.AreEqual(default(DateTimeOffset), output.LastLoginDate);
            Assert.AreEqual(default(DateTimeOffset), output.LastPasswordChangeDate);
            Assert.AreEqual(null, output.Password);
            Assert.AreEqual(60, output.SessionTimeout);
            Assert.AreEqual(HiveId.Empty, output.StartContentHiveId);
            Assert.AreEqual(HiveId.Empty, output.StartMediaHiveId);
        }
        
        [Test]
        public void CmsMapping_ContentEditorModel_To_RevisionTypedEntity()
        {
            var input = CreateContentEditorModel<ContentEditorModel>();
            var output = _cmsModelMapper.Map<Revision<TypedEntity>>(input);
            Assert.AreEqual(FixedStatusTypes.Draft, output.MetaData.StatusType);
            Assert.AreEqual(input.Id, output.Item.Id);
        }

        private void AssertContentEditorModelBaseToTypedEntity(BasicContentEditorModel input, TypedEntity output)
        {
            Assert.AreEqual(input.Tabs.Count + 1, output.AttributeGroups.Count()); // + 1 because the mapping will add the general group
            Assert.AreEqual(input.Properties.Count, output.Attributes.Count);
            foreach (var p in input.Properties)
            {
                var mappedAttribute = output.Attributes.Where(x => x.AttributeDefinition.Alias == p.Alias).SingleOrDefault();
                Assert.IsNotNull(mappedAttribute);
                Assert.AreEqual(p.PropertyEditorModel.GetSerializedValue()["Value"], mappedAttribute.DynamicValue);
                //Can't compare Ids because TypedAttribute Ids get auto generated with TryAdd
                //Assert.AreEqual(p.Id, mappedAttribute.Id);
                Assert.AreNotEqual(HiveId.Empty, mappedAttribute.Id);
            }

            Assert.AreEqual(input.DocumentTypeId, output.EntitySchema.Id);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
        }

        #endregion

        #region ContentProperty <-> TypedAttribute
        [Test]
        public void CmsMapping_ContentProperty_MapsTo_TypedAttribute()
        {
            var input = CreateContentProperty();
            var output = _cmsModelMapper.Map<TypedAttribute>(input);
            Assert.AreEqual(input.DocTypeProperty.Alias, output.AttributeDefinition.Alias);
            Assert.AreEqual(input.DocTypePropertyId, output.AttributeDefinition.Id);
            Assert.AreEqual(input.DocTypeProperty.Name, output.AttributeDefinition.Name.ToString());
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
            Assert.AreEqual(input.PropertyEditorModel.Value, output.DynamicValue);
        }

        [Test]
        public void CmsMapping_TypedAttribute_MapsTo_ContentProperty()
        {
            var input = CreateTypedAttribute();
            input.AttributeDefinition.AttributeType.RenderTypeProvider = new TestContentAwarePropertyEditor().Id.ToString();
            input.AttributeDefinition.AttributeType.RenderTypeProviderConfig = new TestContentAwarePropertyEditor().CreatePreValueEditorModel().GetSerializedValue();

            var output = _cmsModelMapper.Map<ContentProperty>(input);
            Assert.AreEqual(input.AttributeDefinition.Alias, output.Alias);
            Assert.AreEqual(input.AttributeDefinition.Id, output.DocTypeProperty.Id);
            Assert.AreEqual(input.AttributeDefinition.Alias, output.DocTypeProperty.Alias);
            Assert.AreEqual(input.AttributeDefinition.Id, output.DocTypePropertyId);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.AttributeDefinition.Name, output.Name);
            Assert.AreEqual(input.DynamicValue, output.PropertyEditorModel.Value);
            Assert.AreEqual(input.AttributeDefinition.Ordinal, output.SortOrder);
            Assert.AreEqual(input.AttributeDefinition.AttributeGroup.Id, output.TabId);
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);

            Assert.IsTrue(output.DocTypeProperty.DataType.PropertyEditor is IContentAwarePropertyEditor);
            Assert.IsTrue(((IContentAwarePropertyEditor)output.DocTypeProperty.DataType.PropertyEditor).IsContentPropertyAvailable);
        }
        #endregion
        
        #region DocumentTypeProperty <-> AttributeDefinition

        [Test]
        public void CmsMapping_DocumentTypeProperty_MapsTo_AttributeDefinition()
        {
            var group = new AttributeGroup { Id = new HiveId(Guid.NewGuid()), Alias = "test" };
                        
            //any tab that is created needs to be added to the internal list so our mocked resolution works
            _createdTabs.Add(group);

            var input = CreateDocumentTypeProperty();
            input.TabId = group.Id;

            var output = _cmsModelMapper.Map<DocumentTypeProperty, AttributeDefinition>(input);

            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.Name, output.Name.Value);
            Assert.AreEqual(input.Description, output.Description.Value);
            Assert.AreEqual(input.SortOrder, output.Ordinal);
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
            Assert.AreEqual(input.TabId.Value.Value, output.AttributeGroup.Id.Value.Value);

        }

        [Test]
        public void CmsMapping_AttributeDefinition_MapsTo_DocumentTypeProperty()
        {
            var input = CreateAttributeDefinition();
            var output = _cmsModelMapper.Map<DocumentTypeProperty>(input);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.AttributeType.Id, output.DataType.Id);
            Assert.AreEqual(input.AttributeType.Name.ToString(), output.DataType.Name);
            Assert.AreEqual(input.AttributeType.Id, output.DataTypeId);
            Assert.AreEqual(input.Description, output.Description);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Name, output.Name);
            Assert.AreEqual(input.RenderTypeProviderConfigOverride, output.PreValueEditor.GetSerializedValue());
            Assert.AreEqual(input.Ordinal, output.SortOrder);
            Assert.AreEqual(input.AttributeGroup.Id, output.TabId);
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);
            Assert.AreEqual(input.UtcModified, output.UtcModified);
        }

        #endregion       

        #region DocumentTypeEditorModel <-> EntitySchema

        [Test]
        public void CmsMapping_DocumentTypeEditorModel_MapsTo_EntitySchema()
        {
            //Arrange 

            var input = CreateDocumentTypeEditorModel();
            //ensure a non-selected item is present
            input.AllowedChildren.ElementAt(0).Selected = false;
            input.AllowedChildren.ElementAt(1).Selected = true;                
            //ensure a non-selected item is present
            input.AllowedTemplates.ElementAt(0).Selected = false;
            input.AllowedTemplates.ElementAt(1).Selected = true;
            //add more tabs than there are properties assigned
            input.DefinedTabs.Add(new Tab {Id = HiveId.ConvertIntToGuid(446), Name = "tab3", Alias = "tab3"});
            input.DefinedTabs.Add(new Tab {Id = HiveId.ConvertIntToGuid(447), Name = "tab4", Alias = "tab4"});
            
            //Act

            var entity = _cmsModelMapper.Map<DocumentTypeEditorModel, EntitySchema>(input);

            //Assert

            Assert.AreEqual(input.Id, entity.Id);
            Assert.AreEqual(input.Alias, entity.Alias);
            Assert.IsTrue(
                input.AllowedChildIds.ContainsAll(entity.GetXmlPropertyAsList<HiveId>("allowed-children")));
            Assert.IsTrue(
                input.AllowedChildren.Select(x => HiveId.Parse(x.Value)).ContainsAll(entity.GetXmlPropertyAsList<HiveId>("allowed-children")));
            Assert.IsTrue(
                input.AllowedTemplates.Select(x => HiveId.Parse(x.Value)).ContainsAll(entity.GetXmlPropertyAsList<HiveId>("allowed-templates")));
            Assert.AreEqual(input.DefaultTemplateId, entity.GetXmlConfigProperty<HiveId>("default-template"));
            Assert.IsTrue(
                entity.AttributeGroups.ToArray().Select(x => x.Id).ContainsAll(input.DefinedTabs.Select(x => x.Id)));
            Assert.AreEqual(input.Description, entity.GetXmlConfigProperty("description"));
            Assert.AreEqual(input.Icon, entity.GetXmlConfigProperty("icon"));
            Assert.AreEqual(input.Name, entity.Name.Value);
            Assert.AreEqual(input.Thumbnail, entity.GetXmlConfigProperty("thumb"));
            Assert.AreEqual(input.UtcCreated, entity.UtcCreated);
            Assert.AreEqual(input.UtcModified, entity.UtcModified);

        }

        [Test]
        public void CmsMapping_AttributionSchemaDefinition_MapsTo_DocumentTypeEditorModel()
        {

            var input = CreateEntitySchema();
            var output = _cmsModelMapper.Map<DocumentTypeEditorModel>(input);

            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.GetXmlPropertyAsList<string>("allowed-children").Count(), output.AllowedChildIds.Count());
            Assert.AreEqual(input.GetXmlPropertyAsList<string>("allowed-children").First(), output.AllowedChildIds.First().ToString());                       
            Assert.AreEqual(input.GetXmlPropertyAsList<string>("allowed-templates").Count(), output.AllowedTemplateIds.Count());
            Assert.AreEqual(input.GetXmlPropertyAsList<string>("allowed-templates").First(), output.AllowedTemplateIds.First().ToString());
            Assert.AreEqual(input.AttributeGroups.Count(), output.AvailableTabs.Count());            
            //BUG: This has no mapping
            //Assert.AreEqual("", output.CreatedBy);
            Assert.AreEqual(input.GetXmlConfigProperty("default-template"), output.DefaultTemplateId.ToString());
            Assert.AreEqual(input.AttributeGroups.Count(), output.DefinedTabs.Count());
            Assert.AreEqual(input.GetXmlConfigProperty("description"), output.Description);
            Assert.AreEqual(input.GetXmlConfigProperty("icon"), output.Icon);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Name.ToString(), output.Name);            
            Assert.AreEqual(input.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType).First().Item.SourceId, output.InheritFromIds.First());
            Assert.AreEqual(input.AttributeDefinitions.Count(), output.Properties.Count());
            Assert.AreEqual(input.GetXmlConfigProperty("thumb"), output.Thumbnail);
            //BUG: This has no mapping
            //Assert.AreEqual("", output.UpdatedBy);
            Assert.AreEqual(input.UtcCreated, output.UtcCreated);            
            Assert.AreEqual(input.UtcModified, output.UtcModified);

        }

        #endregion

        #region Tab <-> AttributeGroup

        [Test]
        public void CmsMapping_Tab_To_AttributeGroup()
        {
            var input = CreateTab();
            var output = _cmsModelMapper.Map<Tab, AttributeGroup>(input);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.Name, output.Name.Value);
            Assert.AreEqual(input.SortOrder, output.Ordinal);
        }

        [Test]
        public void CmsMapping_AttributeGroup_To_Tab()
        {
            var input = CreateAttributeGroup();
            var output = _cmsModelMapper.Map<AttributeGroup, Tab>(input);
            Assert.AreEqual(input.Id, output.Id);
            Assert.AreEqual(input.Alias, output.Alias);
            Assert.AreEqual(input.Name.Value, output.Name);
            Assert.AreEqual(input.Ordinal, output.SortOrder);
        }

        #endregion     
     
        /// <summary>
        /// Creates a new content editor model by mapping from a document type editor model, this requires mocking
        /// some of Hive to return the correct data during the map
        /// </summary>
        /// <returns></returns>
        private TContent CreateContentEditorModel<TContent>(params string[] propertyAliases)
            where TContent : BasicContentEditorModel
        {
            var docType = CreateDocumentTypeEditorModel(propertyAliases);

            //any tab that is created needs to be added to the internal list so our mocked resolution works
            foreach (var t in docType.DefinedTabs)
                _createdTabs.Add(_cmsModelMapper.Map<AttributeGroup>(t));

            var schema = _cmsModelMapper.Map<EntitySchema>(docType);

            //need to stub the repo to return the schema for mapping to resolve it
            _readonlySchemaSession.Get<EntitySchema>(Arg.Any<bool>(), Arg.Any<HiveId[]>())
                .ReturnsForAnyArgs(new[] {schema});

            return _cmsModelMapper.Map<TContent>(docType);
        }

        private ContentProperty CreateContentProperty(Tab tab = null)
        {
            if (tab == null) 
                tab = new Tab {Alias = "test1", Id = new HiveId(Guid.NewGuid()), Name = "Test1", SortOrder = 0};

            //any tab that is created needs to be added to the internal list so our mocked resolution works
            _createdTabs.Add(_cmsModelMapper.Map<AttributeGroup>(tab));
            
            return new ContentProperty(
                new HiveId(Guid.NewGuid()),
                CreateDocumentTypeProperty(tab: tab), Guid.NewGuid().ToString("N"))
                {
                    Alias = "test" + Guid.NewGuid().ToString("N"),
                    Name = "Test" + Guid.NewGuid().ToString("N"),
                    SortOrder = new Random().Next(0, 1000),
                    TabId = tab.Id,
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now
                };
        }

        private static DataTypeEditorModel CreateDataTypeEditorModel()
        {
            var propertyEditor = new MandatoryPropertyEditor();
            
            return new DataTypeEditorModel(new HiveId(Guid.NewGuid()))
            {
                Name = "Test" + Guid.NewGuid().ToString("N"),                
                CreatedBy = "Shannon" + Guid.NewGuid().ToString("N"),
                ParentId = new HiveId(Guid.NewGuid()),
                UtcCreated = DateTime.Now,
                UtcModified = DateTime.Now,
                PreValueEditorModel = propertyEditor.CreatePreValueEditorModel(),
                PropertyEditorId = propertyEditor.Id,
                UpdatedBy = "Shannon" + Guid.NewGuid().ToString("N")
            };
        }

        private static DataType CreateDataType(PropertyEditor propertyEditor = null)
        {
            if (propertyEditor == null)
            {
                propertyEditor = new MandatoryPropertyEditor();
            }
            return new DataType(new HiveId(Guid.NewGuid()), "Test" + Guid.NewGuid().ToString("N"), "test" + Guid.NewGuid().ToString("N"), propertyEditor, "")
                {
                    CreatedBy = "Shannon" + Guid.NewGuid().ToString("N"),
                    ParentId = new HiveId(Guid.NewGuid()),
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now
                };
        }

        private static DocumentTypeProperty CreateDocumentTypeProperty(
            PropertyEditor propertyEditor = null, 
            Tab tab = null,
            string alias = "")
        {
            if (tab == null) 
                tab = new Tab() {Alias = "tab" + Guid.NewGuid().ToString("N"), Id = new HiveId(Guid.NewGuid()), Name = "Tab" + Guid.NewGuid().ToString("N")};
            if (alias == "")
            {
                alias = "test" + Guid.NewGuid().ToString("N");
            }

            return new DocumentTypeProperty(
                new HiveId(Guid.NewGuid()),
                CreateDataType(propertyEditor))
                {
                    Alias = alias,
                    Description = "my description" + Guid.NewGuid().ToString("N"),
                    Name = "Test" + Guid.NewGuid().ToString("N"),
                    SortOrder = 10,
                    TabId = tab.Id,
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now
                };
        }

        private static Tab CreateTab()
        {
            return new Tab
                {
                    Alias = "test" + Guid.NewGuid().ToString("N"),
                    Id = new HiveId(Guid.NewGuid()),
                    Name = "Test" + Guid.NewGuid().ToString("N"),
                    SortOrder = new Random().Next(0, 1000)
                };
        }

        private static DocumentTypeEditorModel CreateDocumentTypeEditorModel(params string[] propertyAliases)
        {
            var allowedChildIds = new[] {new HiveId(Guid.NewGuid()), new HiveId(Guid.NewGuid())};
            var allowedTemplateIds = new[] {new HiveId(Guid.NewGuid()), new HiveId(Guid.NewGuid())};
            var tabs = new HashSet<Tab>(new[] {CreateTab(), CreateTab()});
            var d =  new DocumentTypeEditorModel(new HiveId(Guid.NewGuid()))
                {
                    ActiveTabIndex = new Random().Next(1, 1000),
                    Alias = "test" + Guid.NewGuid().ToString("N"),
                    AllowedChildIds = allowedChildIds,
                    AllowedChildren = allowedChildIds.Select(x => new SelectListItem {Text = x.ToString(), Value = x.ToString()}),
                    AllowedTemplateIds = new HashSet<HiveId>(allowedTemplateIds),
                    AllowedTemplates = allowedTemplateIds.Select(x => new SelectListItem {Text = x.ToString(), Value = x.ToString()}),
                    AvailableDataTypes = new[] {CreateDataType(), CreateDataType()}.Select(x => new SelectListItem() {Text = x.Name, Value = x.Id.ToString()}),
                    AvailableIcons = new[] {"icon1", "icon2"}.Select(x => new SelectListItem() {Text = x, Value = x}),
                    AvailableTabs = new[] {new HiveId(Guid.NewGuid()), new HiveId(Guid.NewGuid())}.Select(x => new SelectListItem() {Text = x.ToString(), Value = x.ToString()}),
                    AvailableThumbnails = new[] {"thumb1", "thumb2"}.Select(x => new SelectListItem() {Text = x, Value = x}),
                    CreatedBy = Guid.NewGuid().ToString("N"),
                    DefaultTemplateId = allowedTemplateIds.First(),
                    DefinedTabs = tabs,
                    Description = Guid.NewGuid().ToString("N"),
                    Icon = "icon1",
                    Id = new HiveId(Guid.NewGuid()),
                    IsCreatingNewProperty = false,
                    Name = Guid.NewGuid().ToString("N"),
                    ParentId = new HiveId(Guid.NewGuid()),
                    Thumbnail = "thumbnail1",
                    UpdatedBy = Guid.NewGuid().ToString("N"),
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now,
                    Properties = new HashSet<DocumentTypeProperty>(new[]
                        {
                            CreateDocumentTypeProperty(tab: tabs.First()), 
                            CreateDocumentTypeProperty(new TestContentAwarePropertyEditor(), tabs.Last())
                        })
                };

            //create additional properties if specified
            foreach (var s in propertyAliases)
            {
                d.Properties.Add(CreateDocumentTypeProperty(alias: s, tab: tabs.Last()));
            }
            return d;
        }

        private static AttributeDefinition CreateAttributeDefinition(AttributeGroup group = null, string alias = null)
        {
            if (group == null) group = CreateAttributeGroup();
            if (alias == null) alias = "test" + Guid.NewGuid();

            return new AttributeDefinition(alias, "Test" + Guid.NewGuid())
                {
                    AttributeGroup = group,
                    AttributeType = CreateAttributeType(),
                    Description = "testing" + Guid.NewGuid(),
                    Ordinal = new Random().Next(0, 1000),
                    Id = new HiveId(Guid.NewGuid()),
                    RenderTypeProviderConfigOverride = new MandatoryPropertyEditor().CreatePreValueEditorModel().GetSerializedValue(),
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now,
                    UtcStatusChanged = DateTime.Now
                };
        }

        private static TypedAttribute CreateTypedAttribute()
        {
            return new TypedAttribute(CreateAttributeDefinition(), "hello" + Guid.NewGuid())
                {
                    Id = new HiveId(Guid.NewGuid()),
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now,
                    UtcStatusChanged = DateTime.Now
                };
        }

        private static AttributeGroup CreateAttributeGroup()
        {
            return new AttributeGroup()
            {
                Id = new HiveId(Guid.NewGuid()),
                Alias = "test",
                Name = "Test",
                Ordinal = new Random().Next(0, 1000)
            };
        }

        private static AttributeType CreateAttributeType()
        {
            return new AttributeType("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), "description " + Guid.NewGuid(), new StringSerializationType())
                {
                    Id = new HiveId(Guid.NewGuid()),
                    Ordinal = new Random().Next(0, 1000),
                    RenderTypeProvider = new MandatoryPropertyEditor().Id.ToString(),
                    RenderTypeProviderConfig = new MandatoryPropertyEditor().CreatePreValueEditorModel().GetSerializedValue(),
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now,
                    UtcStatusChanged = DateTime.Now
                };
        }

        private static EntitySchema CreateEntitySchema(bool createRelations = true)
        {
            var schema = new EntitySchema("test" + Guid.NewGuid(), "Test" + Guid.NewGuid())
                {
                    Id = new HiveId(Guid.NewGuid()),
                    SchemaType = "Test" + Guid.NewGuid(),
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now,
                    UtcStatusChanged = DateTime.Now                    
                };

            var allowedTemplates = new[] {new HiveId(Guid.NewGuid()).ToString(), new HiveId(Guid.NewGuid()).ToString()}.ToArray();
            schema.SetXmlConfigProperty("allowed-templates", allowedTemplates);
            schema.SetXmlConfigProperty("allowed-children", new[] { new HiveId(Guid.NewGuid()).ToString(), new HiveId(Guid.NewGuid()).ToString() }.ToArray());
            schema.SetXmlConfigProperty("description", "testing" + Guid.NewGuid());
            schema.SetXmlConfigProperty("thumb", "thumb" + Guid.NewGuid());
            schema.SetXmlConfigProperty("icon", "icon" + Guid.NewGuid());
            schema.SetXmlConfigProperty("default-template", allowedTemplates.First());

            schema.AttributeGroups.Add(CreateAttributeGroup());
            schema.AttributeGroups.Add(CreateAttributeGroup());

            //ensure there's at least one content aware property
            var contentAwareProp = CreateAttributeDefinition(schema.AttributeGroups.First());
            contentAwareProp.AttributeType.RenderTypeProvider = new TestContentAwarePropertyEditor().Id.ToString();
            contentAwareProp.AttributeType.RenderTypeProviderConfig = new TestContentAwarePropertyEditor().CreatePreValueEditorModel().GetSerializedValue();
            schema.AttributeDefinitions.Add(contentAwareProp);
            
            schema.AttributeDefinitions.Add(CreateAttributeDefinition(schema.AttributeGroups.Last()));

            if (createRelations)
            {
                schema.RelationProxies.EnlistParent(CreateEntitySchema(false), FixedRelationTypes.DefaultRelationType);    
            }
            
            
            return schema;
        }

        private static TypedEntity CreateTypedEntity<TEntity>(bool createRelations = true, params string[] propertyAliases)
            where TEntity: TypedEntity, new()
        {
            var schema = CreateEntitySchema(false);
            var nodeName = new NodeNameAttributeDefinition(FixedGroupDefinitions.GeneralGroup)
                {
                    AttributeType =
                        {
                            RenderTypeProvider = new NodeNameEditor(null).Id.ToString()
                        }
                };
            schema.AttributeDefinitions.Add(nodeName);
            foreach(var a in propertyAliases)
            {
                schema.AttributeDefinitions.Add(CreateAttributeDefinition(alias: a));
            }
            var e = new TEntity()
                {
                    EntitySchema = schema,
                    Id = new HiveId(Guid.NewGuid()),
                    UtcCreated = DateTime.Now,
                    UtcModified = DateTime.Now,
                    UtcStatusChanged = DateTime.Now
                };
            e.SetupFromSchema();            
            if (createRelations)
            {
                e.RelationProxies.EnlistParent(CreateTypedEntity<TEntity>(false), FixedRelationTypes.DefaultRelationType);
                e.RelationProxies.EnlistChild(CreateTypedEntity<TEntity>(false), FixedRelationTypes.DefaultRelationType);    
            }
            
            return e;
        }

    }
}
