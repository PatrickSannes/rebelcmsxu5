using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.System;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs.PropertyEditors;
using Umbraco.Cms.Web.DependencyManagement;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders
{
    using System.Threading;
    using Umbraco.Cms.Web.Security;

    /// <summary>
    /// Tests for Hive provider that implement the full Hive provider spectrum
    /// </summary>
    public abstract class AbstractProviderTests : AbstractEntityOnlyProviderTests
    {
        [Test]
        public void QueryVisitor_AcceptsPredicatesWithNullValues()
        {
            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var groupUnit = new ReadonlyGroupUnit<IProviderTypeFilter>(new[] { reader }, new Uri("whatever://"), new DictionaryScopedCache(), true, reader.FrameworkContext, FakeHiveCmsManager.CreateFakeRepositoryContext(reader.FrameworkContext));
                var results = groupUnit.Repositories.Where(x => x.Attribute<string>("Alias") == null).ToArray();
            }
        }
        
        private static EntitySchema CreateAndSaveCompositeSchema(IAttributeTypeRegistry attributeTypeRegistry, ProviderSetup providerSetup)
        {
            var nameNameType = attributeTypeRegistry.GetAttributeType(NodeNameAttributeType.AliasValue);
            var textstringType = attributeTypeRegistry.GetAttributeType("singleLineTextBox");

            var schema1 = HiveModelCreationHelper.CreateEntitySchema("schema1", "Schema1", true, new[] { new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
            {
                Id = new HiveId("mi-schema1-name".EncodeAsGuid()),
                AttributeType = nameNameType,
                AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                Ordinal = 0
            }, new AttributeDefinition("alias1", "Alias1")
            {
                Id = new HiveId("mi-schema1-alias1".EncodeAsGuid()),
                AttributeType = textstringType,
                AttributeGroup = new AttributeGroup("group1", "Group 1", 50).FixedDates(),
                Ordinal = 1
            } });
            schema1.AttributeGroups.Add(new AttributeGroup("empty-group", "Empty Group", 100).FixedDates());

            var schema2 = HiveModelCreationHelper.CreateEntitySchema("schema2", "Schema2", true, new[] { new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
            {
                Id = new HiveId("mi-schema2-name".EncodeAsGuid()),
                AttributeType = nameNameType,
                AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                Ordinal = 0
            }, new AttributeDefinition("alias2", "Alias2")
            {
                Id = new HiveId("mi-schema2-alias2".EncodeAsGuid()),
                AttributeType = textstringType,
                AttributeGroup = new AttributeGroup("group1", "Group 1", 50).FixedDates(),
                Ordinal = 1
            } });

            var schema3 = HiveModelCreationHelper.CreateEntitySchema("schema3", "Schema3", true, new[] { new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
            {
                Id = new HiveId("mi-schema3-name".EncodeAsGuid()),
                AttributeType = nameNameType,
                AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                Ordinal = 0
            }, new AttributeDefinition("alias3", "Alias3")
            {
                Id = new HiveId("mi-schema3-alias3".EncodeAsGuid()),
                AttributeType = textstringType,
                AttributeGroup = new AttributeGroup("group2", "Group 2", 100).FixedDates(),
                Ordinal = 1
            } });

            schema3.XmlConfiguration = XDocument.Parse("<test />");

            // Act
            using (var uow = providerSetup.UnitFactory.Create())
            {
                // Add schemas
                uow.EntityRepository.Schemas.AddOrUpdate(schema1);
                uow.EntityRepository.Schemas.AddOrUpdate(schema2);
                uow.EntityRepository.Schemas.AddOrUpdate(schema3);

                // Add relations
                uow.EntityRepository.Schemas.AddRelation(new Relation(FixedRelationTypes.DefaultRelationType, schema1.Id, schema2.Id));
                uow.EntityRepository.Schemas.AddRelation(new Relation(FixedRelationTypes.DefaultRelationType, schema2.Id, schema3.Id));

                uow.Complete();
            }

            return schema3;
        }

        [Test]
        public void GetCompositeEntity_Returns_Correct_Attributes()
        {
            // Arrange
            var childSchema = CreateAndSaveCompositeSchema(AttributeTypeRegistry, ProviderSetup);
            CompositeEntitySchema merged = null;
            var groupUnitFactory = new GroupUnitFactory(ProviderSetup, childSchema.Id.ToUri(), FakeHiveCmsManager.CreateFakeRepositoryContext(ProviderSetup.FrameworkContext));
            using (var uow = groupUnitFactory.Create())
            {
                merged = uow.Repositories.Schemas.GetComposite<EntitySchema>(childSchema.Id);
            }

            List<TypedAttribute> attribs;
            var entity = MockCompositeEntity(merged, out attribs);
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity);

            Assert.That(entity.Attributes.Count, Is.EqualTo(attribs.Count));

            using (var uow = groupUnitFactory.Create())
            {
                uow.Repositories.AddOrUpdate(entity);
                uow.Complete();
            }

            PostWriteCallback.Invoke();

            using (var uow = groupUnitFactory.Create())
            {
                var reloaded = uow.Repositories.Get<TypedEntity>(entity.Id);
                Assert.That(reloaded.EntitySchema.Id.Value, Is.EqualTo(childSchema.Id.Value));
                Assert.That(reloaded.Attributes.Count, Is.EqualTo(attribs.Count));
            }
        }

        private static TypedEntity MockCompositeEntity(CompositeEntitySchema merged, out List<TypedAttribute> attribs)
        {
            attribs = new List<TypedAttribute>();
            foreach (
                var inheritedAttributeDefinition in
                    merged.InheritedAttributeDefinitions.Concat(merged.AttributeDefinitions).DistinctBy(x => x.Alias))
            {
                attribs.Add(new TypedAttribute(inheritedAttributeDefinition, "hello"));
            }

            var entity = HiveModelCreationHelper.CreateTypedEntity(merged, attribs.ToArray());
            return entity;
        }

        [Test]
        public void CompositeEntity_ReSaves()
        {
            // Arrange
            var childSchema = CreateAndSaveCompositeSchema(AttributeTypeRegistry, ProviderSetup);
            CompositeEntitySchema merged = null;
            var groupUnitFactory = new GroupUnitFactory(ProviderSetup, childSchema.Id.ToUri(), FakeHiveCmsManager.CreateFakeRepositoryContext(ProviderSetup.FrameworkContext));
            using (var uow = groupUnitFactory.Create())
            {
                merged = uow.Repositories.Schemas.GetComposite<EntitySchema>(childSchema.Id);
            }

            List<TypedAttribute> attribs;
            var entity = MockCompositeEntity(merged, out attribs);
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity);

            Assert.That(entity.Attributes.Count, Is.EqualTo(attribs.Count));

            var firstRevision = new Revision<TypedEntity>(entity);

            using (var uow = groupUnitFactory.Create())
            {
                uow.Repositories.Revisions.AddOrUpdate(firstRevision);
                uow.Complete();
            }

            PostWriteCallback.Invoke();

            // Edit the data and resave
            var secondRevision = firstRevision.CopyToNewRevision(FixedStatusTypes.Published);
            secondRevision.Item.Attributes.Last().DynamicValue = "changed";
            secondRevision.Item.Attributes.ForEach(x => x.Id = HiveId.Empty);

            using (var uow = groupUnitFactory.Create())
            {
                uow.Repositories.Revisions.AddOrUpdate(secondRevision);
                uow.Complete();
            }

            PostWriteCallback.Invoke();

            // Load the data again to ensure these attributes are still "inherited"
            using (var uow = groupUnitFactory.Create())
            {
                var reloaded = uow.Repositories.Get<TypedEntity>(entity.Id);
                Assert.That(reloaded.EntitySchema.Id.Value, Is.EqualTo(childSchema.Id.Value));
                Assert.That(reloaded.Attributes.Count, Is.EqualTo(attribs.Count));
                Assert.That(reloaded.Attributes.Select(x => x.AttributeDefinition), Has.Some.TypeOf<InheritedAttributeDefinition>());
            }
        }

        [Test]
        public void GetCompositeSchema_ReturnsOnlyUniqueAttributeDefinitions()
        {
            // Arrange
            var schema3 = CreateAndSaveCompositeSchema(AttributeTypeRegistry, ProviderSetup);

            // Assert
            var groupUnitFactory = new GroupUnitFactory(ProviderSetup, schema3.Id.ToUri(), FakeHiveCmsManager.CreateFakeRepositoryContext(ProviderSetup.FrameworkContext));
            using (var uow = groupUnitFactory.Create())
            {
                var merged = uow.Repositories.Schemas.GetComposite<EntitySchema>(schema3.Id);
                var defs = merged.AttributeDefinitions.Select(x => x.Alias).ToArray();
                var inheritedDefs = merged.InheritedAttributeDefinitions.Select(x => x.Alias).ToArray();

                Assert.That(defs, Is.Unique);
                Assert.That(inheritedDefs, Is.Unique);

                Assert.That(defs.Length, Is.GreaterThan(0));
                Assert.That(inheritedDefs.Length, Is.GreaterThan(0));

                Assert.That(inheritedDefs, Is.Not.SubsetOf(defs));
                Assert.That(defs, Is.Not.SubsetOf(inheritedDefs));
            }
        }

        [Test]
        public void GetComposite_Returns_Composite_Schema()
        {
            // Arrange
            var schema3 = CreateAndSaveCompositeSchema(AttributeTypeRegistry, ProviderSetup);

            // Assert
            var groupUnitFactory = new GroupUnitFactory(ProviderSetup, schema3.Id.ToUri(), FakeHiveCmsManager.CreateFakeRepositoryContext(ProviderSetup.FrameworkContext));
            using (var uow = groupUnitFactory.Create())
            {
                var merged = uow.Repositories.Schemas.GetComposite<EntitySchema>(schema3.Id);

                Assert.IsTrue(merged.Id.Value == schema3.Id.Value);
                Assert.AreEqual(2, merged.AttributeGroups.Count);
                Assert.AreEqual(2, merged.AttributeDefinitions.Count);
                Assert.AreEqual(4, merged.InheritedAttributeGroups.Count);
                Assert.AreEqual(3, merged.InheritedAttributeDefinitions.Count);
                Assert.AreEqual("<test />", merged.XmlConfiguration.ToString());

                Assert.That(merged.AllAttributeGroups.Count(), Is.EqualTo(merged.AttributeGroups.Count + merged.InheritedAttributeGroups.Count));
                Assert.That(merged.AllAttributeDefinitions.Count(), Is.EqualTo(merged.AttributeDefinitions.Count + merged.InheritedAttributeDefinitions.Count));

                Assert.IsTrue(merged.InheritedAttributeGroups.Any(x => x.Alias == "empty-group"));
            }
        }

        [Test]
        public void Returned_TypedEntity_Has_Same_AttributeGroup_Instances_In_Graph()
        {
            var entity = HiveModelCreationHelper.MockVersionedTypedEntity();

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.Revisions.AddOrUpdate(entity);
                writer.Complete();
            }
            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var rev = reader.EntityRepository.Revisions.GetLatestRevision<TypedEntity>(entity.Item.Id);
                var result = rev.Item;
                var entityGroups = result.AttributeGroups.ToArray();
                var entityAttributeGroups = result.Attributes.Select(x => x.AttributeDefinition.AttributeGroup).Distinct().ToArray();
                var schemaGroups = result.EntitySchema.AttributeGroups.ToArray();
                var schemaAttDefGroups = result.EntitySchema.AttributeDefinitions.Select(x => x.AttributeGroup).Distinct().ToArray();

                Assert.AreEqual(entityGroups.Count(), schemaGroups.Count());
                Assert.AreEqual(entityGroups.Count(), entityAttributeGroups.Count());
                Assert.AreEqual(schemaGroups.Count(), schemaAttDefGroups.Count());
                Assert.AreEqual(entityGroups.Count(), schemaAttDefGroups.Count());

                foreach(var group in entityGroups)
                {
                    Assert.IsTrue(ReferenceEquals(group, schemaGroups.Single(x => x.Id == group.Id)));
                    Assert.IsTrue(ReferenceEquals(group, schemaAttDefGroups.Single(x => x.Id == group.Id)));
                    Assert.IsTrue(ReferenceEquals(group, entityAttributeGroups.Single(x => x.Id == group.Id)));
                }
            }
        }

        [Test]
        public void Returned_EntitySchema_Has_Same_AttributeType_Instances_As_Its_Attribute_Definitions_AttributeType()
        {
            var schema = HiveModelCreationHelper.MockEntitySchema("test", "Test");

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.Schemas.AddOrUpdate(schema);
                writer.Complete();
            }
            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var result = reader.EntityRepository.Schemas.Get<EntitySchema>(schema.Id);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.AttributeTypes.Count(), result.AttributeDefinitions.Select(x => x.AttributeType).Distinct().Count());//(new AttributeTypeDefinitionComparer()).Count());
                foreach(var a in result.AttributeDefinitions)
                {
                    var matchingAttributeType = result.AttributeTypes.Where(x => x.Alias == a.AttributeType.Alias).SingleOrDefault();
                    Assert.IsNotNull(matchingAttributeType);
                    Assert.IsTrue(ReferenceEquals(matchingAttributeType, a.AttributeType));
                }
            }
        }

         [Test]
        public void Returned_EntitySchema_Has_Same_AttributeGroup_Instances_As_Its_Attribute_Definitions_AttributeGroup()
        {
            var schema = HiveModelCreationHelper.MockEntitySchema("test", "Test");

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.Schemas.AddOrUpdate(schema);
                writer.Complete();
            }
            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var result = reader.EntityRepository.Schemas.Get<EntitySchema>(schema.Id);
                Assert.IsNotNull(result);
                foreach(var a in result.AttributeDefinitions)
                {
                    var matchingAttributeGroup = result.AttributeGroups.Where(x => x.Alias == a.AttributeGroup.Alias).SingleOrDefault();
                    Assert.IsNotNull(matchingAttributeGroup);
                    Assert.IsTrue(ReferenceEquals(matchingAttributeGroup, a.AttributeGroup));
                }
            }
        }

        [Test]
        public void HavingAddedParentRelationProxy_ThenWhenAskingRepoForParents_WhithChildAsSource_ParentIsFound()
        {
            AddParentRelation((unit, root, entity) => entity.RelationProxies.EnlistParent(root, FixedRelationTypes.DefaultRelationType, 0));
        }

        [Test]
        public void HavingAddedParentRelation_ThenWhenAskingRepoForParents_WithChildAsSource_ParentIsFound()
        {
            AddParentRelation((unit, root, entity) => unit.EntityRepository.AddRelation(root, entity, FixedRelationTypes.DefaultRelationType, 0));
        }

        private void AddParentRelation(Action<ProviderUnit, SystemRoot, TypedEntity> addRelation)
        {
            // Arrange
            var parentRoot = new SystemRoot();
            var contentChild = HiveModelCreationHelper.MockTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, contentChild);

            // Act
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.AddOrUpdate(parentRoot);
                writer.EntityRepository.AddOrUpdate(contentChild);
                addRelation.Invoke(writer, parentRoot, contentChild);
                writer.Complete();
            }

            PostWriteCallback.Invoke();

            // Assert
            Assert.NotNull(parentRoot.RelationProxies.LazyLoadDelegate);
            Assert.NotNull(parentRoot.RelationProxies.FirstOrDefault());

            // Check loading the relation from the repo
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                var ancestorRelations = writer.EntityRepository.GetLazyAncestorRelations(contentChild.Id, FixedRelationTypes.DefaultRelationType);
                var ancestor = ancestorRelations.Where(x => x.SourceId == parentRoot.Id).FirstOrDefault();
                Assert.NotNull(ancestor);
                Assert.NotNull(ancestor.Source);
                Assert.AreEqual(ancestor.SourceId, parentRoot.Id);
                Assert.AreEqual(ancestor.DestinationId, contentChild.Id);

                var descendentRelations = writer.EntityRepository.GetLazyDescendentRelations(parentRoot.Id, FixedRelationTypes.DefaultRelationType);
                var descendent = descendentRelations.Where(x => x.DestinationId == contentChild.Id).FirstOrDefault();
                Assert.NotNull(descendent);
                Assert.NotNull(descendent.Source);
                Assert.AreEqual(descendent.SourceId, parentRoot.Id);
                Assert.AreEqual(descendent.DestinationId, contentChild.Id);
            }

            // Check loading the relation from the lazy-loaded collection on the entity
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                var reloadedRoot = writer.EntityRepository.Get<TypedEntity>(parentRoot.Id);
                var child = reloadedRoot.RelationProxies.FirstOrDefault();
                Assert.NotNull(child);
            }
        }

        [Test]
        public void Create_TypedEntity_CheckExists_ByExistsThenByGetting()
        {
            // Arrange
            var content = HiveModelCreationHelper.MockTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, content);

            // Act
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.AddOrUpdate(content);
                writer.Complete();
            }

            // Assert
            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                // Sanity-check that Schemas can't be loaded / exist just because we added content
                Assert.False(reader.EntityRepository.Schemas.Exists<EntitySchema>(content.Id));
                Assert.Null(reader.EntityRepository.Schemas.Get<EntitySchema>(content.Id));

                Assert.True(reader.EntityRepository.Exists<TypedEntity>(content.Id));
                Assert.NotNull(reader.EntityRepository.Get<TypedEntity>(content.Id));
            }
        }

        [Test]
        public void SchemaRepository_GetAll_Queries_For_All_AbstractSchemaParts()
        {
            // Arrange
            // Below we create three independent & distinct schemas, each with 2 group, 4 attribute types, and 6 attribute definitions
            // For non-passthrough provider tests, these will have no ids so will be considered totally separate and distinct
            var schema1 = HiveModelCreationHelper.MockEntitySchema("test-alias", "test-name");
            var schema2 = HiveModelCreationHelper.MockEntitySchema("test-alias", "test-name");
            var schema3 = HiveModelCreationHelper.MockEntitySchema("test-alias", "test-name");
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, schema1, schema2, schema3);

            // Act
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.Schemas.AddOrUpdate(schema1);
                writer.EntityRepository.Schemas.AddOrUpdate(schema2);
                writer.EntityRepository.Schemas.AddOrUpdate(schema3);
                writer.Complete();
            }

            // Assert
            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                Assert.AreEqual(3, reader.EntityRepository.Schemas.GetAll<EntitySchema>().Count());
                var attDefs = reader.EntityRepository.Schemas.GetAll<AttributeDefinition>().ToArray();
                Assert.AreEqual(18, attDefs.Count());
                Assert.AreEqual(6, reader.EntityRepository.Schemas.GetAll<AttributeGroup>().Count());
                Assert.AreEqual(12, reader.EntityRepository.Schemas.GetAll<AttributeType>().Count());
            }
        }

        [Test]
        [Category("Regression")]
        [Description("Tests to ensure that changes to an entity with a fixed Id such as an AttributeType can persist and be reloaded OK")]
        public void WhenSavingChangesTo_AttributeType_RenderTypeProvider_ChangesPersist()
        {
            // Arrange
            var attributeType = Framework.Persistence.AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue);
            Assert.NotNull(attributeType);
            attributeType.RenderTypeProvider = CorePluginConstants.TextBoxPropertyEditorId;
            Assert.That(attributeType.RenderTypeProvider, Is.EqualTo(CorePluginConstants.TextBoxPropertyEditorId));

            // Act
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.Schemas.AddOrUpdate(attributeType);
                writer.Complete();
            }
            PostWriteCallback.Invoke();

            // Assert
            Assert.That(attributeType.Id.Value, Is.EqualTo(FixedHiveIds.StringAttributeType.Value));
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                var loadType = writer.EntityRepository.Schemas.Get<AttributeType>(FixedHiveIds.StringAttributeType);
                Assert.NotNull(loadType);

                loadType.RenderTypeProvider = "something-new";
                writer.EntityRepository.Schemas.AddOrUpdate(loadType);
                writer.Complete();
            }
            PostWriteCallback.Invoke();

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                var loadTypeAgain = writer.EntityRepository.Schemas.Get<AttributeType>(FixedHiveIds.StringAttributeType);
                Assert.NotNull(loadTypeAgain);

                Assert.That(loadTypeAgain.RenderTypeProvider, Is.EqualTo("something-new"));
            }
        }

        [Test]
        public void Create_User_With_All_Ids_Asssigned_To_All_Objects()
        {
            var user = new User();

            // Act
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.AddOrUpdate(new SystemRoot());
                writer.EntityRepository.AddOrUpdate(FixedEntities.UserVirtualRoot);
                writer.EntityRepository.AddOrUpdate(user);
                writer.Complete();
            }

            foreach(var a in user.Attributes)
            {
                Assert.IsFalse(a.Id.IsNullValueOrEmpty());
            }
            foreach(var g in user.AttributeGroups)
            {
                Assert.IsFalse(g.Id.IsNullValueOrEmpty());
            }
            Assert.IsFalse(user.EntitySchema.Id.IsNullValueOrEmpty());
            foreach(var d in user.EntitySchema.AttributeDefinitions)
            {
                Assert.IsFalse(d.Id.IsNullValueOrEmpty());
            }
            foreach(var g in user.EntitySchema.AttributeGroups)
            {
                Assert.IsFalse(g.Id.IsNullValueOrEmpty());
            }
        }

        //NOTE: This would be a nice test to have but the Examine provider wont support it, see ExamienTransaction ExecuteCommit for more info.
        [Test]
        public void Ensures_InvalidOperationException_When_Adding_Entity_With_Relations_Without_Adding_Related_Entities()
        {
            var admin = new User()
            {
                Name = "Admin",
                Username = "admin",
                Email = "admin@admin.com",
                Password = "test"
            };

            Assert.Throws<InvalidOperationException>(() =>
                {
                    using (var writer = ProviderSetup.UnitFactory.Create())
                    {
                        writer.EntityRepository.AddOrUpdate(admin);
                        writer.Complete();
                    }
                });
        }

        [Test]
        public void Committing_Both_Entity_And_Schema_Results_In_No_Group_Overlap()
        {
            var userGuidForDebugging = Guid.Parse("00000000-0000-0000-0000-000000000BFC");
            var userGroup = new UserGroup();
            var user = new User() { Id = new HiveId(userGuidForDebugging) };
            var actualGroups =
                userGroup.EntitySchema.AttributeGroups
                    .Concat(userGroup.AttributeGroups)
                    .Concat(userGroup.Attributes.Select(x => x.AttributeDefinition.AttributeGroup))
                    .Concat(userGroup.EntitySchema.AttributeDefinitions.Select(x => x.AttributeGroup))
                    .Distinct();

            Assert.AreEqual(1, actualGroups.Count());

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.AddOrUpdate(new SystemRoot());
                writer.EntityRepository.AddOrUpdate(FixedEntities.UserVirtualRoot);
                writer.EntityRepository.AddOrUpdate(FixedEntities.UserGroupVirtualRoot);
                writer.EntityRepository.Schemas.AddOrUpdate(FixedSchemas.UserGroup);
                writer.EntityRepository.AddOrUpdate(user);
                writer.EntityRepository.AddOrUpdate(userGroup);                
                writer.Complete();
            }

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                var foundGroup =
                    writer.EntityRepository.GetEntityByRelationType<UserGroup>(
                        FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                        .Where(y => y.Name == userGroup.Name)
                        .FirstOrDefault();
                user.RelationProxies.EnlistParent(foundGroup, FixedRelationTypes.UserGroupRelationType);
                writer.EntityRepository.AddOrUpdate(user);
                writer.Complete();
            }
            
            using (var reader = ProviderSetup.UnitFactory.Create())
            {
                var output = reader.EntityRepository.Get<UserGroup>(true, userGroup.Id);
                Assert.AreEqual(1, output.Count());
                Assert.AreEqual(1, output.Single().AttributeGroups.Count());
                Assert.AreEqual(1, output.Single().EntitySchema.AttributeGroups.Count);
                Assert.AreEqual(1, output.Single().EntitySchema.AttributeDefinitions.Select(x => x.AttributeGroup).Distinct().Count());
                var allGroups = reader.EntityRepository.Schemas.GetAll<AttributeGroup>();
                Assert.AreEqual(1, allGroups.Count(x => x.Alias == FixedGroupDefinitions.UserGroupDetailsAlias));
                Assert.AreEqual(1, allGroups.Count(x => x.Alias == FixedGroupDefinitions.UserDetailsAlias));
            }
        }

        //[Test]
        //public void TestForProfiling()
        //{
        //    for (int i = 0; i < 10; i++)
        //    {
        //        Can_Save_And_Return_Subclass_Of_Typed_Entity();
        //    }
        //}

        [Test]
        public void Get_Entity_By_Relation_Type()
        {
            var admin = new User()
                {
                    Name = "Admin",
                    Username = "admin",
                    Email = "admin@admin.com",
                    Password = "test"
                };

            var myUser = new User()
            {
                Name = "A User",
                Username = "myUser",
                Email = "myUser@myUser.com",
                Password = "password"
            };

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.AddOrUpdate(new SystemRoot());
                writer.EntityRepository.AddOrUpdate(FixedEntities.UserVirtualRoot);
                writer.EntityRepository.AddOrUpdate(admin);
                writer.EntityRepository.AddOrUpdate(myUser);
                writer.Complete();
            }

            PostWriteCallback.Invoke();
            
            using (var reader = ProviderSetup.UnitFactory.Create())
            {
                var user = reader.EntityRepository.GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot).ToArray();
                Assert.AreEqual(2, user.Count());
            }            
        }

        [Test]
        public void Create_EntitySchema_CheckExists_ByExistsThenByGetting()
        {
            // Arrange
            var schema = HiveModelCreationHelper.MockEntitySchema("test-alias", "test-name");
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, schema);

            // Act
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.Schemas.AddOrUpdate(schema);
                writer.Complete();
            }

            // Assert
            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                // Sanity-check that content can't be loaded / exist just because we added schemas
                Assert.False(reader.EntityRepository.Exists<TypedEntity>(schema.Id));
                Assert.Null(reader.EntityRepository.Get<TypedEntity>(schema.Id));

                Assert.True(reader.EntityRepository.Schemas.Exists<EntitySchema>(schema.Id));
                Assert.NotNull(reader.EntityRepository.Schemas.Get<EntitySchema>(schema.Id));
            }
        }

        [Test]
        public void SaveAndLoad_InbuiltSchemas()
        {
            var originalSecurity = new UserGroupSchema();
            var systemRoot = new SystemRoot();

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.AddOrUpdate(systemRoot); // Need to add a system root because UserGroupSchema advertises that it's related to it
                writer.EntityRepository.Schemas.AddOrUpdate(originalSecurity);
                writer.Complete();
            }

            // Clear session to avoid caching when testing readbacks
            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                // Reload. Shouldn't matter that we're newing up the schemas to get the Id as Id should 
                // have been left alone when saving, so leave it here as a test case
                var securitySchema = reader.EntityRepository.Schemas.Get<EntitySchema>(originalSecurity.Id);

                Assert.IsNotNull(securitySchema, "Security schema");

                Assert.AreEqual(originalSecurity.AttributeDefinitions.Count, securitySchema.AttributeDefinitions.Count, "Security attrib defs count differ");
                Assert.AreEqual(originalSecurity.AttributeGroups.Count, securitySchema.AttributeGroups.Count, "Security attrib groups count differ");
                Assert.AreEqual(originalSecurity.AttributeTypes.Count(), securitySchema.AttributeTypes.Count(), "Security attrib types count differ");
            }
        }

        [Test]
        public void Create_Typed_Entity_Under_Root_Then_Copy_To_Another_Parent()
        {
            //Arrange

            var root = new SystemRoot();
            Revision<TypedEntity> content = HiveModelCreationHelper.MockVersionedTypedEntity();
            Revision<TypedEntity> newParent = HiveModelCreationHelper.MockVersionedTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, content, newParent);

            root.RelationProxies.EnlistChild(content.Item, FixedRelationTypes.DefaultRelationType);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(root);
                uow.EntityRepository.Revisions.AddOrUpdate(content);
                uow.EntityRepository.Revisions.AddOrUpdate(newParent);
                uow.Complete();

                // Guard for the fact that the relation proxy must have had its lazyload delegate set as a result of the
                // above calls
                Assert.True(root.RelationProxies.IsConnected);
                Assert.True(content.Item.RelationProxies.IsConnected);
                Assert.True(newParent.Item.RelationProxies.IsConnected);
            }
            PostWriteCallback.Invoke();

            //Act

            TypedEntity copied;
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                //create a new copied entity
                copied = content.Item.CreateDeepCopyToNewParent(newParent.Item, FixedRelationTypes.DefaultRelationType, 0);
                AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, copied);

                uow.EntityRepository.AddOrUpdate(copied);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var queriesCopiedContent = uow.EntityRepository.Get<TypedEntity>(copied.Id);
                Assert.IsNotNull(queriesCopiedContent);
                Assert.IsTrue(queriesCopiedContent.RelationProxies.IsConnected);
                Assert.AreEqual(FixedRelationTypes.DefaultRelationType.RelationName, queriesCopiedContent.RelationProxies.Single().Item.Type.RelationName);
                Assert.AreEqual(newParent.Item.Id, queriesCopiedContent.RelationProxies.Single().Item.SourceId);
            }
            PostWriteCallback.Invoke();
        }

        [Test]
        public void Relations_AddRelation_ChangeRelationUsingSession_ChangesRelation()
        {
            // Arrange
            var parent = HiveModelCreationHelper.MockTypedEntity();
            var child = HiveModelCreationHelper.MockTypedEntity();
            var childSibling = HiveModelCreationHelper.MockTypedEntity();
            var grandChild = HiveModelCreationHelper.MockTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, parent, child, childSibling, grandChild);

            // Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(parent);
                uow.EntityRepository.AddOrUpdate(child);
                uow.EntityRepository.AddOrUpdate(childSibling);
                uow.EntityRepository.AddOrUpdate(grandChild);

                // First of all add the grandChild below child
                uow.EntityRepository.AddRelation(parent, child, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(parent, childSibling, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(child, grandChild, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var foundRelation = uow.EntityRepository.GetParentRelations(grandChild, FixedRelationTypes.DefaultRelationType).FirstOrDefault();
                Assert.NotNull(foundRelation);
                Assert.AreEqual(foundRelation.SourceId, child.Id);
                Assert.AreEqual(foundRelation.DestinationId, grandChild.Id);

                // We're going to move the grandChild instead of being below "child" it will be below "childSibling"
                uow.EntityRepository.ChangeRelation(
                    foundRelation.SourceId,
                    foundRelation.DestinationId,
                    FixedRelationTypes.DefaultRelationType,
                    childSibling.Id,
                    foundRelation.DestinationId);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            // Assert
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var foundParent = uow.EntityRepository.GetParentRelations(grandChild, FixedRelationTypes.DefaultRelationType).FirstOrDefault();

                // The new parent should be childSibling, and the destination id should obviously be grandChild still
                Assert.NotNull(foundParent);
                Assert.AreEqual(foundParent.SourceId, childSibling.Id);
                Assert.AreEqual(foundParent.DestinationId, grandChild.Id);
                Assert.AreNotEqual(foundParent.SourceId, parent.Id);
            }
        }

        [Test]
        public void Relations_AddDescendents_FindsRelationInEitherDirection()
        {
            // Arrange
            var parent = HiveModelCreationHelper.MockTypedEntity();
            var child = HiveModelCreationHelper.MockTypedEntity();
            var grandchild = HiveModelCreationHelper.MockTypedEntity();
            var grandGrandchild = HiveModelCreationHelper.MockTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, parent, child, grandchild, grandGrandchild);

            // Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(parent);
                uow.EntityRepository.AddOrUpdate(child);
                uow.EntityRepository.AddOrUpdate(grandchild);
                uow.EntityRepository.AddOrUpdate(grandGrandchild);

                // Add two relations of different types
                uow.EntityRepository.AddRelation(parent, child, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(parent, child, FixedRelationTypes.RecycledRelationType, 0);
                uow.EntityRepository.AddRelation(child, grandchild, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(grandchild, grandGrandchild, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            // Assert

            // Try loading the parent & child via the repo
            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var reloadParent = uow.EntityRepository.Get<TypedEntity>(parent.Id);
                var reloadChild = uow.EntityRepository.Get<TypedEntity>(child.Id);
                Assert.NotNull(reloadParent);
                Assert.NotNull(reloadChild);

                var reloadChildContentRelations = uow.EntityRepository.GetChildRelations(reloadParent, FixedRelationTypes.DefaultRelationType);
                var reloadParentContentRelations = uow.EntityRepository.GetParentRelations(reloadChild, FixedRelationTypes.DefaultRelationType);

                var reloadAllChildRelations = uow.EntityRepository.GetChildRelations(reloadParent);
                var reloadAllParentRelations = uow.EntityRepository.GetParentRelations(reloadChild);

                Assert.AreEqual(2, reloadAllChildRelations.Count());
                Assert.AreEqual(2, reloadAllParentRelations.Count());

                Assert.AreEqual(1, reloadChildContentRelations.Count());
                Assert.AreEqual(1, reloadParentContentRelations.Count());
                Assert.NotNull(reloadChildContentRelations.FirstOrDefault());

                // Without specifying a relation type, we should end up with 6 because the duplicate relation
                // between parent-child (of different types) should create two "trees" below the parent
                var reloadAllDescendentRelations = uow.EntityRepository.GetDescendentRelations(reloadParent).ToArray();
                Assert.AreEqual(6, reloadAllDescendentRelations.Count());

                // Specifying the relation type to only ContentTree we should end up with 3 
                var reloadDescendentContentRelations = uow.EntityRepository.GetDescendentRelations(reloadParent, FixedRelationTypes.DefaultRelationType);
                Assert.AreEqual(3, reloadDescendentContentRelations.Count());

                var reloadAncestorContentRelations = uow.EntityRepository.GetAncestorRelations(grandGrandchild, FixedRelationTypes.DefaultRelationType);
                Assert.AreEqual(3, reloadAncestorContentRelations.Count());

                // Getting all ancestors *relations* should be four, as the child was added to the parent twice
                var reloadAllAncestorRelations = uow.EntityRepository.GetAncestorRelations(grandGrandchild).ToArray();
                Assert.AreEqual(4, reloadAllAncestorRelations.Count());
            }
            PostWriteCallback.Invoke();

            // Try loading the parent & child via the proxy
            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var reloadParent = uow.EntityRepository.Get<TypedEntity>(parent.Id);
                var reloadChild = uow.EntityRepository.Get<TypedEntity>(child.Id);

                Assert.True(reloadParent.RelationProxies.IsConnected);
                Assert.True(reloadChild.RelationProxies.IsConnected);

                var childRelationProxiesFromParent = reloadParent.RelationProxies.AllChildRelations();
                var parentRelationProxiesFromChild = reloadChild.RelationProxies.AllParentRelations();

                var childContentRelationProxiesFromParent = reloadParent.RelationProxies.GetChildRelations(FixedRelationTypes.DefaultRelationType);
                var parentContentRelationProxiesFromChild = reloadChild.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType);

                Assert.True(childRelationProxiesFromParent.Any());
                Assert.True(parentRelationProxiesFromChild.Any());
                Assert.True(childContentRelationProxiesFromParent.Any());
                Assert.True(parentContentRelationProxiesFromChild.Any());

                Assert.AreEqual(1, childContentRelationProxiesFromParent.Count());
                Assert.NotNull(childContentRelationProxiesFromParent.FirstOrDefault());

                Assert.AreEqual(1, parentContentRelationProxiesFromChild.Count());
                Assert.NotNull(parentContentRelationProxiesFromChild.FirstOrDefault());

                Assert.AreEqual(2, childRelationProxiesFromParent.Count());
                Assert.NotNull(childRelationProxiesFromParent.FirstOrDefault());

                Assert.AreEqual(2, parentRelationProxiesFromChild.Count());
                Assert.NotNull(parentRelationProxiesFromChild.FirstOrDefault());
            }
            PostWriteCallback.Invoke();
        }

        [Test]
        public void Delete_Typed_Entity_With_Revisions()
        {
            //Arrange
            Revision<TypedEntity> content1 = HiveModelCreationHelper.MockVersionedTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, content1);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                    uow.EntityRepository.Revisions.AddOrUpdate(content1);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                    uow.EntityRepository.Delete<TypedEntity>(content1.Item.Id);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                    Revision<TypedEntity> revEntity = uow.EntityRepository.Revisions.Get<TypedEntity>(content1.Item.Id, content1.MetaData.Id);
                Assert.IsNull(revEntity);
                    var e = uow.EntityRepository.Get<TypedEntity>(content1.Item.Id);
                Assert.IsNull(e);
            }
            PostWriteCallback.Invoke();
        }

        [Test]
        public void GetLatestSnapshot_Returns_Null_When_No_Data_Found()
        {
            //Arrange

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                //Act

                var notFound = uow.EntityRepository.Revisions.GetLatestSnapshot<TypedEntity>(HiveId.ConvertIntToGuid(1234));

                //Assert

                Assert.IsNull(notFound);
            }
        }

        [Test]
        public void GetEntitySnapshot_Returns_Null_When_No_Data_Found()
        {
            //Arrange

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                //Act

                var notFound = uow.EntityRepository.Revisions.GetSnapshot<TypedEntity>(
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
        [Test]
        public void Save_Multiple_TypedEntities_With_Multiple_Versions()
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
                AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity);
                return entity;
            };

            var entity1 = createNewEntity(1);
            var entity2 = createNewEntity(2);
            entity1.RelationProxies.EnlistChild(entity2, FixedRelationTypes.DefaultRelationType);

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                // TODO: This implicit versioning (in the NH provider) should be removed and replaced with a direct call to Revisions
                uow.EntityRepository.Revisions.AddOrUpdate(WithFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, new Revision<TypedEntity>(entity1)));
                uow.EntityRepository.Revisions.AddOrUpdate(WithFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, new Revision<TypedEntity>(entity2)));
                uow.EntityRepository.Revisions.AddOrUpdate(WithFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, new Revision<TypedEntity>(entity1)));
                uow.EntityRepository.Revisions.AddOrUpdate(WithFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, new Revision<TypedEntity>(entity2)));
                uow.EntityRepository.Revisions.AddOrUpdate(WithFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, new Revision<TypedEntity>(entity2)));
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var allEntities = uow.EntityRepository.GetAll<TypedEntity>().ToArray();
                Assert.That(allEntities.Length, Is.EqualTo(2));

                var allRevisions = uow.EntityRepository.Revisions.GetAll<TypedEntity>().ToArray();
                Assert.That(allRevisions.Length, Is.EqualTo(5));

                var found1 = uow.EntityRepository.Revisions.GetLatestSnapshot<TypedEntity>(entity1.Id);
                var found2 = uow.EntityRepository.Revisions.GetLatestSnapshot<TypedEntity>(entity2.Id);

                Assert.IsNotNull(found1);
                Assert.IsNotNull(found2);

                var findAllRevisions1 = uow.EntityRepository.Revisions.GetAll<TypedEntity>(entity1.Id);
                var findAllRevisions2 = uow.EntityRepository.Revisions.GetAll<TypedEntity>(entity2.Id);

                Assert.That(findAllRevisions1.Count(), Is.EqualTo(2));
                Assert.That(findAllRevisions2.Count(), Is.EqualTo(3));

                var relation = uow.EntityRepository.GetLazyChildRelations(found1.Revision.Item.Id, FixedRelationTypes.DefaultRelationType).ToArray();
                Assert.NotNull(relation);
                Assert.AreEqual(1, relation.Count());
                var firstRelation = relation.FirstOrDefault();
                Assert.NotNull(firstRelation.Source);
                Assert.AreEqual(found1.Revision.Item.Id, firstRelation.SourceId);
                Assert.NotNull(firstRelation.Destination);
                Assert.AreEqual(found2.Revision.Item.Id, firstRelation.DestinationId);

                var proxyRelation = found1.Revision.Item.RelationProxies.GetChildRelations(FixedRelationTypes.DefaultRelationType);
                Assert.NotNull(proxyRelation);
                Assert.AreEqual(1, proxyRelation.Count());
                var firstRelationProxy = proxyRelation.FirstOrDefault();
                Assert.IsNull(firstRelationProxy.Item.Source); // RelationProxy doesn't yet return the actual item or else that would be back to the situation of recursive lazy-loading
                Assert.AreEqual(found1.Revision.Item.Id, firstRelationProxy.Item.SourceId);
                Assert.IsNull(firstRelationProxy.Item.Destination);
                Assert.AreEqual(found2.Revision.Item.Id, firstRelationProxy.Item.DestinationId);

                Assert.AreEqual(2, found1.EntityRevisionStatusList.Count());
                //NOTE: This is 4 because when you AddOrUpdate an entity with a relation, it automatically does an AddOrUpdate on the entity inside the relation
                //RETEST: (APN) This is no longer the case with RelationProxies, changed to 3
                Assert.AreEqual(3, found2.EntityRevisionStatusList.Count());
                Assert.AreEqual(3, found1.Revision.Item.Attributes.Count());
                Assert.AreEqual(3, found2.Revision.Item.Attributes.Count());
                Assert.AreEqual(2, found1.Revision.Item.AttributeGroups.Count());
                Assert.AreEqual(2, found2.Revision.Item.AttributeGroups.Count());
                Assert.AreEqual(6, found1.Revision.Item.EntitySchema.AttributeDefinitions.Count());
                Assert.AreEqual(6, found2.Revision.Item.EntitySchema.AttributeDefinitions.Count());
                Assert.AreEqual(2, found1.Revision.Item.EntitySchema.AttributeGroups.Count());
                Assert.AreEqual(2, found2.Revision.Item.EntitySchema.AttributeGroups.Count());
            }
        }

        [Test]
        public void Ensure_New_Revision_Has_New_Ids_For_TypedAttributes()
        {
            //Arrange

            var revision = HiveModelCreationHelper.MockVersionedTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, revision);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Revisions.AddOrUpdate(revision);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Act

            Revision<TypedEntity> newRevision;
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var lookedUp = uow.EntityRepository.Revisions.GetLatestRevision<TypedEntity>(revision.Item.Id);
                newRevision = lookedUp.CopyToNewRevision();
                uow.EntityRepository.Revisions.AddOrUpdate(newRevision);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            foreach(var a in revision.Item.Attributes)
            {
                Assert.AreNotEqual(a.Id, newRevision.Item.Attributes.Single(x => x.AttributeDefinition.Alias == a.AttributeDefinition.Alias).Id);
            }

        }

        [Test]
        public void ReSaveRevision()
        {
            //Arrange

            Revision<TypedEntity> revision = HiveModelCreationHelper.MockVersionedTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, revision);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Revisions.AddOrUpdate(revision);
                uow.Complete();
                Assert.That(revision.MetaData.Id, Is.Not.EqualTo(HiveId.Empty));
                Assert.That(revision.Item.Id, Is.Not.EqualTo(HiveId.Empty));
            }
            PostWriteCallback.Invoke();

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var allExisting = uow.EntityRepository.Revisions.GetAll<TypedEntity>();
                Assert.That(allExisting.Count(), Is.EqualTo(1));
            }

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var copy = revision.CopyToNewRevision();
                AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, copy);

                // Now add the same one twice which should not affect the final revision count (id is set)
                uow.EntityRepository.Revisions.AddOrUpdate(copy);
                copy.MetaData.StatusType = FixedStatusTypes.Published; // Change the status type
                uow.EntityRepository.Revisions.AddOrUpdate(copy);
                uow.Complete();

                Assert.That(copy.MetaData.Id, Is.Not.EqualTo(HiveId.Empty));
                Assert.That(copy.Item.Id, Is.Not.EqualTo(HiveId.Empty));
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var allRevisions = uow.EntityRepository.Revisions.GetAll<TypedEntity>(revision.Item.Id).ToArray();
                Assert.AreEqual(2, allRevisions.Length); // We saved the revision three times but only with two distinct revision ids within one UoW

                var entitySnapshot = uow.EntityRepository.Revisions.GetLatestSnapshot<TypedEntity>(revision.Item.Id);
                var allStatusses = entitySnapshot.EntityRevisionStatusList.ToArray();
                Assert.AreEqual(3, allStatusses.Count()); // We saved it three times, with two distinct revisions, so should have three statuses
            }

        }

        [Test]
        public void ReSaveRevision_DoesNotCreateNewRevision()
        {
            //Arrange

            Revision<TypedEntity> revision = HiveModelCreationHelper.MockVersionedTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, revision);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Revisions.AddOrUpdate(revision);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Revisions.AddOrUpdate(revision);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var found = uow.EntityRepository.Revisions.GetLatestSnapshot<TypedEntity>(revision.Item.Id);
                var allRevisions = uow.EntityRepository.Revisions.GetAll<TypedEntity>(revision.Item.Id).ToArray();
                Assert.AreEqual(1, found.EntityRevisionStatusList.Count());
                Assert.AreEqual(1, allRevisions.Length);
            }

        }

        [Test]
        public void GetLatestRevision_WhenGivenANegatingStatusType_ReturnsNull()
        {
            var entity = HiveModelCreationHelper.MockTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity);

            var revision1 = new Revision<TypedEntity>(entity) { MetaData = { StatusType = FixedStatusTypes.Draft } };
            var revision2 = new Revision<TypedEntity>(entity) { MetaData = { StatusType = FixedStatusTypes.Published } };

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Revisions.AddOrUpdate(revision1);
                uow.EntityRepository.Revisions.AddOrUpdate(revision2);

                uow.Complete();
            }

            // Check we can load the revision where the status type is published
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var checkRevision = uow.EntityRepository.Revisions.GetLatestRevision<TypedEntity>(entity.Id, FixedStatusTypes.Published);
                Assert.NotNull(checkRevision);
                Assert.That(checkRevision.Item.Id, Is.EqualTo(entity.Id));
                Assert.That(checkRevision.MetaData.Id, Is.EqualTo(revision2.MetaData.Id));
                Assert.That(checkRevision.MetaData.StatusType, Is.EqualTo(revision2.MetaData.StatusType));
            }

            var revision3 = new Revision<TypedEntity>(entity) { MetaData = { StatusType = FixedStatusTypes.Unpublished } };
            // Add another revision with an unpublished status
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Revisions.AddOrUpdate(revision3);
                uow.Complete();
            }

            // Check we can load the revision where the status type is unpublished, and that we can't load the latest published if unpublished negates it
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var checkUnpublishedRevision = uow.EntityRepository.Revisions.GetLatestRevision<TypedEntity>(entity.Id, FixedStatusTypes.Unpublished);
                Assert.That(checkUnpublishedRevision.Item.Id, Is.EqualTo(entity.Id));

                var checkPublishedRevision = uow.EntityRepository.Revisions.GetLatestRevision<TypedEntity>(entity.Id, FixedStatusTypes.Published);
                Assert.IsNull(checkPublishedRevision);
            }
        }

        [Test]
        public void GetAllTypedEntityRevisions()
        {
            //Arrange

            var entity1 = HiveModelCreationHelper.MockTypedEntity();
            var entity2 = HiveModelCreationHelper.MockTypedEntity();
            var entity3 = HiveModelCreationHelper.MockTypedEntity();
            var entity4 = HiveModelCreationHelper.MockTypedEntity();
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(entity1);
                uow.EntityRepository.AddOrUpdate(entity2);
                uow.EntityRepository.AddOrUpdate(entity3);
                uow.EntityRepository.AddOrUpdate(entity4);

                //create revisions for some
                uow.EntityRepository.AddOrUpdate(entity1);
                uow.EntityRepository.AddOrUpdate(entity1);
                uow.EntityRepository.AddOrUpdate(entity2);

                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Act/Assert

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var anEntity = uow.EntityRepository.Get<TypedEntity>(entity1.Id);
                Assert.NotNull(anEntity);
                Assert.That(entity1.Attributes.Count, Is.GreaterThan(1));
                Assert.That(anEntity.Attributes.Count, Is.EqualTo(entity1.Attributes.Count));

                var found = uow.EntityRepository.Revisions.GetAll<TypedEntity>();
                var first = found.FirstOrDefault();
                Assert.NotNull(first);
                Assert.That(first.Item.Attributes.Count, Is.EqualTo(entity1.Attributes.Count));
                Assert.AreEqual(7, found.Count());
            }

        }

        [Test]
        public void ReSaveEntity()
        {
            //Arrange

            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity);
            HiveId idBetweenSaves;
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(entity);
                uow.Complete();

                idBetweenSaves = entity.Id;
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(entity);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert
            Assert.That(entity.Id, Is.EqualTo(idBetweenSaves));
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var found = uow.EntityRepository.Revisions.GetLatestSnapshot<TypedEntity>(entity.Id);
                Assert.NotNull(found);
                Assert.AreEqual(2, found.EntityRevisionStatusList.Count());
            }
        }

        [Test]
        public void Deleting_Schema_Removes_All_Entities()
        {
            //Arrange

            Revision<TypedEntity> content1 = HiveModelCreationHelper.MockVersionedTypedEntity();
            Revision<TypedEntity> content2 = HiveModelCreationHelper.MockVersionedTypedEntity();
            //assign ids to create a relation
            content1.Item.Id = HiveId.ConvertIntToGuid(1);
            content2.Item.Id = HiveId.ConvertIntToGuid(2);
            content1.Item.RelationProxies.EnlistChild(content2.Item, FixedRelationTypes.DefaultRelationType);
            //assign ids to schema and create schema relation
            content1.Item.EntitySchema.Id = HiveId.ConvertIntToGuid(10);
            content2.Item.EntitySchema.Id = HiveId.ConvertIntToGuid(20);
            content1.Item.EntitySchema.RelationProxies.EnlistChild(content2.Item.EntitySchema, FixedRelationTypes.DefaultRelationType);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Revisions.AddOrUpdate(content1);
                uow.EntityRepository.Revisions.AddOrUpdate(content2);
                //make some versions
                uow.EntityRepository.Revisions.AddOrUpdate(content1);
                uow.EntityRepository.Revisions.AddOrUpdate(content1);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var contentReloaded = uow.EntityRepository.Get<TypedEntity>(content1.Item.Id);
                Assert.IsNotNull(contentReloaded);
            }

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Schemas.Delete<EntitySchema>(content1.Item.EntitySchema.Id);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var attributeType = uow.EntityRepository.Schemas.Get<EntitySchema>(content1.Item.EntitySchema.Id);
                Assert.IsNull(attributeType);
                Assert.IsFalse(content1.Item.Id.IsNullValueOrEmpty());
                var contentReloaded = uow.EntityRepository.Get<TypedEntity>(content1.Item.Id);
                Assert.IsNull(contentReloaded);
                Assert.IsFalse(uow.EntityRepository.Exists<TypedEntity>(content1.Item.Id));
                Assert.IsNull(uow.EntityRepository.Revisions.Get<TypedEntity>(content1.Item.Id, content1.MetaData.Id));
            }
            PostWriteCallback.Invoke();
        }

        [Test]
        [Category("Regression")]
        [Description("This tests for the fact that the same instance of an AttributeType assigned to two or more AttributeDefinitions in the same EntitySchema should result in a single AttributeType persisted")]
        public void WhenCreatingSchema_WithMultipleAttributeDefinitions_ButSameAttributeType_OneAttributeType_IsPersisted()
        {
            // Arrange
            var groupDefinition = HiveModelCreationHelper.CreateAttributeGroup("tab-1", "Tab 1", 0);
            var typeDefinition = HiveModelCreationHelper.CreateAttributeType("type-alias1", "test-type-name", "test-type-description");
            var attribDef1 = HiveModelCreationHelper.CreateAttributeDefinition("def-alias-1-with-type-1", "name-1", "test-description", typeDefinition, groupDefinition);
            var attribDef2 = HiveModelCreationHelper.CreateAttributeDefinition("def-alias-2-with-type-1", "name-2", "test-description", typeDefinition, groupDefinition);
            var schema = HiveModelCreationHelper.CreateEntitySchema("schema", "My Schema", new[] { attribDef1, attribDef2 });
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, groupDefinition, typeDefinition, attribDef1, attribDef2, schema);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Schemas.AddOrUpdate(schema);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            // Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var types = uow.EntityRepository.Schemas.GetAll<AttributeType>();
                Assert.That(types.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void Deleting_Attribute_Type_Removes_All_Associations()
        {
            //Arrange

            Revision<TypedEntity> content = HiveModelCreationHelper.MockVersionedTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, content);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Revisions.AddOrUpdate(content);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            var referncedAttribute = content.Item.Attributes.First();
            var referencedAttributeDef = referncedAttribute.AttributeDefinition;
            var attTypeToDelete = referencedAttributeDef.AttributeType;

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var attributeType = uow.EntityRepository.Schemas.Get<AttributeType>(attTypeToDelete.Id);
                Assert.NotNull(attributeType);
                uow.EntityRepository.Schemas.Delete<AttributeType>(attributeType.Id);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var attributeType = uow.EntityRepository.Schemas.Get<AttributeType>(attTypeToDelete.Id);
                Assert.IsNull(attributeType);
                var attributeDefinition = uow.EntityRepository.Schemas.Get<AttributeDefinition>(referencedAttributeDef.Id);
                Assert.IsNull(attributeDefinition);
                //var attribute = uow.Repositories.Schemas.Get<TypedAttribute>(referncedAttribute.Id);
                //Assert.IsNull(attribute);

                var contentReloaded = uow.EntityRepository.Get<TypedEntity>(content.Item.Id);
                //NOTE: This removes '2' attributes from the TypedEntity because the AttributeType being deleted
                // is assigned to '2' AttributeDefinitions on the schema, this was previously set to '1' which made this test flawed. SD. 25/10/2011
                var allOriginalAttribAliases = content.Item.Attributes.Select(x => x.AttributeDefinition.Alias).ToArray();
                var allNewAttribAliases = contentReloaded.Attributes.Select(x => x.AttributeDefinition.Alias).ToArray();
                Assert.AreEqual(content.Item.Attributes.Count - 2, contentReloaded.Attributes.Count, "{0} old aliases: {1}\n{2} New aliases: {3}", allOriginalAttribAliases.Length, string.Join(", ", allOriginalAttribAliases), allNewAttribAliases.Length, string.Join(", ", allNewAttribAliases));
            }
            PostWriteCallback.Invoke();
        }

        [Test]
        public void Create_Typed_Entity_Under_Root_Then_Move_To_Another_Parent()
        {
            //Arrange

            var root = new SystemRoot();
            Revision<TypedEntity> content = HiveModelCreationHelper.MockVersionedTypedEntity();
            Revision<TypedEntity> newParent = HiveModelCreationHelper.MockVersionedTypedEntity();
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, content, newParent);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(root);
                uow.EntityRepository.Revisions.AddOrUpdate(content);
                uow.EntityRepository.Revisions.AddOrUpdate(newParent);
                uow.EntityRepository.AddRelation(root, content.Item, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();

                // Guard for the fact that the relation proxy must have had its lazyload delegate set as a result of the
                // above calls
                Assert.True(root.RelationProxies.IsConnected);
                Assert.True(content.Item.RelationProxies.IsConnected);
                Assert.True(newParent.Item.RelationProxies.IsConnected);
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var reloadedContent = uow.EntityRepository.Revisions.Get<TypedEntity>(content.Item.Id, content.MetaData.Id);
                Assert.NotNull(reloadedContent);

                var rootRelations = uow.EntityRepository
                    .GetChildRelations(root, FixedRelationTypes.DefaultRelationType);

                var rootRelation = rootRelations
                    .Where(x => x.DestinationId == reloadedContent.Item.Id)
                    .FirstOrDefault();
                Assert.NotNull(rootRelation);

                uow.EntityRepository.RemoveRelation(rootRelation);
                uow.EntityRepository.AddRelation(newParent.Item, content.Item, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var contentReloaded = uow.EntityRepository.Get<TypedEntity>(content.Item.Id);
                Assert.IsNotNull(contentReloaded);
                var relations = contentReloaded.RelationProxies.ToList();
                Assert.That(relations.Count, Is.EqualTo(1));
                var relation = relations.Single().Item;
                Assert.AreEqual(FixedRelationTypes.DefaultRelationType.RelationName, relation.Type.RelationName);
                Assert.AreEqual(newParent.Item.Id, relation.SourceId);

                var rootReloaded = uow.EntityRepository.Get<TypedEntity>(root.Id);
                Assert.NotNull(rootReloaded);
                var rootRelations = uow.EntityRepository.GetDescendentRelations(root, FixedRelationTypes.DefaultRelationType);
                Assert.IsFalse(rootRelations.Any());
            }
            PostWriteCallback.Invoke();
        }

        [Test]
        public void Create_SchemaAndRelations_InSameUnit_WithoutAssigningIds()
        {
            //Arrange
            var schemaParent = HiveModelCreationHelper.MockEntitySchema("schema1", "schema1");
            var schemaChild = HiveModelCreationHelper.MockEntitySchema("schema2", "schema2");

            // Despite the name of the test, if we're testing a passthrough provider, we have to
            // fake the ids because passthrough providers assume they do not have to generate them
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, schemaParent, schemaChild);

            //Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Schemas.AddOrUpdate(schemaParent);
                uow.EntityRepository.Schemas.AddOrUpdate(schemaChild);
                //make schema 1 the parent of schema 2
                uow.EntityRepository.Schemas.AddRelation(schemaParent, schemaChild, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();
            }

            PostWriteCallback.Invoke();

            //Assert
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var parentReloaded = uow.EntityRepository.Schemas.Get<EntitySchema>(schemaParent.Id);
                var childReloaded = uow.EntityRepository.Schemas.Get<EntitySchema>(schemaChild.Id);

                Assert.IsNotNull(parentReloaded);
                Assert.IsNotNull(childReloaded);

                // Ensure that the above methods connect the lazy-load delegate to the RelationProxyCollection
                Assert.IsTrue(parentReloaded.RelationProxies.IsConnected);
                Assert.IsTrue(childReloaded.RelationProxies.IsConnected);

                var relation = parentReloaded.RelationProxies.Single();
                Assert.AreEqual(FixedRelationTypes.DefaultRelationType.RelationName, relation.Item.Type.RelationName);
                Assert.AreEqual(parentReloaded.Id, relation.Item.SourceId);
                Assert.AreEqual(childReloaded.Id, relation.Item.DestinationId);
            }
        }

        [Test]
        public void Create_TypedEntityAndRelations_InSameUnit_WithoutAssigningIds()
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

                // Despite the name of the test, if we're testing a passthrough provider, we have to
                // fake the ids because passthrough providers assume they do not have to generate them
                AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity);
                return entity;
            };

            var entityParent = createNewEntity(1);
            var entityChild = createNewEntity(2);

            //Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(entityParent);
                uow.EntityRepository.AddOrUpdate(entityChild);
                //make schema 1 the parent of schema 2
                uow.EntityRepository.AddRelation(entityParent, entityChild, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();
            }

            PostWriteCallback.Invoke();

            //Assert
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var parentReloaded = uow.EntityRepository.Get<TypedEntity>(entityParent.Id);
                var childReloaded = uow.EntityRepository.Get<TypedEntity>(entityChild.Id);

                Assert.IsNotNull(parentReloaded);
                Assert.IsNotNull(childReloaded);

                var relation = parentReloaded.RelationProxies.Single();
                Assert.AreEqual(FixedRelationTypes.DefaultRelationType.RelationName, relation.Item.Type.RelationName);
                Assert.AreEqual(parentReloaded.Id, relation.Item.SourceId);
                Assert.AreEqual(childReloaded.Id, relation.Item.DestinationId);
            }
        }

        [Test]
        public void Delete_EntitySchema_With_Relations()
        {
            //Arrange           
            var schema1 = HiveModelCreationHelper.MockEntitySchema("schema1", "schema1");
            var schema2 = HiveModelCreationHelper.MockEntitySchema("schema2", "schema2");
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, schema1);
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, schema2);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Schemas.AddOrUpdate(schema1);
                uow.EntityRepository.Schemas.AddOrUpdate(schema2);
                uow.EntityRepository.Schemas.AddRelation(schema1, schema2, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var found2 = uow.EntityRepository.Schemas.Get<EntitySchema>(schema2.Id);
                Assert.AreEqual(1, found2.RelationProxies.Count());
                uow.EntityRepository.Schemas.Delete<EntitySchema>(schema2.Id);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var found1 = uow.EntityRepository.Schemas.Get<EntitySchema>(schema1.Id);
                var found2 = uow.EntityRepository.Schemas.Get<EntitySchema>(schema2.Id);

                Assert.IsNull(found2);
                Assert.IsNotNull(found1);
                Assert.AreEqual(0, found1.RelationProxies.Count());
            }
        }

        [Test]
        public void Delete_Typed_Entity_With_Relations_And_Revisions()
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
                AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity);
                return entity;
            };

            var entity1 = createNewEntity(1);
            var entity2 = createNewEntity(2);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(entity1);
                uow.EntityRepository.AddOrUpdate(entity2);
                uow.EntityRepository.AddRelation(entity1, entity2, FixedRelationTypes.DefaultRelationType, 0);
                //create a few versions
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity2));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity2));
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Act

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var found1 = uow.EntityRepository.Get<TypedEntity>(entity1.Id);
                Assert.AreEqual(1, found1.RelationProxies.Count());
                uow.EntityRepository.Delete<TypedEntity>(entity2.Id);
                uow.Complete();
            }
            PostWriteCallback.Invoke();

            //Assert

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var found1 = uow.EntityRepository.Revisions.GetLatestSnapshot<TypedEntity>(entity1.Id);
                var found2 = uow.EntityRepository.Revisions.GetLatestSnapshot<TypedEntity>(entity2.Id);

                Assert.IsNull(found2);
                Assert.IsNotNull(found1);
                Assert.AreEqual(3, found1.EntityRevisionStatusList.Count());
                Assert.AreEqual(0, found1.Revision.Item.RelationProxies.Count());
            }
        }

        [Test]
        public void Delete_AttributeDefinition_FromSchema_WhenRevisionsAlreadyExist()
        {
            // Arrange
            AttributeGroup group1 = HiveModelCreationHelper.CreateAttributeGroup("group1", "group1", 0);
            AttributeGroup group2 = HiveModelCreationHelper.CreateAttributeGroup("group2", "group2", 0);
            AttributeType type1 = HiveModelCreationHelper.CreateAttributeType("type1", "type1", "type1");
            AttributeType type2 = HiveModelCreationHelper.CreateAttributeType("type2", "type2", "type2");
            AttributeDefinition def1 = HiveModelCreationHelper.CreateAttributeDefinition("def1", "def1", "def1", type1, group1);
            AttributeDefinition def2 = HiveModelCreationHelper.CreateAttributeDefinition("def2", "def2", "def2", type2, group1);
            AttributeDefinition def3 = HiveModelCreationHelper.CreateAttributeDefinition("def3", "def3", "def3", type1, group2);
            AttributeDefinition def2tobedeleted = HiveModelCreationHelper.CreateAttributeDefinition("def2tobedeleted", "def2tobedeleted", "def2tobedeleted", type1, group1);
            AttributeDefinition def3tobedeletedDirectly = HiveModelCreationHelper.CreateAttributeDefinition("def3tobedeletedDirectly", "def3tobedeletedDirectly", "def3tobedeletedDirectly", type1, group1);
            EntitySchema schema1 = HiveModelCreationHelper.CreateEntitySchema("schema1", "schema1", def1, def2, def3, def2tobedeleted, def3tobedeletedDirectly);

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, schema1);

            HiveId schemaId;
            HiveId contentId;

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Schemas.AddOrUpdate(schema1);
                uow.Complete();
                schemaId = schema1.Id;
            }

            PostWriteCallback.Invoke();

            // Reload the schema, make some revisions
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var reloadSchema = uow.EntityRepository.Schemas.Get<EntitySchema>(schemaId);

                TypedEntity content1 = HiveModelCreationHelper.CreateTypedEntity(reloadSchema, new[]
                {
                    new TypedAttribute(def1, "value1"),
                    new TypedAttribute(def2, "value2"),
                    new TypedAttribute(def3, "value3"),
                    new TypedAttribute(def2tobedeleted, "value2tobedeleted"),
                    new TypedAttribute(def3tobedeletedDirectly, "value2tobedeletedDirectly")
                });

                AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, content1);

                // Create a few revisions
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));

                uow.Complete();

                contentId = content1.Id;
            }

            PostWriteCallback.Invoke();

            // Reload the schema, delete an attribute definition, save it
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var reloadSchema = uow.EntityRepository.Schemas.Get<EntitySchema>(schemaId);

                Assert.AreEqual(5, reloadSchema.AttributeDefinitions.Count);
                Assert.AreEqual(2, reloadSchema.AttributeGroups.Count);

                reloadSchema.AttributeDefinitions.RemoveAll(x => x.Alias == def2tobedeleted.Alias);

                Assert.AreEqual(4, reloadSchema.AttributeDefinitions.Count);
                Assert.AreEqual(2, reloadSchema.AttributeGroups.Count);

                uow.EntityRepository.Schemas.AddOrUpdate(reloadSchema);

                uow.Complete();
            }

            PostWriteCallback.Invoke();

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var reloadSchema = uow.EntityRepository.Schemas.Get<EntitySchema>(schemaId);
                Assert.AreEqual(4, reloadSchema.AttributeDefinitions.Count);
                Assert.AreEqual(2, reloadSchema.AttributeGroups.Count);
            }

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var contentReloaded1 = uow.EntityRepository.Get<TypedEntity>(contentId);
                Assert.That(contentReloaded1.Attributes.Select(x => x.AttributeDefinition).Distinct().Count(), Is.EqualTo(4), "Content still has attributes: " + string.Join(", ", contentReloaded1.Attributes.Select(x => x.AttributeDefinition.Alias)));
            }

            PostWriteCallback.Invoke();
        }

        [Test]
        public void Deleting_Attribute_Definition_From_Group()
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

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, schema1);
            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, content1);

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Schemas.AddOrUpdate(schema1);

                // Create a few revisions
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));
                uow.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(content1));

                uow.Complete();
            }

            PostWriteCallback.Invoke();

            //using (var uow = this.ProviderSetup.UnitFactory.Create())
            //{
            //    var contentReloaded1 = uow.Repositories.GetEntity<TypedEntity>(content1.Id);

            //    contentReloaded1.Attributes.RemoveAll(x => x.AttributeDefinition.Alias == def2tobedeleted.Alias);

            //    uow.Repositories.AddOrUpdate(contentReloaded1);
            //    uow.Complete();

            //    contentReloaded1 = uow.Repositories.GetEntity<TypedEntity>(content1.Id);

            //    Assert.AreEqual(2, contentReloaded1.Attributes.Count);
            //}

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var schemaReloaded1 = uow.EntityRepository.Schemas.Get<EntitySchema>(schema1.Id);
                //var contentReloaded1 = uow.Repositories.GetEntity<TypedEntity>(content1.Id);

                Assert.AreEqual(3, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(1, schemaReloaded1.AttributeGroups.Count);

                schemaReloaded1.AttributeDefinitions.RemoveAll(x => x.Alias == def2tobedeleted.Alias);
                //contentReloaded1.Attributes.RemoveAll(x => x.AttributeDefinition.Alias == def2tobedeleted.Alias);

                Assert.AreEqual(2, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(1, schemaReloaded1.AttributeGroups.Count);

                //uow.Repositories.AddOrUpdate(contentReloaded1);
                uow.EntityRepository.Schemas.AddOrUpdate(schemaReloaded1);
                uow.Complete();

                schemaReloaded1 = uow.EntityRepository.Schemas.Get<EntitySchema>(schema1.Id);
                //contentReloaded1 = uow.Repositories.GetEntity<TypedEntity>(content1.Id);

                Assert.AreEqual(2, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(1, schemaReloaded1.AttributeGroups.Count);
                //Assert.AreEqual(1, contentReloaded1.Attributes.Count);
            }

            PostWriteCallback.Invoke();

            HiveId attribDefId = HiveId.Empty;

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                var schemaReloaded1 = uow.EntityRepository.Schemas.Get<EntitySchema>(schema1.Id);
                attribDefId = schemaReloaded1.AttributeDefinitions.FirstOrDefault(x => x.Alias == "def3tobedeletedDirectly").Id;
                var contentReloaded1 = uow.EntityRepository.Get<TypedEntity>(content1.Id);
                Assert.That(schemaReloaded1.AttributeDefinitions.Count, Is.EqualTo(2));
                Assert.That(contentReloaded1.Attributes.Count, Is.Not.EqualTo(content1.Attributes.Count)); // content1 should remain untouched as it was not resaved
                Assert.That(contentReloaded1.Attributes.Count, Is.EqualTo(2));
            }

            PostWriteCallback.Invoke();

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Schemas.Delete<AttributeDefinition>(attribDefId);
                uow.Complete();

                var schemaReloaded1 = uow.EntityRepository.Schemas.Get<EntitySchema>(schema1.Id);
                var contentReloaded1 = uow.EntityRepository.Get<TypedEntity>(content1.Id);

                Assert.AreEqual(1, schemaReloaded1.AttributeDefinitions.Count);
                Assert.AreEqual(1, contentReloaded1.Attributes.Count);
            }
        }

        /// <summary>
        /// This test ensures that the entity Status changed date, the revision created date, the entity modified date and entity created dates
        /// are all set properly whilst committing many revisions in one unit of work.
        /// </summary>
        [Test]
        public virtual void Ensure_Correct_Timestamps_On_Revisions_Single_Unit_Of_Work()
        {
            var e = new[]
                {
                    HiveModelCreationHelper.MockTypedEntity(), 
                    HiveModelCreationHelper.MockTypedEntity()
                };

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                //create revisions with same status twice
                e.ForEach(x => writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(x)));
                e.ForEach(x => writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(x)));
                //new revision with new draft status
                e.ForEach(x => writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(x) { MetaData = new RevisionData(FixedStatusTypes.Draft) }));
                //dont' create a saved rev for the last one
                e.Where(x => x != e.Last()).ForEach(x => writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(x) { MetaData = new RevisionData(FixedStatusTypes.Saved) }));          

                writer.Complete();
            }

            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var result = reader.EntityRepository.Revisions.GetAll<TypedEntity>();
                Assert.AreEqual(7, result.Count());

                //ensure entity status changed dates are correct
                Assert.AreEqual(3, result.Where(x => x.Item.Id == e.ElementAt(0).Id).Select(x => x.Item.UtcStatusChanged).Distinct().Count());
                Assert.AreEqual(2, result.Where(x => x.Item.Id == e.ElementAt(1).Id).Select(x => x.Item.UtcStatusChanged).Distinct().Count());

                //ensure rev created dates are correct
                Assert.AreEqual(4, result.Where(x => x.Item.Id == e.ElementAt(0).Id).Select(x => x.MetaData.UtcCreated).Distinct().Count());
                Assert.AreEqual(3, result.Where(x => x.Item.Id == e.ElementAt(1).Id).Select(x => x.MetaData.UtcCreated).Distinct().Count());
                
                //ensure entity modifieid dates are correct
                Assert.AreEqual(4, result.Where(x => x.Item.Id == e.ElementAt(0).Id).Select(x => x.Item.UtcModified).Distinct().Count());
                Assert.AreEqual(3, result.Where(x => x.Item.Id == e.ElementAt(1).Id).Select(x => x.Item.UtcModified).Distinct().Count());

                //ensure entity created dates are correct
                Assert.AreEqual(1, result.Where(x => x.Item.Id == e.ElementAt(0).Id).Select(x => x.Item.UtcCreated).Distinct().Count());
                Assert.AreEqual(1, result.Where(x => x.Item.Id == e.ElementAt(1).Id).Select(x => x.Item.UtcCreated).Distinct().Count());
            }

        }

        /// <summary>
        /// This test ensures that the entity Status changed date, the revision created date, the entity modified date and entity created dates
        /// are all set properly whilst committing different revisions in different units of work.
        /// </summary>
        [Test]
        public virtual void Ensure_Correct_Timestamps_On_Revisions_Multiple_Units_Of_Work()
        {
            var e = new[]
                {
                    HiveModelCreationHelper.MockTypedEntity(), 
                    HiveModelCreationHelper.MockTypedEntity()
                };

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                e.ForEach(x => writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(x)));
                writer.Complete();
            }
            PostWriteCallback.Invoke();
            Thread.Sleep(TimeSpan.FromSeconds(0.7));

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                //create revisions with same status twice
                e.ForEach(x => writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(x)));
                writer.Complete();
            }
            PostWriteCallback.Invoke();
            Thread.Sleep(TimeSpan.FromSeconds(0.7));

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                //new revision with new draft status
                e.ForEach(x => writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(x) { MetaData = new RevisionData(FixedStatusTypes.Draft) }));
                writer.Complete();
            }
            PostWriteCallback.Invoke();
            Thread.Sleep(TimeSpan.FromSeconds(0.7));

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                //dont' create a saved rev for the last one
                e.Where(x => x != e.Last()).ForEach(x => writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(x) { MetaData = new RevisionData(FixedStatusTypes.Saved) }));
                writer.Complete();
            }
            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var result = reader.EntityRepository.Revisions.GetAll<TypedEntity>().ToArray();
                Assert.AreEqual(7, result.Count());
                //ensure entity status changed dates are correct
                Assert.AreEqual(3, result.Where(x => x.Item.Id == e.ElementAt(0).Id).Select(x => x.Item.UtcStatusChanged).Distinct().Count());
                Assert.AreEqual(2, result.Where(x => x.Item.Id == e.ElementAt(1).Id).Select(x => x.Item.UtcStatusChanged).Distinct().Count());

                //ensure rev created dates are correct
                Assert.AreEqual(4, result.Where(x => x.Item.Id == e.ElementAt(0).Id).Select(x => x.MetaData.UtcCreated).Distinct().Count());
                Assert.AreEqual(3, result.Where(x => x.Item.Id == e.ElementAt(1).Id).Select(x => x.MetaData.UtcCreated).Distinct().Count());

                //ensure entity modifieid dates are correct
                Assert.AreEqual(4, result.Where(x => x.Item.Id == e.ElementAt(0).Id).Select(x => x.Item.UtcModified).Distinct().Count());
                Assert.AreEqual(3, result.Where(x => x.Item.Id == e.ElementAt(1).Id).Select(x => x.Item.UtcModified).Distinct().Count());

                //ensure entity created dates are correct
                Assert.AreEqual(1, result.Where(x => x.Item.Id == e.ElementAt(0).Id).Select(x => x.Item.UtcCreated).Distinct().Count());
                Assert.AreEqual(1, result.Where(x => x.Item.Id == e.ElementAt(1).Id).Select(x => x.Item.UtcCreated).Distinct().Count());
            }

        }

        [Test]
        public virtual void GetAll_TypedEntity_ReturnsResults()
        {
            TypedEntity entity1 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity2 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity3 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity4 = HiveModelCreationHelper.MockTypedEntity();
            TypedEntity entity5 = HiveModelCreationHelper.MockTypedEntity();

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity1, entity2, entity3, entity4, entity5);

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.AddOrUpdate(entity1);
                writer.EntityRepository.AddOrUpdate(entity2);
                writer.EntityRepository.AddOrUpdate(entity3);
                writer.EntityRepository.AddOrUpdate(entity4);
                writer.EntityRepository.AddOrUpdate(entity5);

                // Add some revisions of the same entities
                writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity1));
                writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity2));
                writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity2));
                writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity3));
                writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity4));
                writer.EntityRepository.Revisions.AddOrUpdate(new Revision<TypedEntity>(entity5));

                writer.Complete();
            }

            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                IEnumerable<TypedEntity> result = reader.EntityRepository.GetAll<TypedEntity>();
                Assert.AreEqual(5, result.Count());
                Assert.That(result, Is.Unique);
                var hiveIds = result.Select(x => x.Id).ToArray();
                Assert.That(hiveIds, Is.Unique);
                Assert.That(hiveIds, Contains.Item(entity1.Id));
                Assert.That(hiveIds, Contains.Item(entity2.Id));
                Assert.That(hiveIds, Contains.Item(entity3.Id));
                Assert.That(hiveIds, Contains.Item(entity4.Id));
                Assert.That(hiveIds, Contains.Item(entity5.Id));
            }
        }

        [Test]
        public virtual void RepoMethods_GetEntities_Schema_ReturnsResults()
        {
            EntitySchema entity1 = HiveModelCreationHelper.MockEntitySchema("test1", "test");
            EntitySchema entity2 = HiveModelCreationHelper.MockEntitySchema("test2", "test");
            EntitySchema entity3 = HiveModelCreationHelper.MockEntitySchema("test3", "test");
            EntitySchema entity4 = HiveModelCreationHelper.MockEntitySchema("test4", "test");
            EntitySchema entity5 = HiveModelCreationHelper.MockEntitySchema("test5", "test");

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, entity1, entity2, entity3, entity4, entity5);

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.Schemas.AddOrUpdate(entity1);
                writer.EntityRepository.Schemas.AddOrUpdate(entity2);
                writer.EntityRepository.Schemas.AddOrUpdate(entity3);
                writer.EntityRepository.Schemas.AddOrUpdate(entity4);
                writer.EntityRepository.Schemas.AddOrUpdate(entity5);

                // Save twice to be sure count is still what we expect
                writer.EntityRepository.Schemas.AddOrUpdate(entity1);
                writer.EntityRepository.Schemas.AddOrUpdate(entity2);
                writer.EntityRepository.Schemas.AddOrUpdate(entity3);
                writer.EntityRepository.Schemas.AddOrUpdate(entity4);
                writer.EntityRepository.Schemas.AddOrUpdate(entity5);

                writer.Complete();
            }

            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                IEnumerable<EntitySchema> result = reader.EntityRepository.Schemas.GetAll<EntitySchema>();
                Assert.AreEqual(5, result.Count());
            }
        }

        [Test]
        public virtual void Schema_AttributeGroups_DeleteAfterRemovalFromCollection()
        {
            AttributeGroup group1 = HiveModelCreationHelper.CreateAttributeGroup("group1", "group", 0);
            AttributeGroup group2 = HiveModelCreationHelper.CreateAttributeGroup("group2", "group", 1);
            AttributeGroup group3 = HiveModelCreationHelper.CreateAttributeGroup("group3", "group", 2);
            AttributeGroup group4 = HiveModelCreationHelper.CreateAttributeGroup("group4", "group", 3);

            AttributeDefinition attrib3 = HiveModelCreationHelper.CreateAttributeDefinition("test3", "test", "test", HiveModelCreationHelper.CreateAttributeType("type", "type", "type"), group3);
            AttributeDefinition attrib4 = HiveModelCreationHelper.CreateAttributeDefinition("test4", "test", "test", HiveModelCreationHelper.CreateAttributeType("type", "type", "type"), group4);

            EntitySchema schema = HiveModelCreationHelper.CreateEntitySchema("schema", "schema", null);
            schema.AttributeGroups.Add(group1);
            schema.AttributeGroups.Add(group2);
            schema.AttributeDefinitions.Add(attrib3);
            schema.AttributeDefinitions.Add(attrib4);

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, schema, group1, group2, group3, group4, attrib3, attrib4);

            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                writer.EntityRepository.Schemas.AddOrUpdate(schema);
                writer.Complete();

                Assert.AreEqual(4, schema.AttributeGroups.Count, "Group counts differ after saving");
                Assert.AreEqual(2, schema.AttributeDefinitions.Count, "Def counts != 2 after saving");
            }

            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var schemaReloaded = reader.EntityRepository.Schemas.Get<EntitySchema>(schema.Id);
                Assert.NotNull(schemaReloaded);
                Assert.AreEqual(4, schemaReloaded.AttributeGroups.Count, "Group counts differ");
                Assert.AreEqual(2, schemaReloaded.AttributeDefinitions.Count, "Def counts != 2");
            }

            PostWriteCallback.Invoke();

            HiveId deletedGroup;
            using (var writer = ProviderSetup.UnitFactory.Create())
            {
                var schemaReloaded = writer.EntityRepository.Schemas.Get<EntitySchema>(schema.Id);
                Assert.IsNotNull(schemaReloaded, "Could not get item from db");

                AttributeGroup groupToDelete = schemaReloaded.AttributeGroups.OrderBy(x => x.Ordinal).ElementAt(2);
                deletedGroup = groupToDelete.Id;
                schemaReloaded.AttributeGroups.Remove(groupToDelete);

                Assert.AreEqual(3, schemaReloaded.AttributeGroups.Count, "Removal didn't affect count");
                Assert.AreEqual(1, schemaReloaded.AttributeDefinitions.Count, "Removal didn't affect attrib def count");

                writer.EntityRepository.Schemas.AddOrUpdate(schemaReloaded);
                writer.Complete();
            }

            PostWriteCallback.Invoke();

            using (var reader = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var schemaReloaded = reader.EntityRepository.Schemas.Get<EntitySchema>(schema.Id);
                Assert.IsNotNull(schemaReloaded, "Could not get item from db");
                Assert.AreEqual(3, schemaReloaded.AttributeGroups.Count, "Removal didn't affect count after reloading");
                Assert.AreEqual(1, schemaReloaded.AttributeDefinitions.Count, "Removal didn't affect attrib def count after reloading");
                Assert.IsFalse(schemaReloaded.AttributeGroups.Any(x => x.Id == deletedGroup), "Deleted group still exists - wrong one was deleted");
            }
        }
    }
}
