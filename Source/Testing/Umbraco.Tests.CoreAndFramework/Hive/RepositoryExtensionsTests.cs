using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    [TestFixture]
    public class RepositoryExtensionsTests
    {
        protected ProviderSetup ProviderSetup { get; private set; }
        protected ReadonlyProviderSetup ReadonlyProviderSetup { get; private set; }

        [SetUp]
        public void Setup()
        {
            var helper = new NhibernateTestSetupHelper(new FakeFrameworkContext());

            ProviderSetup = helper.ProviderSetup;
            ReadonlyProviderSetup = helper.ReadonlyProviderSetup;
        }

        [Test]
        public void RepositoryExtensionsTests_GetIdPaths_SimpleLinearInheritance()
        {
            /*
             * - Root
             *   - Child
             *     - Grandchild
             */

            // Arrange
            var root = HiveModelCreationHelper.MockTypedEntity();
            var child = HiveModelCreationHelper.MockTypedEntity();
            var grandchild = HiveModelCreationHelper.MockTypedEntity();

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, root, child, grandchild);

            // Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                // Add entities
                uow.EntityRepository.AddOrUpdate(root);
                uow.EntityRepository.AddOrUpdate(child);
                uow.EntityRepository.AddOrUpdate(grandchild);

                // Add all relations
                uow.EntityRepository.AddRelation(root, child, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(child, grandchild, FixedRelationTypes.DefaultRelationType, 1);
                uow.Complete();
            }

            // Assert
            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var paths = uow.EntityRepository.GetEntityPaths(grandchild.Id, FixedRelationTypes.DefaultRelationType);

                Assert.IsTrue(paths.DestinationId == grandchild.Id);
                Assert.IsTrue(paths.Count() == 1);
                Assert.IsTrue(paths[0].Count() == 3);
                Assert.IsTrue(paths[0][0] == root.Id);
                Assert.IsTrue(paths[0][1] == child.Id);
                Assert.IsTrue(paths[0][2] == grandchild.Id);
            }
        }

        [Test]
        public void RepositoryExtensionsTests_GetIdPaths_SimpleMultiInheritance()
        {
            /*
             * - Root
             *   - Child 1
             *     - Grandchild
             *   - Child 2
             *     - Grandchild
             */

            // Arrange
            var root = HiveModelCreationHelper.MockTypedEntity();
            var child1 = HiveModelCreationHelper.MockTypedEntity();
            var child2 = HiveModelCreationHelper.MockTypedEntity();
            var grandchild = HiveModelCreationHelper.MockTypedEntity();

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, root, child1, child2, grandchild);

            // Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                // Add entities
                uow.EntityRepository.AddOrUpdate(root);
                uow.EntityRepository.AddOrUpdate(child1);
                uow.EntityRepository.AddOrUpdate(child2);
                uow.EntityRepository.AddOrUpdate(grandchild);

                // Add all relations
                uow.EntityRepository.AddRelation(root, child1, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(root, child2, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(child1, grandchild, FixedRelationTypes.DefaultRelationType, 1);
                uow.EntityRepository.AddRelation(child2, grandchild, FixedRelationTypes.DefaultRelationType, 2);
                uow.Complete();
            }

            // Assert
            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var paths = uow.EntityRepository.GetEntityPaths(grandchild.Id, FixedRelationTypes.DefaultRelationType);

                Assert.IsTrue(paths.DestinationId == grandchild.Id);
                Assert.IsTrue(paths.Count() == 2);
                Assert.IsTrue(paths[0].Count() == 3);
                Assert.IsTrue(paths[0][0] == root.Id);
                Assert.IsTrue(paths[0][1] == child1.Id);
                Assert.IsTrue(paths[0][2] == grandchild.Id);
                Assert.IsTrue(paths[1].Count() == 3);
                Assert.IsTrue(paths[1][0] == root.Id);
                Assert.IsTrue(paths[1][1] == child2.Id);
                Assert.IsTrue(paths[1][2] == grandchild.Id);
            }
        }

        [Test]
        public void RepositoryExtensionsTests_GetIdPaths_ComplexMultiInheritance_SimpleForking()
        {
            /*
             * - Root
             *   - Child 1
             *     - Grandchild 1
             *       - Great Grandchild
             *   - Child 2
             *     - Grandchild 2
             *       - Great Grandchild
             *   - Child 3
             *     - Great Grandchild
             */

            // Arrange
            var root = HiveModelCreationHelper.MockTypedEntity();
            var child1 = HiveModelCreationHelper.MockTypedEntity();
            var child2 = HiveModelCreationHelper.MockTypedEntity();
            var child3 = HiveModelCreationHelper.MockTypedEntity();
            var grandchild1 = HiveModelCreationHelper.MockTypedEntity();
            var grandchild2 = HiveModelCreationHelper.MockTypedEntity();
            var greatGrandchild = HiveModelCreationHelper.MockTypedEntity();

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, root, child1, child2, child3, grandchild1, grandchild2, greatGrandchild);

            // Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                // Add entities
                uow.EntityRepository.AddOrUpdate(root);
                uow.EntityRepository.AddOrUpdate(child1);
                uow.EntityRepository.AddOrUpdate(child2);
                uow.EntityRepository.AddOrUpdate(child3);
                uow.EntityRepository.AddOrUpdate(grandchild1);
                uow.EntityRepository.AddOrUpdate(grandchild2);
                uow.EntityRepository.AddOrUpdate(greatGrandchild);

                // Add all relations
                uow.EntityRepository.AddRelation(root, child1, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(root, child2, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(root, child3, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(child1, grandchild1, FixedRelationTypes.DefaultRelationType, 1);
                uow.EntityRepository.AddRelation(child2, grandchild2, FixedRelationTypes.DefaultRelationType, 2);
                uow.EntityRepository.AddRelation(grandchild1, greatGrandchild, FixedRelationTypes.DefaultRelationType, 1);
                uow.EntityRepository.AddRelation(grandchild2, greatGrandchild, FixedRelationTypes.DefaultRelationType, 1);
                uow.EntityRepository.AddRelation(child3, greatGrandchild, FixedRelationTypes.DefaultRelationType, 1);
                uow.Complete();
            }

            // Assert
            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var paths = uow.EntityRepository.GetEntityPaths(greatGrandchild.Id, FixedRelationTypes.DefaultRelationType);

                Assert.IsTrue(paths.DestinationId == greatGrandchild.Id);
                Assert.IsTrue(paths.Count() == 3);
                Assert.IsTrue(paths[0].Count() == 4);
                Assert.IsTrue(paths[0][0] == root.Id);
                Assert.IsTrue(paths[0][1] == child1.Id);
                Assert.IsTrue(paths[0][2] == grandchild1.Id);
                Assert.IsTrue(paths[0][3] == greatGrandchild.Id);
                Assert.IsTrue(paths[1].Count() == 4);
                Assert.IsTrue(paths[1][0] == root.Id);
                Assert.IsTrue(paths[1][1] == child2.Id);
                Assert.IsTrue(paths[1][2] == grandchild2.Id);
                Assert.IsTrue(paths[1][3] == greatGrandchild.Id);
                Assert.IsTrue(paths[2].Count() == 3);
                Assert.IsTrue(paths[2][0] == root.Id);
                Assert.IsTrue(paths[2][1] == child3.Id);
                Assert.IsTrue(paths[2][2] == greatGrandchild.Id);
            }
        }

        [Test]
        public void RepositoryExtensionsTests_GetIdPaths_ComplexMultiInheritance_ComplexForking()
        {
            /*
             * - Root
             *   - Child 1
             *     - Grandchild 1
             *       - Great Grandchild
             *   - Child 2
             *     - Grandchild 2
             *       - Great Grandchild
             *   - Child 3
             *     - Grandchild 2
             */

            // Arrange
            var root = HiveModelCreationHelper.MockTypedEntity();
            var child1 = HiveModelCreationHelper.MockTypedEntity();
            var child2 = HiveModelCreationHelper.MockTypedEntity();
            var child3 = HiveModelCreationHelper.MockTypedEntity();
            var grandchild1 = HiveModelCreationHelper.MockTypedEntity();
            var grandchild2 = HiveModelCreationHelper.MockTypedEntity();
            var greatGrandchild = HiveModelCreationHelper.MockTypedEntity();

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, root, child1, child2, child3, grandchild1, grandchild2, greatGrandchild);

            // Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                // Add entities
                uow.EntityRepository.AddOrUpdate(root);
                uow.EntityRepository.AddOrUpdate(child1);
                uow.EntityRepository.AddOrUpdate(child2);
                uow.EntityRepository.AddOrUpdate(child3);
                uow.EntityRepository.AddOrUpdate(grandchild1);
                uow.EntityRepository.AddOrUpdate(grandchild2);
                uow.EntityRepository.AddOrUpdate(greatGrandchild);

                // Add all relations
                uow.EntityRepository.AddRelation(root, child1, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(root, child2, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(root, child3, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(child1, grandchild1, FixedRelationTypes.DefaultRelationType, 1);
                uow.EntityRepository.AddRelation(child2, grandchild2, FixedRelationTypes.DefaultRelationType, 2);
                uow.EntityRepository.AddRelation(grandchild1, greatGrandchild, FixedRelationTypes.DefaultRelationType, 1);
                uow.EntityRepository.AddRelation(grandchild2, greatGrandchild, FixedRelationTypes.DefaultRelationType, 1);
                uow.EntityRepository.AddRelation(child3, grandchild2, FixedRelationTypes.DefaultRelationType, 1);
                uow.Complete();
            }

            // Assert
            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                var paths = uow.EntityRepository.GetEntityPaths(greatGrandchild.Id, FixedRelationTypes.DefaultRelationType);

                Assert.IsTrue(paths.DestinationId == greatGrandchild.Id);
                Assert.IsTrue(paths.Count() == 3);
                Assert.IsTrue(paths[0].Count() == 4);
                Assert.IsTrue(paths[0][0] == root.Id);
                Assert.IsTrue(paths[0][1] == child1.Id);
                Assert.IsTrue(paths[0][2] == grandchild1.Id);
                Assert.IsTrue(paths[0][3] == greatGrandchild.Id);
                Assert.IsTrue(paths[1].Count() == 4);
                Assert.IsTrue(paths[1][0] == root.Id);
                Assert.IsTrue(paths[1][1] == child2.Id);
                Assert.IsTrue(paths[1][2] == grandchild2.Id);
                Assert.IsTrue(paths[1][3] == greatGrandchild.Id);
                Assert.IsTrue(paths[2].Count() == 4);
                Assert.IsTrue(paths[2][0] == root.Id);
                Assert.IsTrue(paths[2][1] == child3.Id);
                Assert.IsTrue(paths[2][2] == grandchild2.Id);
                Assert.IsTrue(paths[2][3] == greatGrandchild.Id);
            }
        }

        [Test]
        public void RepositoryExtensionsTests_GetIdPaths_InfinateLoop()
        {
            /*
             * - Root
             *   - Child
             *     - Grandchild
             *       - Child
             *         - Grandchild
             *           - ...
             */

            // Arrange
            var root = HiveModelCreationHelper.MockTypedEntity();
            var child = HiveModelCreationHelper.MockTypedEntity();
            var grandchild = HiveModelCreationHelper.MockTypedEntity();

            AssignFakeIdsIfPassthrough(ProviderSetup.ProviderMetadata, root, child, grandchild);

            // Act
            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                // Add entities
                uow.EntityRepository.AddOrUpdate(root);
                uow.EntityRepository.AddOrUpdate(child);
                uow.EntityRepository.AddOrUpdate(grandchild);

                // Add all relations
                uow.EntityRepository.AddRelation(root, child, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(child, grandchild, FixedRelationTypes.DefaultRelationType, 0);
                uow.EntityRepository.AddRelation(grandchild, child, FixedRelationTypes.DefaultRelationType, 0);
                uow.Complete();
            }

            // Assert
            using (var uow = ReadonlyProviderSetup.ReadonlyUnitFactory.CreateReadonly())
            {
                Assert.Throws<InvalidOperationException>(
                    () => uow.EntityRepository.GetEntityPaths(grandchild.Id, FixedRelationTypes.DefaultRelationType));
            }
        }

        #region HelperMethods

        private void AssignFakeIdsIfPassthrough(ProviderMetadata providerMetadata, params IReferenceByHiveId[] entity)
        {
            if (!providerMetadata.IsPassthroughProvider) 
                return;

            entity.ForEach(
                x =>
                {
                    var allItems = x.GetAllIdentifiableItems();
                    foreach (var referenceByHiveId in allItems)
                    {
                        // Only change / set certain ids otherwise relations are broken e.g. between an attributedefinition and its group
                        if (referenceByHiveId.Id.IsNullValueOrEmpty() && (TypeFinder.IsTypeAssignableFrom<TypedEntity>(referenceByHiveId)
                            || TypeFinder.IsTypeAssignableFrom<EntitySchema>(referenceByHiveId)
                            || TypeFinder.IsTypeAssignableFrom<Revision<TypedEntity>>(referenceByHiveId)))
                            referenceByHiveId.Id = new HiveId(providerMetadata.MappingRoot, providerMetadata.Alias, new HiveIdValue(Guid.NewGuid()));
                    }

                    // If we've specifically been given the item, then set its id irrespective of the rule above
                    if (x.Id.IsNullValueOrEmpty())
                        x.Id = new HiveId(providerMetadata.MappingRoot, providerMetadata.Alias, new HiveIdValue(Guid.NewGuid()));
                });
        }

        #endregion
    }
}
