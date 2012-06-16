using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Hive;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.NHibernate;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders
{
    [TestClass]
    public abstract class AbstractHivePersistenceTest : DisposableObject
    {
        protected abstract Action PostWriteCallback { get; }
        protected abstract IHiveReadProvider DirectReaderProvider { get; }
        protected abstract IHiveReadWriteProvider DirectReadWriteProvider { get; }

        protected abstract IHiveReadProvider ReaderProviderViaHiveGovernor { get; }
        protected abstract IHiveReadWriteProvider ReadWriteProviderViaHiveGovernor { get; }
        protected abstract IFrameworkContext FrameworkContext { get; }

        [TestMethod]
        public void SaveAndLoad_InbuiltSchemas()
        {
            var originalSecurity = new UserSchema();


            using (IReadWriteUnitOfWork writer = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(originalSecurity);
                writer.Commit();
            }

            // Clear session to avoid caching when testing readbacks
            PostWriteCallback.Invoke();

            using (IReadOnlyUnitOfWork reader = ReaderProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                // Reload. Shouldn't matter that we're newing up the schemas to get the Id as Id should 
                // have been left alone when saving, so leave it here as a test case
                var securitySchema = reader.ReadRepository.GetEntity<EntitySchema>(originalSecurity.Id);

                Assert.IsNotNull(securitySchema, "Security schema");

                Assert.AreEqual(originalSecurity.AttributeDefinitions.Count, securitySchema.AttributeDefinitions.Count, "Security attrib defs count differ");
                Assert.AreEqual(originalSecurity.AttributeGroups.Count, securitySchema.AttributeGroups.Count, "Security attrib groups count differ");
                Assert.AreEqual(originalSecurity.AttributeTypes.Count, securitySchema.AttributeTypes.Count, "Security attrib types count differ");
            }
        }

        [TestMethod]
        public void Deleting_Schema_Removes_All_Entities()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
            {
                //Arrange

                Revision<TypedEntity> content1 = HiveModelCreationHelper.MockVersionedTypedEntity();
                Revision<TypedEntity> content2 = HiveModelCreationHelper.MockVersionedTypedEntity();
                //assign ids to create a relation
                content1.Item.Id = HiveId.ConvertIntToGuid(1);
                content2.Item.Id = HiveId.ConvertIntToGuid(2);
                content1.Item.Relations.Add(FixedRelationTypes.ContentTreeRelationType, content2.Item);
                //assign ids to schema and create schema relation
                content1.Item.EntitySchema.Id = HiveId.ConvertIntToGuid(10);
                content2.Item.EntitySchema.Id = HiveId.ConvertIntToGuid(20);
                content1.Item.EntitySchema.Relations.Add(FixedRelationTypes.SchemaTreeRelationType, content2.Item.EntitySchema);

                using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                {
                    uow.ReadWriteRepository.AddOrUpdate(content1);
                    //make some versions
                    uow.ReadWriteRepository.AddOrUpdate(content1);
                    uow.ReadWriteRepository.AddOrUpdate(content1);
                    uow.Commit();
                }
                PostWriteCallback.Invoke();

                //Act

                using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                {
                    uow.ReadWriteRepository.Delete<EntitySchema>(content1.Item.EntitySchema.Id);
                    uow.Commit();
                }
                PostWriteCallback.Invoke();

                //Assert

                using (IReadOnlyUnitOfWork uow = readWriter.CreateReadOnlyUnitOfWork())
                {
                    var attributeType = uow.ReadRepository.GetEntity<EntitySchema>(content1.Item.EntitySchema.Id);
                    Assert.IsNull(attributeType);
                    var contentReloaded = uow.ReadRepository.GetEntity<TypedEntity>(content1.Item.Id);
                    Assert.IsNull(contentReloaded);
                }
                PostWriteCallback.Invoke();
            };

            //runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public void Deleting_Attribute_Type_Removes_All_Associations()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
                {
                    //Arrange

                    Revision<TypedEntity> content = HiveModelCreationHelper.MockVersionedTypedEntity();

                    using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                    {
                        uow.ReadWriteRepository.AddOrUpdate(content);
                        uow.Commit();
                    }
                    PostWriteCallback.Invoke();

                    var referncedAttribute = content.Item.Attributes.First();
                    var referencedAttributeDef = referncedAttribute.AttributeDefinition;
                    var attTypeToDelete = referencedAttributeDef.AttributeType;

                    //Act

                    using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                    {
                        var attributeType = uow.ReadWriteRepository.GetEntity<AttributeType>(attTypeToDelete.Id);
                        uow.ReadWriteRepository.Delete<AttributeType>(attributeType.Id);
                        uow.Commit();
                    }
                    PostWriteCallback.Invoke();

                    //Assert

                    using (IReadOnlyUnitOfWork uow = readWriter.CreateReadOnlyUnitOfWork())
                    {
                        var attributeType = uow.ReadRepository.GetEntity<AttributeType>(attTypeToDelete.Id);
                        Assert.IsNull(attributeType);
                        var attributeDefinition = uow.ReadRepository.GetEntity<AttributeDefinition>(referencedAttributeDef.Id);
                        Assert.IsNull(attributeDefinition);
                        var attribute = uow.ReadRepository.GetEntity<TypedAttribute>(referncedAttribute.Id);
                        Assert.IsNull(attribute);

                        var contentReloaded = uow.ReadRepository.GetEntity<TypedEntity>(content.Item.Id);
                        Assert.AreEqual(content.Item.Attributes.Count - 1, contentReloaded.Attributes.Count);
                    }
                    PostWriteCallback.Invoke();
                };

            //runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public void Create_Typed_Entity_Under_Root_Then_Copy_To_Another_Parent()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
                {
                    //Arrange

                    SystemRoot root = FixedEntities.SystemRoot;
                    Revision<TypedEntity> content = HiveModelCreationHelper.MockVersionedTypedEntity();
                    //content.Item.Id = new HiveId(1); // <- Need to set id based on current Bug with relations
                    Revision<TypedEntity> newParent = HiveModelCreationHelper.MockVersionedTypedEntity();
                    //newParent.Item.Id = new HiveId(2); // <- Need to set id based on current Bug with relations

                    content.Item.Relations.Add(FixedRelationTypes.ContentTreeRelationType, root, content.Item);

                    using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                    {
                        uow.ReadWriteRepository.AddOrUpdate(root);
                        uow.ReadWriteRepository.AddOrUpdate(content);
                        uow.ReadWriteRepository.AddOrUpdate(newParent);
                        uow.Commit();
                    }
                    PostWriteCallback.Invoke();

                    //Act

                    TypedEntity copied;
                    using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                    {
                        //create a new copied entity
                        copied = content.Item.CreateCopy(newParent.Item.Id);

                        uow.ReadWriteRepository.AddOrUpdate(copied);
                        uow.Commit();
                    }
                    PostWriteCallback.Invoke();

                    //Assert

                    using (IReadOnlyUnitOfWork uow = readWriter.CreateReadOnlyUnitOfWork())
                    {
                        var queriesCopiedContent = uow.ReadRepository.GetEntity<TypedEntity>(copied.Id);
                        Assert.IsNotNull(queriesCopiedContent);
                        Assert.AreEqual(FixedRelationTypes.ContentTreeRelationType.RelationName, queriesCopiedContent.Relations.Single().Type.RelationName);
                        Assert.AreEqual(newParent.Item.Id, queriesCopiedContent.Relations.Single().SourceId);
                    }
                    PostWriteCallback.Invoke();
                };

            //runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public void Create_Typed_Entity_Under_Root_Then_Move_To_Another_Parent()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
                {
                    //Arrange

                    SystemRoot root = FixedEntities.SystemRoot;
                    Revision<TypedEntity> content = HiveModelCreationHelper.MockVersionedTypedEntity();
                    //content.Item.Id = new HiveId(1); // <- Need to set id based on current Bug with relations                    
                    Revision<TypedEntity> newParent = HiveModelCreationHelper.MockVersionedTypedEntity();
                    //newParent.Item.Id = new HiveId(2); // <- Need to set id based on current Bug with relations

                    content.Item.Relations.Add(FixedRelationTypes.ContentTreeRelationType, root, content.Item);


                    using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                    {
                        uow.ReadWriteRepository.AddOrUpdate(root);
                        uow.ReadWriteRepository.AddOrUpdate(content);
                        uow.ReadWriteRepository.AddOrUpdate(newParent);
                        uow.Commit();
                    }
                    PostWriteCallback.Invoke();

                    //Act

                    using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                    {

                        //tried this... which ends up creating another new relation so content is now related to both root and new Parent
                        //but at least no exceptions are thrown.

                        //var contentParent = content.Item.Relations.Where(x =>
                        //            x.Type.RelationName == FixedRelationTypes.ContentTreeRelationType.RelationName
                        //            && x.Source.Id == root.Id).Single();
                        //contentParent.Source = newParent.Item;

                        //Then tried this ( and combinations of it )... with no luck, this causes a: deleted object would be re-saved by cascade NH exception

                        root.Relations.RemoveWhere(
                            x => x.Source == root && x.Destination == content.Item && x.Type.RelationName == FixedRelationTypes.ContentTreeRelationType.RelationName);
                        content.Item.Relations.RemoveWhere(
                            x => x.Source == root && x.Destination == content.Item && x.Type.RelationName == FixedRelationTypes.ContentTreeRelationType.RelationName);
                        content.Item.Relations.Add(
                            FixedRelationTypes.ContentTreeRelationType, newParent.Item, content.Item);

                        uow.ReadWriteRepository.AddOrUpdate(root);
                        uow.ReadWriteRepository.AddOrUpdate(content);
                        uow.Commit();
                    }
                    PostWriteCallback.Invoke();

                    //Assert

                    using (IReadOnlyUnitOfWork uow = readWriter.CreateReadOnlyUnitOfWork())
                    {
                        var e = uow.ReadRepository.GetEntity<TypedEntity>(content.Item.Id);
                        Assert.IsNotNull(e);
                        Assert.AreEqual(FixedRelationTypes.ContentTreeRelationType.RelationName, e.Relations.Single().Type.RelationName);
                        Assert.AreEqual(newParent.Item.Id, content.Item.Relations.Single().SourceId);
                    }
                    PostWriteCallback.Invoke();
                };

            //runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public void Delete_Typed_Entity_With_Revisions()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
                {
                    //Arrange

                    Revision<TypedEntity> content1 = HiveModelCreationHelper.MockVersionedTypedEntity();
                    using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                    {
                        uow.ReadWriteRepository.AddOrUpdate(content1);
                        uow.Commit();
                    }
                    PostWriteCallback.Invoke();

                    //Act

                    using (IReadWriteUnitOfWork uow = readWriter.CreateReadWriteUnitOfWork())
                    {
                        uow.ReadWriteRepository.Delete<TypedEntity>(content1.Item.Id);
                        uow.Commit();
                    }
                    PostWriteCallback.Invoke();

                    //Assert

                    using (IReadOnlyUnitOfWork uow = readWriter.CreateReadOnlyUnitOfWork())
                    {
                        Revision<TypedEntity> revEntity = uow.ReadRepository.GetRevision<TypedEntity>(content1.Item.Id, content1.MetaData.Id);
                        Assert.IsNull(revEntity);
                        var e = uow.ReadRepository.GetEntity<TypedEntity>(content1.Item.Id);
                        Assert.IsNull(e);
                    }
                    PostWriteCallback.Invoke();
                };

            //runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public void SaveAndLoadEntityByQuery()
        {
            //TypedEntity entity = HiveModelCreationHelper.MockTypedPersistenceEntity();

            //PersistenceTestBuilder.Create<TypedEntity>(PostWriteCallback)
            //    .AddAssertStringEquals(x => x.Attributes.First().Value, "First attribute value")
            //    .AddAssertStringEquals(x => x.EntitySchema.Alias, "EntitySchema.Alias")
            //    .AddAssertStringEquals(x => x.EntitySchema.AttributeDefinitions[0].Alias, "EntitySchema.AttributeDefinitions[0].Alias")
            //    .RunAssertionsWithQuery(DirectReaderProvider, DirectReadWriteProvider, entity, id => query => query.Id == id)
            //    .RunAssertionsWithQuery(DirectReaderProvider, DirectReadWriteProvider, entity, id => query => query.Attributes["alias-1"].Value == "my-test-value")
            //    .RunAssertionsWithQuery(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, entity, id => query => query.Id == id)
            //    .RunAssertionsWithQuery(DirectReaderProvider, DirectReadWriteProvider, entity, id => query => query.EntitySchema.Alias == "test-doctype-alias")
            //    .RunAssertionsWithQuery(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, entity, id => query => query.EntitySchema.Alias == "test-doctype-alias");

            Assert.Inconclusive();
        }

        [TestMethod]
        public virtual void Create_Schema_Relations_No_Ids()
        {

            //Arrange

            var schema1 = HiveModelCreationHelper.MockEntitySchema("schema1", "schema1");
            var schema2 = HiveModelCreationHelper.MockEntitySchema("schema2", "schema2");
            //make schema 1 the parent of schema 2
            schema1.Relations.Add(FixedRelationTypes.SchemaTreeRelationType, schema2);

            //Act

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(schema1);
                uow.ReadWriteRepository.AddOrUpdate(schema2);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                var found1 = uow.ReadRepository.GetEntity<EntitySchema>(schema1.Id);
                var found2 = uow.ReadRepository.GetEntity<EntitySchema>(schema2.Id);

                Assert.AreEqual(1, found1.Relations.Count());
                Assert.AreEqual(1, found2.Relations.AncestorsOrSelfAsSchema().Count());
                Assert.AreEqual(found1.Relations.Single().Destination.Id, found2.Id);
                Assert.AreEqual(found2.Relations.AncestorsOrSelfAsSchema().Single().Id, found1.Id);
            }
        }

        [TestMethod]
        public virtual void Create_Typed_Entity_Relations_No_Ids()
        {
            //Arrange

            Func<int, TypedEntity> createNewEntity = i =>
            {
                EntitySchema schema = HiveModelCreationHelper.MockEntitySchema("schema" + i, "schema" + i);
                TypedEntity entity = HiveModelCreationHelper.CreateTypedEntity(schema, new[]
                    {
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(0), "value1"),
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(1), "value2"),
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(2), "value3")
                    });
                return entity;
            };

            var entity1 = createNewEntity(1);
            var entity2 = createNewEntity(2);
            //make entity 1 the parent of entity 2
            entity1.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entity2);

            //Act

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(entity1);
                uow.ReadWriteRepository.AddOrUpdate(entity2);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                var found1 = uow.ReadRepository.GetEntity<TypedEntity>(entity1.Id);
                var found2 = uow.ReadRepository.GetEntity<TypedEntity>(entity2.Id);

                Assert.AreEqual(1, found1.Relations.Count());
                Assert.AreEqual(1, found2.Relations.AncestorsOrSelfAsSchema().Count());
                Assert.AreEqual(found1.Relations.Single().Destination.Id, found2.Id);
                Assert.AreEqual(found2.Relations.AncestorsOrSelfAsSchema().Single().Id, found1.Id);
            }
        }

        [TestMethod]
        public virtual void Delete_Entity_Schema_With_Relations()
        {
            //Arrange           

            var schema1 = HiveModelCreationHelper.MockEntitySchema("schema1", "schema1");
            schema1.Id = HiveId.ConvertIntToGuid(1); // <- must assign Ids as there is currently a bug creating a relation with new entities without any ids
            var schema2 = HiveModelCreationHelper.MockEntitySchema("schema2", "schema2");
            schema2.Id = HiveId.ConvertIntToGuid(2); // <- must assign Ids as there is currently a bug creating a relation with new entities without any ids
            schema1.Relations.Add(FixedRelationTypes.SchemaTreeRelationType, schema2);

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(schema1);
                uow.ReadWriteRepository.AddOrUpdate(schema2);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.Delete<EntitySchema>(schema2.Id);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                var found1 = uow.ReadRepository.GetEntity<EntitySchema>(schema1.Id);
                var found2 = uow.ReadRepository.GetEntity<EntitySchema>(schema2.Id);

                Assert.IsNull(found2);
                Assert.IsNotNull(found1);
                Assert.AreEqual(0, found1.Relations.Count());
            }
        }

        [TestMethod]
        public virtual void Delete_Typed_Entity_With_Relations_And_Revisions()
        {
            //Arrange

            Func<int, TypedEntity> createNewEntity = i =>
            {
                EntitySchema schema = HiveModelCreationHelper.MockEntitySchema("schema" + i, "schema" + i);
                TypedEntity entity = HiveModelCreationHelper.CreateTypedEntity(schema, new[]
                    {
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(0), "value1"),
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(1), "value2"),
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(2), "value3")
                    });
                return entity;
            };

            var entity1 = createNewEntity(1);
            entity1.Id = HiveId.ConvertIntToGuid(1); // <- BUG: must assign Ids as there is currently a bug creating a relation with new entities without any ids
            var entity2 = createNewEntity(2);
            entity2.Id = HiveId.ConvertIntToGuid(2); // <- BUG: must assign Ids as there is currently a bug creating a relation with new entities without any ids
            entity1.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entity2);

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(entity1);
                uow.ReadWriteRepository.AddOrUpdate(entity2);
                //create a few versions
                uow.ReadWriteRepository.AddOrUpdate(entity1);
                uow.ReadWriteRepository.AddOrUpdate(entity2);
                uow.ReadWriteRepository.AddOrUpdate(entity1);
                uow.ReadWriteRepository.AddOrUpdate(entity2);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.Delete<TypedEntity>(entity2.Id);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                var found1 = uow.ReadRepository.GetLatestSnapshot<TypedEntity>(entity1.Id);
                var found2 = uow.ReadRepository.GetLatestSnapshot<TypedEntity>(entity2.Id);

                Assert.IsNull(found2);
                Assert.IsNotNull(found1);
                Assert.AreEqual(3, found1.EntityRevisionStatusList.Count());
                Assert.AreEqual(0, found1.Revision.Item.Relations.Count());
            }
        }

        [TestMethod]
        public virtual void GetLatestSnapshot_Returns_Null_When_No_Data_Found()
        {
            //Arrange

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                //Act

                var notFound = uow.ReadRepository.GetLatestSnapshot<TypedEntity>(HiveId.ConvertIntToGuid(1234));

                //Assert

                Assert.IsNull(notFound);
            }
        }

        [TestMethod]
        public virtual void GetEntitySnapshot_Returns_Null_When_No_Data_Found()
        {
            //Arrange

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                //Act

                var notFound = uow.ReadRepository.GetEntitySnapshot<TypedEntity>(
                    HiveId.ConvertIntToGuid(1234),
                    HiveId.ConvertIntToGuid(9876));

                //Assert

                Assert.IsNull(notFound);
            }
        }

        /// <summary>
        /// This test exists because there were previously bugs in the MergeMapCollections method of the ModelToRdbmsMapper that
        /// wouldn't allow for re-saving new versions with multiple TypedEntities with relations were being saved
        /// </summary>
        [TestMethod]
        public virtual void Save_Multiple_TypedEntities_With_Multiple_Versions()
        {
            //Arrange

            Func<int, TypedEntity> createNewEntity = i =>
            {
                EntitySchema schema = HiveModelCreationHelper.MockEntitySchema("schema" + i, "schema" + i);
                TypedEntity entity = HiveModelCreationHelper.CreateTypedEntity(schema, new[]
                    {
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(0), "value1"),
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(1), "value2"),
                        new TypedAttribute(schema.AttributeDefinitions.ElementAt(2), "value3")
                    });
                return entity;
            };

            var entity1 = createNewEntity(1);
            entity1.Id = HiveId.ConvertIntToGuid(1); // <- BUG: must assign Ids as there is currently a bug creating a relation with new entities without any ids
            var entity2 = createNewEntity(2);
            entity2.Id = HiveId.ConvertIntToGuid(2); // <- BUG: must assign Ids as there is currently a bug creating a relation with new entities without any ids
            entity1.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entity2);

            //Act

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(entity1);
                uow.ReadWriteRepository.AddOrUpdate(entity2);
                //create a few versions
                uow.ReadWriteRepository.AddOrUpdate(entity1);
                uow.ReadWriteRepository.AddOrUpdate(entity2);
                uow.ReadWriteRepository.AddOrUpdate(entity1);
                uow.ReadWriteRepository.AddOrUpdate(entity2);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                var found1 = uow.ReadRepository.GetLatestSnapshot<TypedEntity>(entity1.Id);
                var found2 = uow.ReadRepository.GetLatestSnapshot<TypedEntity>(entity2.Id);

                Assert.IsNotNull(found1);
                Assert.IsNotNull(found2);
                Assert.AreEqual(3, found1.EntityRevisionStatusList.Count());
                //NOTE: This is 4 because when you AddOrUpdate an entity with a relation, it automatically does an AddOrUpdate on the entity inside the relation
                Assert.AreEqual(4, found2.EntityRevisionStatusList.Count());
                Assert.AreEqual(3, found1.Revision.Item.Attributes.Count());
                Assert.AreEqual(3, found2.Revision.Item.Attributes.Count());
                Assert.AreEqual(1, found1.Revision.Item.AttributeGroups.Count());
                Assert.AreEqual(1, found2.Revision.Item.AttributeGroups.Count());
                Assert.AreEqual(4, found1.Revision.Item.EntitySchema.AttributeDefinitions.Count());
                Assert.AreEqual(4, found2.Revision.Item.EntitySchema.AttributeDefinitions.Count());
                Assert.AreEqual(1, found1.Revision.Item.EntitySchema.AttributeGroups.Count());
                Assert.AreEqual(1, found2.Revision.Item.EntitySchema.AttributeGroups.Count());
            }
        }

        [TestMethod]
        public virtual void Deleting_Attribute_Definition_From_Group()
        {
            AttributeGroup group1 = HiveModelCreationHelper.CreateAttributeGroup("group1", "group1", 0);
            AttributeType type1 = HiveModelCreationHelper.CreateAttributeType("type1", "type1", "type1");
            AttributeDefinition def1 = HiveModelCreationHelper.CreateAttributeDefinition("def1", "def1", "def1", type1, group1);
            AttributeDefinition def2tobedeleted = HiveModelCreationHelper.CreateAttributeDefinition("def2tobedeleted", "def2tobedeleted", "def2tobedeleted", type1, group1);
            AttributeDefinition def3tobedeletedDirectly = HiveModelCreationHelper.CreateAttributeDefinition("def3tobedeletedDirectly", "def3tobedeletedDirectly", "def3tobedeletedDirectly", type1, group1);
            EntitySchema schema1 = HiveModelCreationHelper.CreateEntitySchema("schema1", "schema1", def1, def2tobedeleted, def3tobedeletedDirectly);
            TypedEntity content1 = HiveModelCreationHelper.CreateTypedEntity(schema1, new[]
                {
                    new TypedAttribute(def1, "value1"),
                    new TypedAttribute(def2tobedeleted, "value2tobedeleted"),
                    new TypedAttribute(def3tobedeletedDirectly, "value2tobedeletedDirectly")
                });

            using (IReadWriteUnitOfWork uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(schema1);

                // Create a few revisions
                uow.ReadWriteRepository.AddOrUpdate(content1);
                uow.ReadWriteRepository.AddOrUpdate(content1);
                uow.ReadWriteRepository.AddOrUpdate(content1);
                uow.ReadWriteRepository.AddOrUpdate(content1);
                uow.ReadWriteRepository.AddOrUpdate(content1);
                uow.ReadWriteRepository.AddOrUpdate(content1);

                uow.Commit();
            }

            PostWriteCallback.Invoke();

            //using (var uow = this.ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            //{
            //    var contentReloaded1 = uow.ReadWriteRepository.GetEntity<TypedEntity>(content1.Id);

            //    contentReloaded1.Attributes.RemoveAll(x => x.AttributeDefinition.Alias == def2tobedeleted.Alias);

            //    uow.ReadWriteRepository.AddOrUpdate(contentReloaded1);
            //    uow.Commit();

            //    contentReloaded1 = uow.ReadWriteRepository.GetEntity<TypedEntity>(content1.Id);

            //    Assert.AreEqual(2, contentReloaded1.Attributes.Count);
            //}

            using (IReadWriteUnitOfWork uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                var schemaReloaded1 = uow.ReadWriteRepository.GetEntity<EntitySchema>(schema1.Id);
                //var contentReloaded1 = uow.ReadWriteRepository.GetEntity<TypedEntity>(content1.Id);

                Assert.AreEqual(3, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(1, schemaReloaded1.AttributeGroups.Count);

                schemaReloaded1.AttributeDefinitions.RemoveAll(x => x.Alias == def2tobedeleted.Alias);
                //contentReloaded1.Attributes.RemoveAll(x => x.AttributeDefinition.Alias == def2tobedeleted.Alias);

                Assert.AreEqual(2, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(1, schemaReloaded1.AttributeGroups.Count);

                //uow.ReadWriteRepository.AddOrUpdate(contentReloaded1);
                uow.ReadWriteRepository.AddOrUpdate(schemaReloaded1);
                uow.Commit();

                schemaReloaded1 = uow.ReadWriteRepository.GetEntity<EntitySchema>(schema1.Id);
                //contentReloaded1 = uow.ReadWriteRepository.GetEntity<TypedEntity>(content1.Id);

                Assert.AreEqual(2, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(1, schemaReloaded1.AttributeGroups.Count);
                //Assert.AreEqual(1, contentReloaded1.Attributes.Count);
            }

            PostWriteCallback.Invoke();

            HiveId attribDefId = HiveId.Empty;

            using (IReadWriteUnitOfWork uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                var schemaReloaded1 = uow.ReadWriteRepository.GetEntity<EntitySchema>(schema1.Id);
                attribDefId = schemaReloaded1.AttributeDefinitions.FirstOrDefault(x => x.Alias == "def3tobedeletedDirectly").Id;
                var contentReloaded1 = uow.ReadWriteRepository.GetEntity<TypedEntity>(content1.Id);
                Assert.AreEqual(2, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(2, contentReloaded1.Attributes.Count);
            }

            PostWriteCallback.Invoke();

            using (IReadWriteUnitOfWork uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.Delete<AttributeDefinition>(attribDefId);
                uow.Commit();

                var schemaReloaded1 = uow.ReadWriteRepository.GetEntity<EntitySchema>(schema1.Id);
                var contentReloaded1 = uow.ReadWriteRepository.GetEntity<TypedEntity>(content1.Id);

                Assert.AreEqual(1, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(1, contentReloaded1.Attributes.Count);
            }
        }

        [TestMethod]
        public virtual void ReSaveRevision()
        {
            //Arrange

            Revision<TypedEntity> revision = HiveModelCreationHelper.MockVersionedTypedEntity();

            using (IReadWriteUnitOfWork uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(revision);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Act

            using (IReadWriteUnitOfWork uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(revision);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                var found = uow.ReadRepository.GetLatestSnapshot<TypedEntity>(revision.Item.Id);
                Assert.AreEqual(2, found.EntityRevisionStatusList.Count());
            }

        }

        [TestMethod]
        public virtual void ReSaveEntity()
        {
            //Arrange

            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity();
            using (IReadWriteUnitOfWork uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(entity);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Act

            using (IReadWriteUnitOfWork uow = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.AddOrUpdate(entity);
                uow.Commit();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadWriteProviderViaHiveGovernor.CreateReadOnlyUnitOfWork())
            {
                var found = uow.ReadRepository.GetLatestSnapshot<TypedEntity>(entity.Id);
                Assert.AreEqual(2, found.EntityRevisionStatusList.Count());
            }
        }

        [TestMethod]
        public virtual void SaveAndLoadAttributeTypeDefinition()
        {
            Func<AttributeType> entityGenerator = () => HiveModelCreationHelper.CreateAttributeType("test-alias",
                                                                                                              "test-name",
                                                                                                              "test-description");

            PersistenceTestBuilder.Create<AttributeType>(PostWriteCallback)
                .AddAssertStringEquals(x => x.Name, "Name")
                .AddAssertStringEquals(x => x.Description, "Description")
                .AddAssertStringEquals(x => x.Alias, "Alias")
                .AddAssertDateTimeOffset(x => x.UtcCreated, "Date created")
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, entityGenerator)
                .RunAssertions(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, entityGenerator);
            //.RunAssertionsWithQuery(_reader, _writer, entityGenerator, generatedId => query => query.Id == generatedId);
        }

        [TestMethod]
        [ExpectedException(typeof(UnitTestAssertException), AllowDerivedTypes = true)]
        public virtual void TestHelperThrows()
        {
            // The purpose of this test is to establish that a bad assert (in this case, requiring that the Name
            // equals the Description once written-and-read) raises an assertion exception

            AttributeType entity = HiveModelCreationHelper.CreateAttributeType("test-alias",
                                                                                         "test-name",
                                                                                         "test-description");
            PersistenceTestBuilder.Create<AttributeType>(PostWriteCallback)
                .AddAssert(x => x.Name, (y, origValue) => Assert.AreEqual(origValue, y.Description, "Should throw because comparison isn't valid"))
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, entity)
                .RunAssertions(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, entity);
        }

        [TestMethod]
        public virtual void Ensure_Transactions_And_Data_Contexts_Are_Disposed_Correctly()
        {
            // The purpose of this test is to establish that the DataContext is not disposed after a UoW

            AttributeType entity = HiveModelCreationHelper.CreateAttributeType("test-alias",
                                                                                         "test-name",
                                                                                         "test-description");

            AbstractDataContext directContext;
            AbstractDataContext hiveContext;

            ITransaction directTransaction;
            ITransaction hiveTransaction;

            using (IReadWriteUnitOfWork writer = DirectReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(entity);
                directContext = writer.DataContext;
                directTransaction = writer.DataContext.CurrentTransaction;
                writer.Commit();
            }

            Assert.IsInstanceOfType(directContext, typeof(DisposableObject));
            Assert.IsFalse((directContext).IsDisposed, "Direct context was disposed by uow");

            Assert.IsInstanceOfType(directTransaction, typeof(DisposableObject));
            Assert.IsTrue(((DisposableObject)directTransaction).IsDisposed, "Direct transaction was NOT disposed by uow");

            using (IReadWriteUnitOfWork writer = ReadWriteProviderViaHiveGovernor.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(entity);
                hiveContext = writer.DataContext;
                hiveTransaction = writer.DataContext.CurrentTransaction;
                writer.Commit();
            }

            Assert.IsInstanceOfType(hiveContext, typeof(DisposableObject));
            Assert.IsFalse((hiveContext).IsDisposed, "Hive context was disposed by uow");

            Assert.IsInstanceOfType(hiveTransaction, typeof(DisposableObject));
            Assert.IsTrue(((DisposableObject)hiveTransaction).IsDisposed, "Hive transaction was NOT disposed by uow");

            FrameworkContext.ScopedFinalizer.FinalizeScope();

            Assert.IsInstanceOfType(directContext, typeof(DisposableObject));
            Assert.IsTrue((directContext).IsDisposed, "Direct context was not disposed by Finalizer");

            Assert.IsInstanceOfType(hiveContext, typeof(DisposableObject));
            Assert.IsTrue((hiveContext).IsDisposed, "Hive context was not disposed by Finalizer");
        }

        [TestMethod]
        public virtual void SaveAndLoadContentTreeRelation()
        {
            // We use a factory here so that when running two tests, below (one direct and one via hive) we don't have
            // a generated id spanning a Session.Clear() call into the next test where that Id will not be found
            Func<TypedEntity> entity = () =>
                {
                    TypedEntity entityParent =
                        HiveModelCreationHelper.MockTypedEntity();
                    TypedEntity entityChild =
                        HiveModelCreationHelper.MockTypedEntity();

                    entityParent.Relations.Add(
                        FixedRelationTypes.ContentTreeRelationType, entityChild);

                    return entityParent;
                };

            PersistenceTestBuilder.Create<TypedEntity>(PostWriteCallback)
                .AddAssertIntegerEquals(x => x.Relations.Count(), "Number of relations")
                //.AddAssertStringEquals(x => x.Relations.GetRelations(HierarchyScope.Children).First().SourceId.ToString(), "First item on relation")
                // .AddAssertStringEquals(x => x.Relations.GetRelations(HierarchyScope.Children).First().DestinationId.ToString(), "First item on relation")
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, entity);
            //.RunAssertions(ReaderViaHive, ReadWriterViaHive, entity);
        }

        [TestMethod]
        public virtual void SaveAndLoadSchemaWithRelation()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
            {
                //Arrange

                var entityParent = HiveModelCreationHelper.MockEntitySchema("test-schema-parent", "parent");
                entityParent.Id = HiveId.ConvertIntToGuid(1);
                var entityChild = HiveModelCreationHelper.MockEntitySchema("test-schema-child", "child");
                entityChild.Id = HiveId.ConvertIntToGuid(2);

                entityParent.Relations.Add(FixedRelationTypes.SchemaTreeRelationType, entityChild);
                //BUG: Adding this will cause a stack overflow exception when AddOrUpdate is called on the first element
                //entityChild.Relations.Add(FixedRelationTypes.SchemaTreeRelationType, entityParent, entityChild);

                //Act

                using (var uow = readWriter.CreateReadWriteUnitOfWork())
                {
                    uow.ReadWriteRepository.AddOrUpdate(entityParent);
                    uow.ReadWriteRepository.AddOrUpdate(entityChild);
                    uow.Commit();
                }
                PostWriteCallback.Invoke();

                //Assert

                using (var uow = readWriter.CreateReadOnlyUnitOfWork())
                {
                    var queriedParent = uow.ReadRepository.GetEntity<EntitySchema>(entityParent.Id);
                    var queriedChild = uow.ReadRepository.GetEntity<EntitySchema>(entityChild.Id);

                    Assert.AreEqual(entityParent.Relations.Count(), queriedParent.Relations.Count());
                    Assert.AreEqual(entityParent.Relations.Single().Source.Id, entityParent.Id);
                    Assert.AreEqual(entityParent.Relations.Single().Destination.Id, entityChild.Id);
                    Assert.AreEqual(queriedChild.Relations.First().Source.Id, queriedParent.Id);
                }
                PostWriteCallback.Invoke();
            };

            runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public virtual void SaveAndLoadSchemaByRelation()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
            {
                //Arrange

                var entityParent = HiveModelCreationHelper.MockEntitySchema("test-schema-parent", "parent");
                entityParent.Id = HiveId.ConvertIntToGuid(1);
                var entityChild = HiveModelCreationHelper.MockEntitySchema("test-schema-child", "child");
                entityChild.Id = HiveId.ConvertIntToGuid(2);

                entityParent.Relations.Add(FixedRelationTypes.SchemaTreeRelationType, entityChild);
                //BUG: Adding this will cause a stack overflow exception when AddOrUpdate is called on the first element
                //entityChild.Relations.Add(FixedRelationTypes.SchemaTreeRelationType, entityParent, entityChild);

                //Act

                using (var uow = readWriter.CreateReadWriteUnitOfWork())
                {
                    uow.ReadWriteRepository.AddOrUpdate(entityParent);
                    uow.ReadWriteRepository.AddOrUpdate(entityChild);
                    uow.Commit();
                }
                PostWriteCallback.Invoke();

                //Assert

                using (var uow = readWriter.CreateReadOnlyUnitOfWork())
                {
                    var children = uow.ReadRepository.GetEntityByRelationType<EntitySchema>(FixedRelationTypes.SchemaTreeRelationType, entityParent.Id);

                    Assert.AreEqual(1, children.Count());
                    Assert.AreEqual(entityParent.Relations.Single().Source.Id, entityParent.Id);
                    Assert.AreEqual(entityParent.Relations.Single().Destination.Id, children.Single().Id);
                }
            };

            runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public virtual void SaveAndLoadTypedEntityWithRelation()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
            {
                //Arrange

                var entityParent = HiveModelCreationHelper.MockTypedEntity(false);
                entityParent.Id = HiveId.ConvertIntToGuid(1);
                var entityChild = HiveModelCreationHelper.MockTypedEntity(false);
                entityChild.Id = HiveId.ConvertIntToGuid(2);

                entityParent.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityChild, 2); // <- Testing that ordinal is saved too

                //BUG: Adding this will cause a stack overflow exception when AddOrUpdate is called on the first element
                //entityChild.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityParent, entityChild);

                //Act

                using (var uow = readWriter.CreateReadWriteUnitOfWork())
                {
                    uow.ReadWriteRepository.AddOrUpdate(entityParent);
                    //uow.ReadWriteRepository.AddOrUpdate(entityChild);
                    uow.Commit();
                }
                PostWriteCallback.Invoke();

                //Assert

                using (var uow = readWriter.CreateReadOnlyUnitOfWork())
                {
                    var queriedParent = uow.ReadRepository.GetEntity<TypedEntity>(entityParent.Id);
                    var queriedChild = uow.ReadRepository.GetEntity<TypedEntity>(entityChild.Id);

                    Assert.AreEqual(entityParent.Relations.Count(), queriedParent.Relations.Count());
                    Assert.AreEqual(entityParent.Relations.Single().Source.Id, entityParent.Id);
                    Assert.AreEqual(entityParent.Relations.Single().Destination.Id, entityChild.Id);
                    Assert.AreEqual(2, entityParent.Relations.Single().Ordinal);
                    Assert.AreEqual(FixedRelationTypes.ContentTreeRelationType.RelationName, entityParent.Relations.Single().Type.RelationName);
                    Assert.AreEqual(queriedChild.Relations.First().Source.Id, queriedParent.Id);
                    Assert.AreEqual(2, queriedChild.Relations.Single().Ordinal);
                    Assert.AreEqual(FixedRelationTypes.ContentTreeRelationType.RelationName, queriedChild.Relations.Single().Type.RelationName);
                }
                PostWriteCallback.Invoke();
            };

            //BUG: enabling both of these will result in a YSOD... seems as though the session is timed out HOWEVER, this works for Schema relations so not sure whats going on
            runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public virtual void SaveAndLoadTypedEntityByRelation()
        {
            Action<IHiveReadWriteProvider> runTest = readWriter =>
            {
                //Arrange

                var entityParent = HiveModelCreationHelper.MockTypedEntity(false);
                entityParent.Id = HiveId.ConvertIntToGuid(1);
                var entityChild = HiveModelCreationHelper.MockTypedEntity(false);
                entityChild.Id = HiveId.ConvertIntToGuid(2);

                entityParent.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityChild, 2); // <- Testing that ordinal is saved too
                //BUG: Adding this will cause a stack overflow exception when AddOrUpdate is called on the first element
                //entityChild.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityParent, entityChild);

                //Act

                using (var uow = readWriter.CreateReadWriteUnitOfWork())
                {
                    uow.ReadWriteRepository.AddOrUpdate(entityParent);
                    uow.ReadWriteRepository.AddOrUpdate(entityChild);
                    uow.Commit();
                }
                PostWriteCallback.Invoke();

                //Assert

                using (var uow = readWriter.CreateReadOnlyUnitOfWork())
                {
                    var children = uow.ReadRepository.GetEntityByRelationType<TypedEntity>(FixedRelationTypes.ContentTreeRelationType, entityParent.Id);

                    Assert.AreEqual(1, children.Count());
                    Assert.AreEqual(entityParent.Relations.Single().Source.Id, entityParent.Id);
                    Assert.AreEqual(entityParent.Relations.Single().Destination.Id, children.Single().Id);
                    Assert.AreEqual(2, entityParent.Relations.Single().Ordinal);
                }
                PostWriteCallback.Invoke();
            };

            //BUG: enabling both of these will result in a YSOD... seems as though the session is timed out HOWEVER, this works for Schema relations so not sure whats going on
            runTest(DirectReadWriteProvider);
            runTest(ReadWriteProviderViaHiveGovernor);
        }

        [TestMethod]
        public virtual void SaveAndLoadEntitySchema()
        {
            Func<EntitySchema> entity = () =>
                {
                    var e = HiveModelCreationHelper.MockEntitySchema("test-schema", "parent");
                    e.SchemaType = "unit-test-check";
                    return e;
                };

            PersistenceTestBuilder.Create<EntitySchema>(PostWriteCallback)
                .AddAssertStringEquals(x => x.Alias, "Schema alias")
                .AddAssertStringEquals(x => x.Name, "Schema name")
                .AddAssertStringEquals(x => x.SchemaType, "unit-test-check")
                .AddAssertIsTrue(x => x.AttributeDefinitions.Count, (x, y) => x == y, "Attribute definitions")
                .AddAssertIsTrue(x => x.AttributeTypes.Count, (x, y) => x == y, "Attribute type definitions")
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, entity)
                .RunAssertions(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, entity);
        }

        [TestMethod]
        public virtual void SaveAndLoadContentTreeRelationWithMetadata()
        {
            // We use a factory here so that when running two tests, below (one direct and one via hive) we don't have
            // a generated id spanning a Session.Clear() call into the next test where that Id will not be found
            Func<TypedEntity> entity = () =>
                {
                    TypedEntity entityParent =
                        HiveModelCreationHelper.MockTypedEntity();
                    TypedEntity entityChild =
                        HiveModelCreationHelper.MockTypedEntity();

                    entityParent.Relations.Add(
                        FixedRelationTypes.ContentTreeRelationType, entityChild,
                        new RelationMetaDatum("custom-metadata-key", "some-value"));

                    return entityParent;
                };

            PersistenceTestBuilder.Create<TypedEntity>(PostWriteCallback)
                .AddAssertIntegerEquals(x => x.Relations.Count(), "Number of relations")
                .AddAssertStringEquals(x => x.Relations.GetRelations(HierarchyScope.Children).First().SourceId.ToString(), "First item on relation")
                .AddAssertStringEquals(x => x.Relations.GetRelations(HierarchyScope.Children).First().DestinationId.ToString(), "First item on relation")
                .AddAssertStringEquals(x => x.Relations.GetRelations(HierarchyScope.Children).First().MetaData.First().Value, "First metadata item on relation")
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, entity)
                .RunAssertions(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, entity);
        }

        [TestMethod]
        public virtual void SaveAndLoadAttribute()
        {
            AttributeGroup groupDefinition = HiveModelCreationHelper.CreateAttributeGroup("tab-1", "Tab 1", 0);


            AttributeType typeDefinition = HiveModelCreationHelper.CreateAttributeType("test-type-alias",
                                                                                                 "test-type-name",
                                                                                                 "test-type-description");

            AttributeDefinition attributeDefinition = HiveModelCreationHelper.CreateAttributeDefinition("test-attrib-alias",
                                                                                                        "test-attrib-name", "test-description",
                                                                                                        typeDefinition,
                                                                                                        groupDefinition);

            EntitySchema schemaDef = HiveModelCreationHelper.CreateEntitySchema("test-schema", "Test Schema", new[] { attributeDefinition });


            TypedAttribute attribute = HiveModelCreationHelper.CreateAttribute(attributeDefinition, "my-test-value");

            // We use a factory here so that when running two tests, below (one direct and one via hive) we don't have
            // a generated id spanning a Session.Clear() call into the next test where that Id will not be found
            Func<TypedEntity> entity = () => HiveModelCreationHelper.CreateTypedEntity(schemaDef, new[] { attribute });

            PersistenceTestBuilder.Create<TypedEntity>(PostWriteCallback)
                .AddAssertStringEquals(x => x.Attributes.First().AttributeDefinition.Alias, "Attribute definition alias")
                .AddAssertStringEquals(x => x.Attributes.First().AttributeDefinition.Name, "Attribute definition name")
                .AddAssertIsTrue(x => x.Attributes.First().AttributeDefinition.AttributeType.Id, (y, z) => y == z, "AttributeDefinition.AttributeType.Id")
                .AddAssertDateTimeOffset(x => x.UtcCreated, "Date created")
                .AddAssertDateTimeOffset(x => x.Attributes.First().AttributeDefinition.AttributeType.UtcCreated, "AttributeDefinition.AttributeType date created")
                .AddAssertStringEquals(x => x.Attributes.First().DynamicValue, "Value")
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, entity)
                .RunAssertions(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, entity);
        }

        [TestMethod]
        public virtual void SaveAndLoadAttributeDefinition()
        {
            AttributeGroup groupDefinition = HiveModelCreationHelper.CreateAttributeGroup("tab-1", "Tab 1", 0);

            AttributeType typeDefinition = HiveModelCreationHelper.CreateAttributeType("test-type-alias",
                                                                                                 "test-type-name",
                                                                                                 "test-type-description");

            AttributeDefinition attribDef = HiveModelCreationHelper.CreateAttributeDefinition("test-attrib-alias",
                                                                                              "test-attrib-name", "test-description",
                                                                                              typeDefinition,
                                                                                              groupDefinition);

            EntitySchema schemaDef = HiveModelCreationHelper.CreateEntitySchema("test-schema", "Test Schema", new[] { attribDef });

            PersistenceTestBuilder.Create<EntitySchema>(PostWriteCallback)
                .AddAssertStringEquals(x => x.Name, "Name")
                .AddAssertStringEquals(x => x.Alias, "Alias")
                .AddAssertIsTrue(x => x.AttributeDefinitions.First().AttributeType.Id, (y, z) => y == z, "AttributeType Id")
                .AddAssertIsTrue(x => x.AttributeDefinitions.First().Alias, (y, z) => y == z, "First attrib def Alias")
                .AddAssertIsTrue(x => x.AttributeDefinitions.First().Name, (y, z) => y == z, "First attrib def Name")
                .AddAssertIsTrue(x => x.AttributeDefinitions.First().Description, (y, z) => y == z, "First attrib def Description")
                .AddAssertDateTimeOffset(x => x.UtcCreated, "Date created")
                .AddAssertDateTimeOffset(x => x.AttributeDefinitions.First().AttributeType.UtcCreated, "AttributeType date created")
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, schemaDef)
                .RunAssertions(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, schemaDef);
        }

        [TestMethod]
        public virtual void SaveAndLoadAttributeGroup()
        {
            AttributeGroup groupDefinition = HiveModelCreationHelper.CreateAttributeGroup("tab-1", "Tab 1", 0);

            AttributeType typeDefinition = HiveModelCreationHelper.CreateAttributeType("test-type-alias",
                                                                                                 "test-type-name",
                                                                                                 "test-type-description");
            AttributeType typeDefinition2 = HiveModelCreationHelper.CreateAttributeType("test-type-alias2",
                                                                                                  "test-type-name2",
                                                                                                  "test-type-description2");

            AttributeDefinition attribDef1 = HiveModelCreationHelper.CreateAttributeDefinition("alias-1", "name-1", "test-description", typeDefinition, groupDefinition);
            AttributeDefinition attribDef2 = HiveModelCreationHelper.CreateAttributeDefinition("alias-2", "name-2", "test-description", typeDefinition, groupDefinition);
            AttributeDefinition attribDef3 = HiveModelCreationHelper.CreateAttributeDefinition("alias-3", "name-3", "test-description", typeDefinition2, groupDefinition);
            AttributeDefinition attribDef4 = HiveModelCreationHelper.CreateAttributeDefinition("alias-4", "name-4", "test-description", typeDefinition2, groupDefinition);

            EntitySchema schemaDef = HiveModelCreationHelper.CreateEntitySchema("test-schema", "Test Schema", new[] { attribDef1, attribDef2, attribDef3, attribDef4 });

            PersistenceTestBuilder.Create<EntitySchema>(PostWriteCallback)
                .AddAssertStringEquals(x => x.Name, "Name")
                .AddAssertStringEquals(x => x.Alias, "Alias")
                .AddAssertIntegerEquals(x => x.AttributeGroups.Count(), "Group count")
                .AddAssertStringEquals(x => x.AttributeGroups.First().Alias, "First group alias")
                //.AddAssertIsTrue(x => x.AttributeDefinitions[0].Id, (x, y) => x == y, "AttributeDefinitions[0].Id")
                //.AddAssertIsTrue(x => x.AttributeDefinitions[1].Id, (x, y) => x == y, "AttributeDefinitions[1].Id")
                //.AddAssertIsTrue(x => x.AttributeDefinitions[2].Id, (x, y) => x == y, "AttributeDefinitions[2].Id")
                //.AddAssertIsTrue(x => x.AttributeDefinitions[3].Id, (x, y) => x == y, "AttributeDefinitions[3].Id")
                //.AddAssertIsTrue(x => x.AttributeDefinitions[0].AttributeType.Id, (x, y) => x == y, "AttributeDefinitions[0].AttributeType.Id")
                //.AddAssertIsTrue(x => x.AttributeDefinitions[0].AttributeType.Name, (x, y) => x == y, "AttributeDefinitions[0].AttributeType.Name")
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, schemaDef)
                .RunAssertions(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, schemaDef);
        }

        [TestMethod]
        public virtual void SaveAndLoadTypedEntity()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity();

            PersistenceTestBuilder.Create<TypedEntity>(PostWriteCallback)
                .AddAssertStringEquals(x => x.Attributes.Single(a => a.AttributeDefinition.Alias == "alias-1").DynamicValue, "First attribute value")
                .AddAssertStringEquals(x => x.Attributes.Single(a => a.AttributeDefinition.Alias == "alias-2").DynamicValue, "Second attribute value")
                .AddAssertStringEquals(x => x.Attributes.Single(a => a.AttributeDefinition.Alias == "alias-3").DynamicValue, "Third attribute value")
                .AddAssertStringEquals(x => x.Attributes.Single(a => a.AttributeDefinition.Alias == "alias-4").DynamicValue, "Fourth attribute value")
                .AddAssertStringEquals(x =>
                                       x.Attributes.Single(a => a.AttributeDefinition.Alias == NodeNameAttributeDefinition.AliasValue).DynamicValue,
                                       "NodeName attribute value")
                .AddAssertStringEquals(x =>
                                       x.Attributes.Single(a => a.AttributeDefinition.Alias == SelectedTemplateAttributeDefinition.AliasValue).DynamicValue,
                                       "SelectedTemplate attribute value")
                .AddAssertStringEquals(x => x.EntitySchema.Alias, "EntitySchema.Alias")
                .AddAssertStringEquals(x => x.EntitySchema.AttributeDefinitions.OrderBy(y => y.Alias).ElementAt(0).Alias,
                                       "EntitySchema.AttributeDefinitions[0].Alias")
                .RunAssertions(DirectReaderProvider, DirectReadWriteProvider, entity)
                .RunAssertions(ReaderProviderViaHiveGovernor, ReadWriteProviderViaHiveGovernor, entity);
        }

        [TestMethod]
        public virtual void RepoMethods_GetEntities_TypedEntity_ReturnsResults()
        {
            TypedEntity entity1 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity2 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity3 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity4 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity5 = HiveModelCreationHelper.MockTypedEntity();

            using (IReadWriteUnitOfWork writer = DirectReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(entity1);
                writer.ReadWriteRepository.AddOrUpdate(entity2);
                writer.ReadWriteRepository.AddOrUpdate(entity3);
                writer.ReadWriteRepository.AddOrUpdate(entity4);
                writer.ReadWriteRepository.AddOrUpdate(entity5);
                writer.Commit();
            }

            PostWriteCallback.Invoke();

            using (IReadOnlyUnitOfWork reader = DirectReaderProvider.CreateReadOnlyUnitOfWork())
            {
                IEnumerable<TypedEntity> result = reader.ReadRepository.GetEntities<TypedEntity>();
                Assert.AreEqual(5, result.Count());
            }
        }

        [TestMethod]
        public virtual void RepoMethods_GetEntities_Schema_ReturnsResults()
        {
            EntitySchema entity1 = HiveModelCreationHelper.MockEntitySchema("test1", "test");
            EntitySchema entity2 = HiveModelCreationHelper.MockEntitySchema("test2", "test");
            EntitySchema entity3 = HiveModelCreationHelper.MockEntitySchema("test3", "test");
            EntitySchema entity4 = HiveModelCreationHelper.MockEntitySchema("test4", "test");
            EntitySchema entity5 = HiveModelCreationHelper.MockEntitySchema("test5", "test");

            using (IReadWriteUnitOfWork writer = DirectReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(entity1);
                writer.ReadWriteRepository.AddOrUpdate(entity2);
                writer.ReadWriteRepository.AddOrUpdate(entity3);
                writer.ReadWriteRepository.AddOrUpdate(entity4);
                writer.ReadWriteRepository.AddOrUpdate(entity5);
                writer.Commit();
            }

            PostWriteCallback.Invoke();

            using (IReadOnlyUnitOfWork reader = DirectReaderProvider.CreateReadOnlyUnitOfWork())
            {
                IEnumerable<EntitySchema> result = reader.ReadRepository.GetEntities<EntitySchema>();
                Assert.AreEqual(5, result.Count());
            }
        }

        [TestMethod]
        public virtual void Schema_AttributeGroups_DeleteAfterRemovalFromCollection()
        {
            AttributeGroup group1 = HiveModelCreationHelper.CreateAttributeGroup("group1", "group", 0);
            AttributeGroup group2 = HiveModelCreationHelper.CreateAttributeGroup("group2", "group", 1);
            AttributeGroup group3 = HiveModelCreationHelper.CreateAttributeGroup("group3", "group", 2);
            AttributeGroup group4 = HiveModelCreationHelper.CreateAttributeGroup("group4", "group", 3);

            AttributeDefinition attrib1 = HiveModelCreationHelper.CreateAttributeDefinition("test1", "test", "test", HiveModelCreationHelper.CreateAttributeType("type", "type", "type"), group1);
            AttributeDefinition attrib2 = HiveModelCreationHelper.CreateAttributeDefinition("test2", "test", "test", HiveModelCreationHelper.CreateAttributeType("type", "type", "type"), group2);
            AttributeDefinition attrib3 = HiveModelCreationHelper.CreateAttributeDefinition("test3", "test", "test", HiveModelCreationHelper.CreateAttributeType("type", "type", "type"), group3);
            AttributeDefinition attrib4 = HiveModelCreationHelper.CreateAttributeDefinition("test4", "test", "test", HiveModelCreationHelper.CreateAttributeType("type", "type", "type"), group4);

            EntitySchema schema = HiveModelCreationHelper.CreateEntitySchema("schema", "schema", null);
            schema.AttributeGroups.Add(group1);
            schema.AttributeGroups.Add(group2);
            schema.AttributeDefinitions.Add(attrib3);
            schema.AttributeDefinitions.Add(attrib4);

            using (IReadWriteUnitOfWork writer = DirectReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(schema);
                writer.Commit();

                Assert.AreEqual(4, schema.AttributeGroups.Count, "Group counts differ after saving");
                Assert.AreEqual(2, schema.AttributeDefinitions.Count, "Def counts != 2 after saving");
            }

            PostWriteCallback.Invoke();

            using (IReadOnlyUnitOfWork reader = DirectReaderProvider.CreateReadOnlyUnitOfWork())
            {
                var schemaReloaded = reader.ReadRepository.GetEntity<EntitySchema>(schema.Id);
                Assert.AreEqual(4, schemaReloaded.AttributeGroups.Count, "Group counts differ");
                Assert.AreEqual(2, schemaReloaded.AttributeDefinitions.Count, "Def counts != 2");
            }

            PostWriteCallback.Invoke();

            HiveId deletedGroup;
            using (IReadWriteUnitOfWork writer = DirectReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                var schemaReloaded = writer.ReadWriteRepository.GetEntity<EntitySchema>(schema.Id);
                Assert.IsNotNull(schemaReloaded, "Could not get item from db");

                AttributeGroup groupToDelete = schemaReloaded.AttributeGroups.OrderBy(x => x.Ordinal).ElementAt(2);
                deletedGroup = groupToDelete.Id;
                schemaReloaded.AttributeGroups.Remove(groupToDelete);

                Assert.AreEqual(3, schemaReloaded.AttributeGroups.Count, "Removal didn't affect count");
                Assert.AreEqual(1, schemaReloaded.AttributeDefinitions.Count, "Removal didn't affect attrib def count");

                writer.ReadWriteRepository.AddOrUpdate(schemaReloaded);
                writer.Commit();
            }

            PostWriteCallback.Invoke();

            using (IReadOnlyUnitOfWork reader = DirectReaderProvider.CreateReadOnlyUnitOfWork())
            {
                var schemaReloaded = reader.ReadRepository.GetEntity<EntitySchema>(schema.Id);
                Assert.IsNotNull(schemaReloaded, "Could not get item from db");
                Assert.AreEqual(3, schemaReloaded.AttributeGroups.Count, "Removal didn't affect count after reloading");
                Assert.IsFalse(schemaReloaded.AttributeGroups.Any(x => x.Id == deletedGroup), "Deleted group still exists - wrong one was deleted");
            }
        }

        [TestMethod]
        public virtual void Read_ReadWrite_ReposReturnSameResultsFromSameProvider()
        {
            DoSameResultsCheck(DirectReadWriteProvider, DirectReaderProvider);
        }

        [TestMethod]
        public virtual void Read_ReadWrite_ReposReturnSameResultsFromHive()
        {
            DoSameResultsCheck(ReadWriteProviderViaHiveGovernor, ReaderProviderViaHiveGovernor);
        }

        [TestMethod]
        public virtual void EntityRelations_GetByRoute()
        {
            TypedEntity entityParent = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entityChild = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entityGrandchild = HiveModelCreationHelper.MockTypedEntity();

            entityParent.Attributes.SetValueOrAdd(new NodeNameAttribute("homepage"));
            entityChild.Attributes.SetValueOrAdd(new NodeNameAttribute("news"));
            entityGrandchild.Attributes.SetValueOrAdd(new NodeNameAttribute("newsitem1"));

            var root = new SystemRoot();
            root.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityParent);
            entityParent.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityChild);
            entityChild.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityGrandchild);

            using (IReadWriteUnitOfWork writer = DirectReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(root);
                writer.Commit();
            }

            using (DisposableTimer timer = DisposableTimer.TraceDuration<AbstractHivePersistenceTest>("Start perf test", "End perf test"))
            {
                for (int i = 0; i < 5; i++)
                {
                    PostWriteCallback.Invoke();

                    using (IReadOnlyUnitOfWork reader = DirectReaderProvider.CreateReadOnlyUnitOfWork())
                    {
                        var nhReader = reader.ReadRepository as RepositoryReader;
                        var hp = nhReader.GetByPath<TypedEntity>(new HiveId("/homepage/"), FixedRelationTypes.ContentTreeRelationType, FixedStatusTypes.Created);
                        var news = nhReader.GetByPath<TypedEntity>(new HiveId("/homepage/news"), FixedRelationTypes.ContentTreeRelationType, FixedStatusTypes.Created);
                        var newsitem1 = nhReader.GetByPath<TypedEntity>(new HiveId("/homepage/news/newsitem1"), FixedRelationTypes.ContentTreeRelationType, FixedStatusTypes.Created);
                        Assert.IsNotNull(hp);
                        Assert.IsNotNull(news);
                        Assert.IsNotNull(newsitem1);

                        var newsitem1_bad1 = nhReader.GetByPath<TypedEntity>(new HiveId("/homepage/newsitem1"));
                        var newsitem1_bad2 = nhReader.GetByPath<TypedEntity>(new HiveId("newsitem1"));
                        Assert.IsNull(newsitem1_bad1);
                        Assert.IsNull(newsitem1_bad2);
                    }
                }

                LogHelper.TraceIfEnabled<AbstractHivePersistenceTest>("Avg of {0}ms per run", () => timer.Stopwatch.ElapsedMilliseconds / 5);
            }
        }

        [TestMethod]
        public virtual void EntityRelations_CachePopulates()
        {
            TypedEntity entityParent = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entityChild = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entitySibling = HiveModelCreationHelper.MockTypedEntity();

            var root = new SystemRoot();
            root.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityParent);
            entityParent.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityChild);
            entityParent.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entitySibling);

            using (IReadWriteUnitOfWork writer = DirectReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(root);
                writer.Commit();
            }

            PostWriteCallback.Invoke();

            using (IReadOnlyUnitOfWork reader = DirectReaderProvider.CreateReadOnlyUnitOfWork())
            {
                IEnumerable<TypedEntity> getEntities = reader.ReadRepository.GetEntities<TypedEntity>();
                Assert.AreEqual(4, getEntities.Count());
            }

            Assert.Inconclusive("Not finished");
        }

        [TestMethod]
        public virtual void SaveAndLoadTypedEntityWithNestedTransaction_UsingProvider()
        {
            DoNestedTest(DirectReadWriteProvider, DirectReaderProvider, PostWriteCallback);
            DoNestedTest(DirectReadWriteProvider, DirectReaderProvider, PostWriteCallback);
        }

        [TestMethod]
        public virtual void SaveAndLoadTypedEntityWithNestedTransaction_UsingHive()
        {
            DoNestedTest(ReadWriteProviderViaHiveGovernor, ReaderProviderViaHiveGovernor, PostWriteCallback);
            DoNestedTest(ReadWriteProviderViaHiveGovernor, ReaderProviderViaHiveGovernor, PostWriteCallback);
        }

        [TestMethod]
        public virtual void SaveAndLoadTypedEntityWithNestedTransaction_UsingProviderThenHive()
        {
            DoNestedTest(DirectReadWriteProvider, DirectReaderProvider, PostWriteCallback);
            DoNestedTest(ReadWriteProviderViaHiveGovernor, ReaderProviderViaHiveGovernor, PostWriteCallback);
        }

        private static void DoNestedTest(IHiveReadWriteProvider hiveReadWriteProvider, IHiveReadProvider hiveReadProvider, Action postWriteCallback)
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity2 = HiveModelCreationHelper.MockTypedEntity();

            using (IReadWriteUnitOfWork outerUnit = hiveReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                outerUnit.ReadWriteRepository.AddOrUpdate(entity);

                // Do some nested writing
                using (IReadWriteUnitOfWork innerUnit = hiveReadWriteProvider.CreateReadWriteUnitOfWork())
                {
                    innerUnit.ReadWriteRepository.AddOrUpdate(entity2);
                    innerUnit.Commit();
                }

                // Do some nested reading
                using (IReadOnlyUnitOfWork innerUnit = hiveReadProvider.CreateReadOnlyUnitOfWork())
                {
                    innerUnit.ReadRepository.Exists<TypedEntity>(new HiveId(Guid.NewGuid()));
                }

                // Do some nested reading with the writer
                using (IReadWriteUnitOfWork innerUnit = hiveReadWriteProvider.CreateReadWriteUnitOfWork())
                {
                    innerUnit.ReadWriteRepository.Exists<TypedEntity>(new HiveId(Guid.NewGuid()));
                }

                // Do some nested writing
                using (IReadWriteUnitOfWork innerUnit = hiveReadWriteProvider.CreateReadWriteUnitOfWork())
                {
                    innerUnit.ReadWriteRepository.AddOrUpdate(entity2);
                    innerUnit.Commit();
                }

                outerUnit.Commit();
            }

            HiveId id1 = entity.Id;
            HiveId id2 = entity2.Id;

            using (IReadWriteUnitOfWork outerUnit = hiveReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                // Do some nested reading
                using (IReadOnlyUnitOfWork innerUnit = hiveReadProvider.CreateReadOnlyUnitOfWork())
                {
                    innerUnit.ReadRepository.Exists<TypedEntity>(new HiveId(Guid.NewGuid()));
                    //innerUnit.Commit();
                }

                outerUnit.ReadWriteRepository.AddOrUpdate(entity);

                outerUnit.Commit();
            }

            postWriteCallback.Invoke();

            using (IReadOnlyUnitOfWork unit = hiveReadProvider.CreateReadOnlyUnitOfWork())
            {
                var compare = unit.ReadRepository.GetEntity<TypedEntity>(id1);
                var compare2 = unit.ReadRepository.GetEntity<TypedEntity>(id2);

                Assert.IsNotNull(compare);
                Assert.IsNotNull(compare2);
            }
        }

        private void DoSameResultsCheck(IHiveReadWriteProvider hiveReadWriteProvider, IHiveReadProvider hiveReadProvider)
        {
            TypedEntity entity1 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity2 = HiveModelCreationHelper.MockTypedEntity();

            using (IReadWriteUnitOfWork writer = hiveReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                writer.ReadWriteRepository.AddOrUpdate(entity1);
                writer.ReadWriteRepository.AddOrUpdate(entity2);
                writer.Commit();
            }

            PostWriteCallback.Invoke();

            using (IReadWriteUnitOfWork writer = hiveReadWriteProvider.CreateReadWriteUnitOfWork())
            {
                var check1 = writer.ReadWriteRepository.GetEntity<TypedEntity>(entity1.Id);
                Assert.IsNotNull(check1, "Same writer could not retrieve item 1");
                var check2 = writer.ReadWriteRepository.GetEntity<TypedEntity>(entity2.Id);
                Assert.IsNotNull(check2, "Same writer could not retrieve item 2");
            }

            PostWriteCallback.Invoke();

            using (IReadOnlyUnitOfWork writer = hiveReadProvider.CreateReadOnlyUnitOfWork())
            {
                var check1 = writer.ReadRepository.GetEntity<TypedEntity>(entity1.Id);
                Assert.IsNotNull(check1, "Direct reader could not retrieve item 1 when writer could");
                var check2 = writer.ReadRepository.GetEntity<TypedEntity>(entity2.Id);
                Assert.IsNotNull(check2, "Direct reader could not retrieve item 2 when writer could");
            }
        }
    }
}