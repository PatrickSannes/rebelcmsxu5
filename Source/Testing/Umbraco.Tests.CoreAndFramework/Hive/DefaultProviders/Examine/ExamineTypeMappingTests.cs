using System;
using System.Diagnostics;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Examine;
using Umbraco.Framework.Persistence.Examine.Mapping;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Examine
{
    [TestFixture]
    public class ExamineTypeMappingTests
    {
        private ExamineTestSetupHelper _examineTestSetupHelper;
        private ExamineModelMapper _mapper;
        private IAttributeTypeRegistry _attributeTypeRegistry = new CmsAttributeTypeRegistry();

        private readonly AttributeType _stringAttType = new AttributeType("text", "Text", "Text field", new StringSerializationType());

        [TestFixtureSetUp]
        public void Setup()
        {
            _examineTestSetupHelper = new ExamineTestSetupHelper();
            _mapper = new ExamineModelMapper(_examineTestSetupHelper.ExamineHelper, _examineTestSetupHelper.FrameworkContext);
            _mapper.ConfigureMappings();
        }

        [TestFixtureTearDown]
        public void CleanUp()
        {
            _examineTestSetupHelper.Dispose();
        }

        private TypedEntity CreateTypedEntity()
        {
            var entity = new TypedEntity
            {
                EntitySchema = new EntitySchema("schema", "Schema") { SchemaType = "Content", Id = new HiveId(Guid.NewGuid()) },
                Id = new HiveId(Guid.NewGuid()),
                UtcCreated = DateTimeOffset.UtcNow,
                UtcModified = DateTimeOffset.UtcNow,
                UtcStatusChanged = DateTimeOffset.UtcNow
            };
            SetupTypedEntity(entity);
            return entity;
        }

        private void SetupTypedEntity(TypedEntity entity)
        {

            entity.EntitySchema.AttributeGroups.Add(new AttributeGroup("tab1", "Tab 1", 0));
            entity.EntitySchema.AttributeGroups.Add(new AttributeGroup("tab2", "Tab 2", 1));
            entity.EntitySchema.AttributeGroups.Add(new AttributeGroup("tab3", "Tab 3", 2));
            entity.EntitySchema.AttributeDefinitions.Add(new AttributeDefinition("property1", "Property 1") { Description = "property1", AttributeGroup = entity.EntitySchema.AttributeGroups.ElementAt(0), AttributeType = _stringAttType });
            entity.EntitySchema.AttributeDefinitions.Add(new AttributeDefinition("property2", "Property 2") { Description = "property2", AttributeGroup = entity.EntitySchema.AttributeGroups.ElementAt(1), AttributeType = _stringAttType });
            entity.EntitySchema.AttributeDefinitions.Add(new AttributeDefinition("property3", "Property 3") { Description = "property3", AttributeGroup = entity.EntitySchema.AttributeGroups.ElementAt(2), AttributeType = _stringAttType });

            entity.Attributes.Add(new TypedAttribute(entity.EntitySchema.AttributeDefinitions.ElementAt(0), "Value 1"));
            entity.Attributes.Add(new TypedAttribute(entity.EntitySchema.AttributeDefinitions.ElementAt(1), "Value 2"));
            entity.Attributes.Add(new TypedAttribute(entity.EntitySchema.AttributeDefinitions.ElementAt(2), "Value 3"));

        }

        private void VerifyIEntityFields(NestedHiveIndexOperation result, IEntity entity)
        {
            Assert.AreEqual(entity.UtcCreated.UtcDateTime, result.Fields["UtcCreated"].FieldValue);

            Assert.AreEqual(entity.UtcStatusChanged.UtcDateTime, result.Fields["UtcStatusChanged"].FieldValue);
        }

        //[Test]
        //public void Relations_Retain_Ordinal()
        //{
        //    var entity = CreateTypedEntity();
        //    var entity2 = CreateTypedEntity();
        //    var entity3 = CreateTypedEntity();
        //    var entity4 = CreateTypedEntity();
        //    entity.RelationProxies.EnlistChild(entity2, FixedRelationTypes.ContentTreeRelationType, 0);
        //    entity.RelationProxies.EnlistChild(entity3, FixedRelationTypes.ContentTreeRelationType, 1);
        //    entity.RelationProxies.EnlistChild(entity4, FixedRelationTypes.ContentTreeRelationType, 2);

        //    var output = _mapper.Map<NestedHiveIndexOperation>(entity);
            
        //    Assert.AreEqual(0, output.SubIndexOperations.Single(x => x.Entity is IRelationById).Fields[FixedIndexedFields.Ordinal]);
        //    Assert.AreEqual(1, output.SubIndexOperations.Single(x => x.Entity is IRelationById).Fields[FixedIndexedFields.Ordinal]);
        //    Assert.AreEqual(2, output.SubIndexOperations.Single(x => x.Entity is IRelationById).Fields[FixedIndexedFields.Ordinal]);
        //}

        [Test]
        public void Typed_Entity_With_Relations_To_IndexOperation()
        {
            //Arrange

            var entity = CreateTypedEntity();
            var parentEntity = CreateTypedEntity();
            //reset the ids to null, we want to ensure the ids still map lazily after mapping
            entity.Id = HiveId.Empty;
            parentEntity.Id = HiveId.Empty;
            //set up a relation
            entity.RelationProxies.EnlistParent(parentEntity, FixedRelationTypes.DefaultRelationType);

            //Act

            var result = _mapper.Map<TypedEntity, NestedHiveIndexOperation>(entity);
            //now set the ids back
            entity.Id = new HiveId(Guid.NewGuid());
            parentEntity.Id = new HiveId(Guid.NewGuid());

            //Assert

            var relationOp = result.SubIndexOperations.Where(x => x.Entity is Relation).SingleOrDefault();
            Assert.IsNotNull(relationOp);
            Assert.AreEqual(entity.Id.Value.ToString(), relationOp.Fields[FixedRelationIndexFields.DestinationId].FieldValue);
            Assert.AreEqual(parentEntity.Id.Value.ToString(), relationOp.Fields[FixedRelationIndexFields.SourceId].FieldValue);
            Assert.AreEqual(FixedRelationTypes.DefaultRelationType.RelationName, relationOp.Fields[FixedRelationIndexFields.RelationType].FieldValue);

        }

        [Test]
        public void From_Inherited_AttributeType_To_IndexOperation()
        {
            //Arrange

            var attributeType = (StringAttributeType)_attributeTypeRegistry.GetAttributeType(StringAttributeType.AliasValue);

            //Act

            var result = _mapper.Map<StringAttributeType, NestedHiveIndexOperation>(attributeType);

            //Assert

            Assert.AreEqual("AttributeType", result.ItemCategory);
            Assert.AreEqual(IndexOperationType.Add, result.OperationType);
            Assert.AreEqual(attributeType.Id.Value.ToString(), result.Id.Value);
            Assert.AreEqual(attributeType.Alias, result.Fields["Alias"].FieldValue);
            Assert.AreEqual(attributeType.Name, result.Fields["Name"].FieldValue);
            Assert.AreEqual(attributeType.Description, result.Fields["Description"].FieldValue);
            Assert.AreEqual(attributeType.Ordinal, result.Fields["Ordinal"].FieldValue);

            VerifyIEntityFields(result, attributeType);

        }

        [Test]
        public void From_AttributeType_To_IndexOperation()
        {
            //Arrange

            var attributeType = new AttributeType("test", "Test", "This is a test", new StringSerializationType())
                {
                    Id = HiveId.ConvertIntToGuid(123),
                    RenderTypeProvider = Guid.NewGuid().ToString(),
                    RenderTypeProviderConfig = "<some><xml/><structure/></some>"
                };

            //Act

            var result = _mapper.Map<AttributeType, NestedHiveIndexOperation>(attributeType);

            //Assert

            Assert.AreEqual("AttributeType", result.ItemCategory);
            Assert.AreEqual(IndexOperationType.Add, result.OperationType);
            Assert.AreEqual(attributeType.Id.Value.ToString(), result.Id.Value);
            Assert.AreEqual(attributeType.RenderTypeProvider, result.Fields["RenderTypeProvider"].FieldValue);
            Assert.AreEqual(attributeType.Alias, result.Fields["Alias"].FieldValue);
            Assert.AreEqual(attributeType.Name, result.Fields["Name"].FieldValue);
            Assert.AreEqual(attributeType.Description, result.Fields["Description"].FieldValue);
            Assert.AreEqual(attributeType.Ordinal, result.Fields["Ordinal"].FieldValue);
            Assert.AreEqual(attributeType.RenderTypeProviderConfig, result.Fields["RenderTypeProviderConfig"].FieldValue);
            VerifyIEntityFields(result, attributeType);
        }

        [Test]
        public void From_Inherited_TypedEntity_To_IndexOperation()
        {
            //Arrange

            var systemRoot = new SystemRoot();


            //Act

            var result1 = _mapper.Map<SystemRoot, NestedHiveIndexOperation>(systemRoot);
            var result2 = _mapper.Map<TypedEntity, NestedHiveIndexOperation>(systemRoot);

            //Assert

            Assert.AreEqual("TypedEntity", result1.ItemCategory);
            Assert.AreEqual(systemRoot.EntitySchema.SchemaType, result1.Fields[FixedIndexedFields.SchemaType].FieldValue);
            Assert.AreEqual(IndexOperationType.Add, result1.OperationType);
            Assert.AreEqual(systemRoot.Id.Value.ToString(), result1.Id.Value);
            Assert.AreEqual(systemRoot.Id.Value.ToString(), result1.Fields[FixedIndexedFields.EntityId].FieldValue);
            Assert.AreEqual(systemRoot.EntitySchema.Alias, result1.Fields[FixedIndexedFields.SchemaAlias].FieldValue);
            Assert.AreEqual(systemRoot.EntitySchema.Name, result1.Fields[FixedIndexedFields.SchemaName].FieldValue);
            Assert.AreEqual(systemRoot.EntitySchema.Id.Value.ToString(), result1.Fields[FixedIndexedFields.SchemaId].FieldValue);

            VerifyIEntityFields(result1, systemRoot);
        }

        [Test]
        public void From_Revision_TypedEntity_To_IndexOperation()
        {
            var revision = new Revision<TypedEntity>(CreateTypedEntity());
            revision.MetaData.StatusType = FixedStatusTypes.Published;
            revision.Item.Id = new HiveId(Guid.NewGuid());

            //Act

            var result = _mapper.Map<Revision<TypedEntity>, NestedHiveIndexOperation>(revision);

            //Assert

            Assert.AreEqual(revision.Item.Id.Value.ToString() + "," + revision.MetaData.Id.Value.ToString(), result.Id.Value);
            Assert.AreEqual(revision.MetaData.StatusType.Alias, result.Fields[FixedRevisionIndexFields.RevisionStatusAlias].FieldValue);
            Assert.AreEqual(revision.MetaData.StatusType.Id.Value.ToString(), result.Fields[FixedRevisionIndexFields.RevisionStatusId].FieldValue);
            Assert.AreEqual(revision.MetaData.Id.Value.ToString(), result.Fields[FixedRevisionIndexFields.RevisionId].FieldValue);
            Assert.AreEqual(1, result.Fields[FixedRevisionIndexFields.IsLatest].FieldValue);
            Assert.AreEqual("TypedEntity", result.ItemCategory);
            Assert.AreEqual(revision.Item.EntitySchema.SchemaType, result.Fields[FixedIndexedFields.SchemaType].FieldValue);
            Assert.AreEqual(IndexOperationType.Add, result.OperationType);
            Assert.AreEqual(revision.Item.EntitySchema.Alias, result.Fields[FixedIndexedFields.SchemaAlias].FieldValue);
            Assert.AreEqual(revision.Item.EntitySchema.Name, result.Fields[FixedIndexedFields.SchemaName].FieldValue);
            Assert.AreEqual(revision.Item.EntitySchema.Id.Value.ToString(), result.Fields[FixedIndexedFields.SchemaId].FieldValue);

            Assert.AreEqual(revision.Item.Attributes.Count(), result.Fields.Count(x =>
                x.Key.StartsWith(FixedAttributeIndexFields.AttributePrefix)
                && x.Key.EndsWith(FixedAttributeIndexFields.AttributeAlias)));

            Assert.AreEqual(revision.Item.Attributes.ElementAt(0).DynamicValue.ToString(), result.Fields[FixedAttributeIndexFields.AttributePrefix + revision.Item.Attributes.ElementAt(0).AttributeDefinition.Alias].FieldValue);
            Assert.AreEqual(revision.Item.Attributes.ElementAt(1).DynamicValue.ToString(), result.Fields[FixedAttributeIndexFields.AttributePrefix + revision.Item.Attributes.ElementAt(1).AttributeDefinition.Alias].FieldValue);
            Assert.AreEqual(revision.Item.Attributes.ElementAt(2).DynamicValue.ToString(), result.Fields[FixedAttributeIndexFields.AttributePrefix + revision.Item.Attributes.ElementAt(2).AttributeDefinition.Alias].FieldValue);

            VerifyIEntityFields(result, revision.Item);

        }

        [Test]
        public void From_TypedEntity_To_IndexOperation()
        {
            //Arrange

            var entity = CreateTypedEntity();
            var parentEntity = CreateTypedEntity();
            //set up a relation
            entity.RelationProxies.EnlistParent(parentEntity, FixedRelationTypes.DefaultRelationType);

            //Act

            var result = _mapper.Map<TypedEntity, NestedHiveIndexOperation>(entity);

            //Assert

            Assert.AreEqual("TypedEntity", result.ItemCategory);
            Assert.AreEqual(entity.EntitySchema.SchemaType, result.Fields[FixedIndexedFields.SchemaType].FieldValue);
            Assert.AreEqual(IndexOperationType.Add, result.OperationType);
            Assert.AreEqual(entity.Id.Value.ToString(), result.Id.Value);
            Assert.AreEqual(entity.EntitySchema.Alias, result.Fields[FixedIndexedFields.SchemaAlias].FieldValue);
            Assert.AreEqual(entity.EntitySchema.Name, result.Fields[FixedIndexedFields.SchemaName].FieldValue);
            Assert.AreEqual(parentEntity.Id.Value.ToString(), result.Fields[FixedIndexedFields.ParentId].FieldValue);

            Assert.AreEqual(entity.Attributes.Count(), result.Fields.Count(x =>
                x.Key.StartsWith(FixedAttributeIndexFields.AttributePrefix)
                && x.Key.EndsWith(FixedAttributeIndexFields.AttributeAlias)));

            Assert.AreEqual(entity.Attributes.ElementAt(0).DynamicValue.ToString(), result.Fields[FixedAttributeIndexFields.AttributePrefix + entity.Attributes.ElementAt(0).AttributeDefinition.Alias].FieldValue);
            Assert.AreEqual(entity.Attributes.ElementAt(1).DynamicValue.ToString(), result.Fields[FixedAttributeIndexFields.AttributePrefix + entity.Attributes.ElementAt(1).AttributeDefinition.Alias].FieldValue);
            Assert.AreEqual(entity.Attributes.ElementAt(2).DynamicValue.ToString(), result.Fields[FixedAttributeIndexFields.AttributePrefix + entity.Attributes.ElementAt(2).AttributeDefinition.Alias].FieldValue);

            VerifyIEntityFields(result, entity);
        }

        [Test]
        public void From_EntitySchema_To_IndexOperation()
        {
            //Arrange

            var input = HiveModelCreationHelper.CreateEntitySchema("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), true,
                                                                    HiveModelCreationHelper.CreateAttributeDefinition("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), "this is a test" + Guid.NewGuid(),
                                                                                                                      HiveModelCreationHelper.CreateAttributeType("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), "this is a test" + Guid.NewGuid()),
                                                                                                                      HiveModelCreationHelper.CreateAttributeGroup("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), new Random().Next(0, 1000), true), true));
            var child = HiveModelCreationHelper.CreateEntitySchema("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), true,
                                                                    HiveModelCreationHelper.CreateAttributeDefinition("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), "this is a test" + Guid.NewGuid(),
                                                                                                                      HiveModelCreationHelper.CreateAttributeType("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), "this is a test" + Guid.NewGuid()),
                                                                                                                      HiveModelCreationHelper.CreateAttributeGroup("test" + Guid.NewGuid(), "Test" + Guid.NewGuid(), new Random().Next(0, 1000), true), true));
            input.RelationProxies.EnlistChild(child, FixedRelationTypes.DefaultRelationType);

            //Act

            var output = _mapper.Map<EntitySchema, NestedHiveIndexOperation>(input);
            //lazily add the ids
            ExamineHelper.EnsureIds(output);

            //Assert

            Assert.AreEqual(typeof(EntitySchema).Name, output.ItemCategory);
            Assert.AreEqual(IndexOperationType.Add, output.OperationType);
            Assert.AreEqual(input.Id.Value.ToString(), output.Id.Value);
            Assert.AreEqual(input.SchemaType, output.Fields["SchemaType"].FieldValue);
            Assert.AreEqual(
                input.AttributeDefinitions.Count() +
                input.AttributeGroups.Count() +
                input.RelationProxies.AllChildRelations().Count() + 1, output.SubIndexOperations.Count());

            Assert.AreEqual(input, output.Entity);
            var fieldMapper = new EntitySchemaToIndexFields();
            Assert.AreEqual(fieldMapper.GetValue(input).Count(), output.Fields.Count());

            VerifyIEntityFields(output, input);

            foreach (var a in output.SubIndexOperations.Where(x => x.Entity is AttributeDefinition).ToArray())
            {
                Assert.IsFalse(((HiveIdValue)a.Fields[FixedIndexedFields.GroupId].FieldValue) == HiveIdValue.Empty);
            }

        }

        [Test]
        public void From_AttributeDefinition_To_IndexOperation()
        {
            //Arrange

            var entity = CreateTypedEntity();
            var attDef = entity.EntitySchema.AttributeDefinitions.First();

            //Act

            var result = _mapper.Map<AttributeDefinition, NestedHiveIndexOperation>(attDef);
            //lazily add the ids
            ExamineHelper.EnsureIds(result);

            //Assert

            Assert.AreEqual("AttributeDefinition", result.ItemCategory);
            Assert.AreEqual(IndexOperationType.Add, result.OperationType);
            Assert.AreEqual(attDef.Id.Value.ToString(), result.Id.Value);
            Assert.AreEqual(attDef.Alias, result.Fields["Alias"].FieldValue);
            Assert.AreEqual(attDef.Name, result.Fields["Name"].FieldValue);
            Assert.AreEqual(attDef.Description, result.Fields["Description"].FieldValue);
            Assert.AreEqual(attDef.Ordinal, result.Fields["Ordinal"].FieldValue);
            Assert.IsNotNull(result.Fields[FixedIndexedFields.GroupId].FieldValue);
            Assert.AreEqual(attDef.AttributeGroup.Id.Value, result.Fields[FixedIndexedFields.GroupId].FieldValue);
            Assert.IsNotNull(result.Fields[FixedIndexedFields.AttributeTypeId].FieldValue);
            Assert.AreEqual(attDef.AttributeType.Id.Value, result.Fields[FixedIndexedFields.AttributeTypeId].FieldValue);
            //NOTE: Groups is not a sub operation because a group is attached to a schema, not definition
            Assert.AreEqual(1, result.SubIndexOperations.Count);

            VerifyIEntityFields(result, attDef);

        }

        //[Test]
        //public void From_Search_Result_To_Typed_Entity()
        //{
        //    //Arrange

        //    ////we need to commit an EntitySchema/Relations first since our resolver looks it up...
        //    var newEntity = CreateTypedEntity();
        //    var parentEntity = CreateTypedEntity();
        //    var childEntity = CreateTypedEntity();
        //    //add some relations
        //    newEntity.RelationProxies.EnlistParent(parentEntity, FixedRelationTypes.ContentTreeRelationType);
        //    newEntity.RelationProxies.EnlistChild(childEntity, FixedRelationTypes.ContentTreeRelationType);
        //    var trans = new ExamineTransaction(_examineTestSetupHelper.ExamineManager, new ProviderMetadata("test", new Uri("content://"), true, false), _examineTestSetupHelper.FrameworkContext);
        //    _examineTestSetupHelper.ExamineHelper.PerformAddOrUpdate(newEntity, trans);
        //    _examineTestSetupHelper.ExamineHelper.PerformAddOrUpdate(parentEntity, trans);
        //    _examineTestSetupHelper.ExamineHelper.PerformAddOrUpdate(childEntity, trans);
        //    trans.ExecuteCommit();


        //    //manually create the search result
        //    var result = new SearchResult
        //        {
        //            Id = newEntity.Id.ToString(),
        //            Score = 10
        //        };
        //    result.Fields.Add(LuceneIndexer.IndexCategoryFieldName, typeof(TypedEntity).Name);
        //    result.Fields.Add(FixedIndexedFields.SchemaId, newEntity.EntitySchema.Id.ToString());
        //    result.Fields.Add(FixedIndexedFields.SchemaType, newEntity.EntitySchema.SchemaType);
        //    result.Fields.Add(FixedIndexedFields.SchemaAlias, newEntity.EntitySchema.Alias);
        //    result.Fields.Add(FixedIndexedFields.SchemaName, newEntity.EntitySchema.Name);
        //    result.Fields.Add("UtcCreated", DateTools.DateToString(newEntity.UtcCreated.UtcDateTime, DateTools.Resolution.MILLISECOND));
        //    result.Fields.Add("UtcModified", DateTools.DateToString(newEntity.UtcModified.UtcDateTime, DateTools.Resolution.MILLISECOND));
        //    result.Fields.Add("UtcStatusChanged", DateTools.DateToString(newEntity.UtcStatusChanged.UtcDateTime, DateTools.Resolution.MILLISECOND));
        //    foreach (var a in newEntity.EntitySchema.AttributeDefinitions)
        //    {
        //        //add the alias, name, id
        //        result.Fields.Add(FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix + a.Alias + "." + FixedIndexedFields.FixedAttributeIndexFields.AttributeAlias, a.Alias);
        //        result.Fields.Add(FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix + a.Alias + "." + FixedIndexedFields.FixedAttributeIndexFields.AttributeName, a.Name);
        //        result.Fields.Add(FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix + a.Alias + "." + FixedIndexedFields.FixedAttributeIndexFields.AttributeId, a.Id.ToString());
        //        //add the value
        //        result.Fields.Add(FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix + a.Alias, "Some body text");
        //    }


        //    //Act

        //    var mappedEntity = _examineHive.FrameworkContext.TypeMappers.Map<SearchResult, TypedEntity>(result);

        //    //Assert

        //    Assert.AreEqual(result.Id, mappedEntity.Id.ToString());
        //    Assert.AreEqual(result.Fields[FixedIndexedFields.SchemaType], mappedEntity.EntitySchema.SchemaType);
        //    Assert.AreEqual(result.Fields[FixedIndexedFields.SchemaAlias], mappedEntity.EntitySchema.Alias);
        //    Assert.AreEqual(result.Fields[FixedIndexedFields.SchemaName], mappedEntity.EntitySchema.Name.Value);
        //    Assert.AreEqual(result.Fields.Count(
        //        x => x.Key.StartsWith(FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix)
        //        && x.Key.EndsWith(FixedIndexedFields.FixedAttributeIndexFields.AttributeAlias)),
        //        mappedEntity.Attributes.Count);
        //    foreach (var a in newEntity.EntitySchema.AttributeDefinitions)
        //    {
        //        Assert.AreEqual(result.Fields[FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix + a.Alias + "." + FixedIndexedFields.FixedAttributeIndexFields.AttributeAlias], mappedEntity.Attributes[a.Alias].AttributeDefinition.Alias);
        //        Assert.AreEqual(result.Fields[FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix + a.Alias + "." + FixedIndexedFields.FixedAttributeIndexFields.AttributeName], mappedEntity.Attributes[a.Alias].AttributeDefinition.Name.Value);
        //        Assert.AreEqual(result.Fields[FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix + a.Alias + "." + FixedIndexedFields.FixedAttributeIndexFields.AttributeId], mappedEntity.Attributes[a.Alias].AttributeDefinition.Id.ToString());
        //        Assert.AreEqual(result.Fields[FixedIndexedFields.FixedAttributeIndexFields.AttributePrefix + a.Alias], mappedEntity.Attributes[a.Alias].DynamicValue);
        //    }
        //    Assert.AreEqual(newEntity.EntitySchema.AttributeGroups.Count(), mappedEntity.AttributeGroups.Count());
        //    Assert.AreEqual(newEntity.AttributeGroups.Count(), mappedEntity.AttributeGroups.Count());
        //    Assert.AreEqual(newEntity.EntitySchema.AttributeGroups.Count(), mappedEntity.EntitySchema.AttributeGroups.Count());

        //    //BUG: There's an issue with the relations collection when calling count unless A LazyLookAhead provider is there ... which i need help with: SD.
        //    //Assert.AreEqual(2, mappedEntity.Relations.Count());
        //    foreach (var r in newEntity.Relations)
        //    {
        //        Assert.IsTrue(mappedEntity.Relations.Any(x => x.SourceId == r.SourceId && x.DestinationId == r.DestinationId));
        //    }

        //}
    }
}
