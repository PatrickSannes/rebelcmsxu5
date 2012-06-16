using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.NHibernate;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Hive;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.NHibernate
{
    [TestFixture]
    public class SessionTests
    {

        [TestCase]
        public void GetIdPath_Returns_In_Correct_Order_For_Entities()
        {
            //mock hive
            IReadonlyEntityRepositoryGroup<IContentStore> readonlyEntitySession;
            IReadonlySchemaRepositoryGroup<IContentStore> readonlySchemaSession;
            IEntityRepositoryGroup<IContentStore> entityRepository;
            ISchemaRepositoryGroup<IContentStore> schemaSession;
            var hive = MockHiveManager.GetManager().MockContentStore(out readonlyEntitySession, out readonlySchemaSession, out entityRepository, out schemaSession);
            var entity = new TypedEntity {Id = new HiveId(100)};
            entityRepository.Get<TypedEntity>(Arg.Any<bool>(), Arg.Any<HiveId[]>()).Returns(new[] {entity});
            entityRepository.GetAncestorRelations(new HiveId(100), FixedRelationTypes.DefaultRelationType)
                .Returns(new[]
                    {
                        new Relation(FixedRelationTypes.DefaultRelationType, new TypedEntity{Id = new HiveId(99)}, entity),
                        new Relation(FixedRelationTypes.DefaultRelationType, new TypedEntity{Id = new HiveId(98)}, new TypedEntity{Id = new HiveId(99)}),
                        new Relation(FixedRelationTypes.DefaultRelationType, new TypedEntity{Id = new HiveId(97)}, new TypedEntity{Id = new HiveId(98)}),
                    });

            using (var uow = hive.OpenWriter<IContentStore>())
            {
                var path = uow.Repositories.GetEntityPath<TypedEntity>(new HiveId(100), FixedRelationTypes.DefaultRelationType);
                Assert.AreEqual(new HiveId(97), path.ElementAt(0));
                Assert.AreEqual(new HiveId(98), path.ElementAt(1));
                Assert.AreEqual(new HiveId(99), path.ElementAt(2));                
                Assert.AreEqual(new HiveId(100), path.ElementAt(3));
            }

        }

        [TestCase]
        public void GetIdPath_Returns_In_Correct_Order_For_Schemas()
        {
            //mock hive
            IReadonlyEntityRepositoryGroup<IContentStore> readonlyEntitySession;
            IReadonlySchemaRepositoryGroup<IContentStore> readonlySchemaSession;
            IEntityRepositoryGroup<IContentStore> entityRepository;
            ISchemaRepositoryGroup<IContentStore> schemaSession;
            var hive = MockHiveManager.GetManager().MockContentStore(out readonlyEntitySession, out readonlySchemaSession, out entityRepository, out schemaSession);
            var schema = new EntitySchema { Id = new HiveId(100) };
            schemaSession.Get<EntitySchema>(Arg.Any<bool>(), Arg.Any<HiveId[]>()).Returns(new[] { schema });
            schemaSession.GetAncestorRelations(new HiveId(100), FixedRelationTypes.DefaultRelationType)
                .Returns(new[]
                    {
                        new Relation(FixedRelationTypes.DefaultRelationType, new EntitySchema{Id = new HiveId(99)}, schema),
                        new Relation(FixedRelationTypes.DefaultRelationType, new EntitySchema{Id = new HiveId(98)}, new EntitySchema{Id = new HiveId(99)}),
                        new Relation(FixedRelationTypes.DefaultRelationType, new EntitySchema{Id = new HiveId(97)}, new EntitySchema{Id = new HiveId(98)}),
                    });

            using (var uow = hive.OpenWriter<IContentStore>())
            {
                var path = uow.Repositories.Schemas.GetEntityPath<EntitySchema>(new HiveId(100), FixedRelationTypes.DefaultRelationType);
                Assert.AreEqual(new HiveId(97), path.ElementAt(0));
                Assert.AreEqual(new HiveId(98), path.ElementAt(1));
                Assert.AreEqual(new HiveId(99), path.ElementAt(2));
                Assert.AreEqual(new HiveId(100), path.ElementAt(3));
            }

        }

        [TestCase]
        public void AddOneRemoveOne_EntityDoesntExist()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();

            HiveId id;
            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                var entity = HiveModelCreationHelper.CreateAttributeType("alias", "name", "description");
                uow.EntityRepository.Schemas.AddOrUpdate(entity);
                uow.Complete();
                id = entity.Id;
            }

            setup.SessionForTest.Clear();

            //Act

            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.Schemas.Delete<AttributeType>(id);
                uow.Complete();
            }

            //assert
            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                Assert.IsNull(uow.EntityRepository.Schemas.Get<AttributeType>(id));
            }
        }

        [TestCase]
        public void OnceDisposed_ExceptionsThrown()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();

            //Act
            var uow = setup.ProviderSetup.UnitFactory.Create();
            uow.Dispose();

            var entity = HiveModelCreationHelper.CreateAttributeType("alias", "name", "description");
            Assert.Throws<ObjectDisposedException>(() => { var accessorTest = uow.EntityRepository; });
            Assert.Throws<ObjectDisposedException>(() => { var accessorTest = uow.EntityRepository.Revisions; });
            Assert.Throws<ObjectDisposedException>(() => { var accessorTest = uow.EntityRepository.Schemas; });
            Assert.Throws<ObjectDisposedException>(() => { var accessorTest = uow.EntityRepository.Schemas.Revisions; });
            Assert.Throws<ObjectDisposedException>(() => uow.EntityRepository.Schemas.AddOrUpdate(entity));
        }

        [TestCase]
        public void Deleting_Schema_Removes_All_Entities()
        {
            Action<NhibernateTestSetupHelper> runTest = setupHelper =>
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

                using (var uow = setupHelper.ProviderSetup.UnitFactory.Create())
                {
                    uow.EntityRepository.Revisions.AddOrUpdate(content1);
                    //make some versions
                    uow.EntityRepository.Revisions.AddOrUpdate(content2);
                    uow.EntityRepository.Revisions.AddOrUpdate(content1);
                    uow.Complete();
                }
                //PostWriteCallback.Invoke();
                setupHelper.SessionForTest.Clear();

                //Act

                // First check that all items are loadable fine
                using (var uow = setupHelper.ProviderSetup.UnitFactory.Create())
                {
                    var attributeType = uow.EntityRepository.Schemas.Get<EntitySchema>(content1.Item.EntitySchema.Id);
                    Assert.IsNotNull(attributeType);
                    var revisionReloaded = uow.EntityRepository.Revisions.Get<TypedEntity>(content1.Item.Id, content1.MetaData.Id);
                    Assert.IsNotNull(revisionReloaded);
                    var contentReloaded = uow.EntityRepository.Get<TypedEntity>(content1.Item.Id);
                    Assert.IsNotNull(contentReloaded);
                }

                using (var uow = setupHelper.ProviderSetup.UnitFactory.Create())
                {
                    uow.EntityRepository.Schemas.Delete<EntitySchema>(content1.Item.EntitySchema.Id);
                    uow.Complete();
                }
                //PostWriteCallback.Invoke();
                setupHelper.SessionForTest.Clear();

                //Assert

                using (var uow = setupHelper.ProviderSetup.UnitFactory.Create())
                {
                    var attributeType = uow.EntityRepository.Schemas.Get<EntitySchema>(content1.Item.EntitySchema.Id);
                    Assert.IsNull(attributeType);
                    var revisionReloaded = uow.EntityRepository.Revisions.Get<TypedEntity>(content1.Item.Id, content1.MetaData.Id);
                    Assert.IsNull(revisionReloaded);
                    var contentReloaded = uow.EntityRepository.Get<TypedEntity>(content1.Item.Id);
                    Assert.IsNull(contentReloaded);
                }
                //PostWriteCallback.Invoke();
                setupHelper.SessionForTest.Clear();
            };

            //runTest(DirectReadWriteProvider);
            runTest(new NhibernateTestSetupHelper());
        }

        [TestCase]
        public void GetWithNullId_ResultsInArgumentNullException()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                //Assert
                Assert.Throws<ArgumentNullException>(() => writer.EntityRepository.Get<TypedEntity>(HiveId.Empty));
            }
        }

        [TestCase]
        public void OnceUnitOpened_TransactionPropertyIsPopulated()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();

            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                dynamic access = new AccessPrivateWrapper(writer.EntityRepository.Transaction);
                Assert.NotNull(access._nhTransaction);
            }
        }

        [TestCase]
        public void UnitTestCtor_SessionSurvivesUnits()
        {
            // Arrange
            var setup = new NhibernateTestSetupHelper();
            Guid entitySessionId;
            Guid revisionSessionId;
            Guid schemaSessionId;
            
            // Act
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                dynamic entitySession = new AccessPrivateWrapper(((EntityRepository)writer.EntityRepository).Helper.NhSession);
                dynamic schemaSession = new AccessPrivateWrapper(((SchemaRepository)writer.EntityRepository.Schemas).Helper.NhSession);
                dynamic revisionSession = new AccessPrivateWrapper(((RevisionRepository)writer.EntityRepository.Revisions).Helper.NhSession);
                entitySessionId = entitySession.SessionId;
                schemaSessionId = schemaSession.SessionId;
                revisionSessionId = revisionSession.SessionId;
            }

            // Assert
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                dynamic entitySession = new AccessPrivateWrapper(((EntityRepository) writer.EntityRepository).Helper.NhSession);
                dynamic schemaSession = new AccessPrivateWrapper(((SchemaRepository)writer.EntityRepository.Schemas).Helper.NhSession);
                dynamic revisionSession = new AccessPrivateWrapper(((RevisionRepository)writer.EntityRepository.Revisions).Helper.NhSession);

                Assert.AreNotEqual(entitySession.SessionId, Guid.Empty);
                Assert.AreNotEqual(schemaSession.SessionId, Guid.Empty);
                Assert.AreNotEqual(revisionSession.SessionId, Guid.Empty);
                Assert.AreEqual(entitySessionId, entitySession.SessionId);
                Assert.AreEqual(schemaSessionId, entitySession.SessionId);
                Assert.AreEqual(revisionSessionId, entitySession.SessionId);
                Assert.AreEqual(entitySession.SessionId, schemaSession.SessionId);
                Assert.AreEqual(entitySession.SessionId, revisionSession.SessionId);
            }
        }

        [TestCase]
        public void OnceUnitOfWorkCommitted_SchemaDataStoreUpdated()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();
            HiveId id;
            
            //Act
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                var entity = HiveModelCreationHelper.CreateAttributeType("alias", "name", "description");

                writer.EntityRepository.Schemas.AddOrUpdate(entity);
                writer.Complete();
                id = entity.Id;
                Assert.IsNotNull(writer.EntityRepository.Schemas.Get<AttributeType>(id));
            }

            setup.SessionForTest.Clear();

            //assert
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                Assert.IsNotNull(writer.EntityRepository.Schemas.Get<AttributeType>(id));
            }
        }

        [TestCase]
        public void IfUnitOfWorkUncomitted_EntityDataStoreNotUpdated()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();
            HiveId id;

            //act
            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                var entity = HiveModelCreationHelper.MockTypedEntity(false);
                uow.EntityRepository.AddOrUpdate(entity);
                id = entity.Id;
                Assert.IsNotNull(uow.EntityRepository.Get<TypedEntity>(id));
            }

            setup.SessionForTest.Clear();

            //assert
            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                Assert.AreNotEqual(id, HiveId.Empty);
                Assert.IsNull(uow.EntityRepository.Get<TypedEntity>(id));
            }
        }

        [TestCase]
        public void IfUnitOfWorkUncomitted_SchemaDataStoreNotUpdated()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();
            HiveId id;

            //act
            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                var entitySchema = HiveModelCreationHelper.MockEntitySchema("blah-alias", "blah-name");
                uow.EntityRepository.Schemas.AddOrUpdate(entitySchema);
                id = entitySchema.Id;
                Assert.IsNotNull(uow.EntityRepository.Schemas.Get<EntitySchema>(id));
            }

            setup.SessionForTest.Clear();

            //assert
            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                Assert.AreNotEqual(id, HiveId.Empty);
                Assert.IsNull(uow.EntityRepository.Schemas.Get<EntitySchema>(id));
            }
        }

        [TestCase]
        public void IfUnitOfWorkUncomitted_RevisionDataStoreNotUpdated()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();
            HiveId id;
            HiveId revisionId;

            //act
            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                var entity = HiveModelCreationHelper.MockTypedEntity(false);
                var revision = new Revision<TypedEntity>(entity);

                uow.EntityRepository.Revisions.AddOrUpdate(revision);

                id = revision.Item.Id;
                revisionId = revision.MetaData.Id;
            }

            setup.SessionForTest.Clear();

            //assert
            using (var uow = setup.ProviderSetup.UnitFactory.Create())
            {
                Assert.AreNotEqual(id, HiveId.Empty);
                Assert.AreNotEqual(revisionId, HiveId.Empty);
                Assert.IsNull(uow.EntityRepository.Revisions.Get<TypedEntity>(id, revisionId));
            }
        }

        [TestCase]
        public void OnceUnitOfWorkCommitted_EntityDataStoreUpdated()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();

            //Act
            HiveId id;
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                //var entitySchema = HiveModelCreationHelper.MockEntitySchema("blah-alias", "blah-name");
                var entity = HiveModelCreationHelper.MockTypedEntity(false);

                writer.EntityRepository.AddOrUpdate(entity);
                writer.Complete();
                id = entity.Id;
                Assert.IsNotNull(writer.EntityRepository.Get<TypedEntity>(id));
            }

            setup.SessionForTest.Clear();

            //assert
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                Assert.AreNotEqual(id, Guid.Empty);
                Assert.IsNotNull(writer.EntityRepository.Get<TypedEntity>(id));
            }
        }

        [TestCase]
        public void OnceUnitOfWorkCommitted_RevisionDataStoreUpdated()
        {
            //Arrange
            var setup = new NhibernateTestSetupHelper();
            HiveId id;
            HiveId revisionId;

            //Act
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                //var entitySchema = HiveModelCreationHelper.MockEntitySchema("blah-alias", "blah-name");
                var entity = HiveModelCreationHelper.MockTypedEntity(false);
                var revision = new Revision<TypedEntity>(entity);

                writer.EntityRepository.Revisions.AddOrUpdate(revision);
                writer.Complete();

                id = revision.Item.Id;
                revisionId = revision.MetaData.Id;
                Assert.IsNotNull(writer.EntityRepository.Revisions.Get<TypedEntity>(id, revisionId));
            }

            setup.SessionForTest.Clear();

            //assert
            using (var writer = setup.ProviderSetup.UnitFactory.Create())
            {
                Assert.AreNotEqual(id, Guid.Empty);
                Assert.IsNotNull(writer.EntityRepository.Revisions.Get<TypedEntity>(id, revisionId));
            }
        }
    }
}
