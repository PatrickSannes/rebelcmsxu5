using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    [TestFixture]
    public class GroupActorTests
    {
        [TestFixtureSetUp]
        public void SetupLog4net()
        {
            TestHelper.SetupLog4NetForTests();
        }

        // Note: to keep compatibility with NCrunch 1.33b, Value and Range attributes are not used here
        [TestCase(1, 1, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(1, 1, 2)]
        [TestCase(1, 2, 0)]
        [TestCase(1, 2, 1)]
        [TestCase(1, 2, 2)]
        [TestCase(2, 1, 0)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 1)]
        [TestCase(2, 2, 2)]
        public void WhenGetIsCalled_WithMultipleIds_MultipleItemsAreReturned(
            int allItemCount,
            int numbProviderCount,
            int numberOfPassthroughProviders)
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var providerGroup = GroupedProviderMockHelper.GenerateProviderGroup(numbProviderCount, numberOfPassthroughProviders, allItemCount, context);
            var idRoot = new Uri("oh-yeah://this-is-my-root/");
            var groupUnitFactory = new GroupUnitFactory(providerGroup.Writers, idRoot, FakeHiveCmsManager.CreateFakeRepositoryContext(context), context);
            HiveId int1 = HiveId.ConvertIntToGuid(1);
            HiveId int2 = HiveId.ConvertIntToGuid(2);
            HiveId int3 = HiveId.ConvertIntToGuid(3);


            // Act & Assert
            using (var uow = groupUnitFactory.Create<IContentStore>())
            {
                Assert.NotNull(uow.IdRoot);
                Assert.AreEqual(uow.IdRoot, idRoot);

                // TODO: This looks like a poor test since Get with multiple ids is actually just mocked to return three
                // but the purpose of this test is to establish that when running with multiple providers, some of which
                // are passthrough, only the correct number of items should be returned
                var items = uow.Repositories.Get<TypedEntity>(true, int1, int2, int3);
                Assert.That(items.Count(), Is.EqualTo(3 * numbProviderCount), "item count wrong");

                // Assert that the correct number of relations are returned too including passthrough providers being filtered for
                var parents = uow.Repositories.GetParentRelations(HiveId.Empty);
                Assert.That(parents.Count(), Is.EqualTo(GroupedProviderMockHelper.MockRelationCount * numbProviderCount), "parents count wrong");
            }
        }

        // Note: to keep compatibility with NCrunch 1.33b, Value and Range attributes are not used here
        [TestCase(1, 1, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(1, 1, 2)]
        [TestCase(1, 2, 0)]
        [TestCase(1, 2, 1)]
        [TestCase(1, 2, 2)]
        [TestCase(2, 1, 0)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 2, 1)]
        [TestCase(2, 2, 2)]
        public void WhenProviderIsMatched_IdIsRemappedToMappingGroup_OnReturn(
            int allItemCount,
            int numbProviderCount,
            int numberOfPassthroughProviders)
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var providerGroup = GroupedProviderMockHelper.GenerateProviderGroup(numbProviderCount, numberOfPassthroughProviders, allItemCount, context);
            var idRoot = new Uri("oh-yeah://this-is-my-root/");
            var groupUnitFactory = new GroupUnitFactory(providerGroup.Writers, idRoot, FakeHiveCmsManager.CreateFakeRepositoryContext(context), context);



            // Act & Assert
            using (var uow = groupUnitFactory.Create<IContentStore>())
            {
                Assert.NotNull(uow.IdRoot);
                Assert.AreEqual(uow.IdRoot, idRoot);

                var singleItem = uow.Repositories.Get<TypedEntity>(HiveId.Empty);
                Assert.NotNull(singleItem);
                Assert.NotNull(singleItem.Id.ProviderGroupRoot);
                Assert.AreEqual(singleItem.Id.ProviderGroupRoot, idRoot);
                Assert.NotNull(singleItem.Id.ProviderId);
                Assert.True(singleItem.Id.ToString(HiveIdFormatStyle.AsUri).StartsWith(idRoot.ToString()), "Was: " + singleItem.Id.ToFriendlyString());

                var items = uow.Repositories.GetAll<TypedEntity>();
                var itemShouldBeCount = allItemCount * numbProviderCount;
                Assert.AreEqual(itemShouldBeCount, items.Count());
                foreach (var typedEntity in items)
                {
                    AssertEntityIdIsRooted(idRoot, ((IReferenceByHiveId)typedEntity).Id);
                }

                AssertRelationsIdsHaveRoot(uow.Repositories.GetParentRelations(HiveId.Empty), idRoot);
                AssertRelationsIdsHaveRoot(uow.Repositories.GetDescendentRelations(HiveId.Empty), idRoot);
                AssertRelationsIdsHaveRoot(uow.Repositories.GetChildRelations(HiveId.Empty), idRoot);
                AssertRelationsIdsHaveRoot(uow.Repositories.GetAncestorRelations(HiveId.Empty), idRoot);
            }
        }

        [Test]
        public void HiveEntityExtensions_GetAllIdentifiableItems_Gets_All_Items()
        {
            var e = HiveModelCreationHelper.MockTypedEntity(false);
            var r = new Revision<TypedEntity>(e);
            
            var actualEntities = e.Attributes.Cast<IReferenceByHiveId>()
                //we need to get ALL unique gruops... this is because the mocked hive model doesn't assign the groups properly.
                .Concat(e.EntitySchema.AttributeGroups
                    .Concat(e.AttributeGroups)
                    .Concat(e.Attributes.Select(x => x.AttributeDefinition.AttributeGroup))
                    .Concat(e.EntitySchema.AttributeDefinitions.Select(x => x.AttributeGroup))
                    .Distinct().Cast<IReferenceByHiveId>())
                .Concat(e.EntitySchema.AttributeDefinitions.Cast<IReferenceByHiveId>())
                .Concat(e.EntitySchema.AttributeTypes.Cast<IReferenceByHiveId>())
                .Concat(new[] { (IReferenceByHiveId)e.EntitySchema, (IReferenceByHiveId)r.Item, (IReferenceByHiveId)r.MetaData })
                .Distinct();

            Debug.WriteLine("actual total attributes: " + e.Attributes.Count);
            Debug.WriteLine("actual total entity groups: " + e.AttributeGroups.Count());
            Debug.WriteLine("actual total schema groups: " + e.EntitySchema.AttributeGroups.Count());
            Debug.WriteLine("actual total unique groups: " + e.EntitySchema.AttributeGroups
                    .Concat(e.AttributeGroups)
                    .Concat(e.Attributes.Select(x => x.AttributeDefinition.AttributeGroup))
                    .Concat(e.EntitySchema.AttributeDefinitions.Select(x => x.AttributeGroup))
                    .Distinct().Count());
            Debug.WriteLine("actual total entity attribute defs: " + e.Attributes.Select(x => x.AttributeDefinition).Count());
            Debug.WriteLine("actual total schema attribute defs: " + e.EntitySchema.AttributeDefinitions.Count);
            Debug.WriteLine("actual total attribute types: " + e.EntitySchema.AttributeTypes.Count());

            var foundEntities = r.GetAllIdentifiableItems().Distinct().ToArray();

            Debug.WriteLine("found total revision data: " + foundEntities.Count(x => x is RevisionData));
            Debug.WriteLine("found total schemas: " + foundEntities.Count(x => x is EntitySchema));
            Debug.WriteLine("found total typed entities: " + foundEntities.Count(x => x is TypedEntity));
            Debug.WriteLine("found total attributes: " + foundEntities.Count(x => x is TypedAttribute));
            Debug.WriteLine("found total groups: " + foundEntities.Count(x => x is AttributeGroup));
            Debug.WriteLine("found total attribute defs: " + foundEntities.Count(x => x is AttributeDefinition));
            Debug.WriteLine("found total attribute types: " + foundEntities.Count(x => x is AttributeType));

            Assert.AreEqual(actualEntities.Count(), foundEntities.Count());
        }

        [Test]
        [Category("Performance")]
        public void HiveEntityExtensions_GetAllIdentifiableItems_Perf()
        {
            // Arrange
            var items = EnumerableExtensions.Range(count => HiveModelCreationHelper.MockTypedEntity(HiveId.ConvertIntToGuid(count + 1)), 250).ToArray();

            // Act & Assert
            using (DisposableTimer.TraceDuration<GroupActorTests>("Starting getting all dependents", "Finished"))
            {
                var allGraph = new List<IReferenceByHiveId>();
                foreach (var typedEntity in items)
                {
                    allGraph.AddRange(typedEntity.GetAllIdentifiableItems().ToArray());
                }
                LogHelper.TraceIfEnabled<GroupActorTests>("Found {0} items", () => allGraph.Count);
            }
        }

        [Test]
        public void WhenTypedEntity_IsReturnedFromGroupEntityRepository_AllObjects_HaveProviderId()
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var providerGroup = GroupedProviderMockHelper.GenerateProviderGroup(1, 0, 50, context);
            var idRoot = new Uri("myroot://yeah/");
            var groupUnitFactory = new GroupUnitFactory(providerGroup.Writers, idRoot, FakeHiveCmsManager.CreateFakeRepositoryContext(context), context);

            // Act & Assert
            using (var uow = groupUnitFactory.Create())
            {
                Assert.NotNull(uow.IdRoot);
                Assert.AreEqual(uow.IdRoot, idRoot);

                var singleItem = uow.Repositories.Get<TypedEntity>(HiveId.Empty);
                var allDependentItems = singleItem.GetAllIdentifiableItems().ToArray();
                Assert.IsTrue(allDependentItems.Any());
                Assert.That(allDependentItems.Select(x => x.Id).Distinct().Count(), Is.GreaterThan(1));
                foreach (var referenceByHiveId in allDependentItems)
                {
                    AssertEntityIdIsRooted(idRoot, referenceByHiveId.Id);
                }
            }
        }

        //[Test]
        //public void TempProfileTest()
        //{
        //    for (int i = 0; i < 5; i++)
        //    {
        //        WhenTypedEntity_IsReturnedFromGroupEntityRepository_ItsRelationProxies_HaveAbsoluteId();
        //    }
        //}

        [Test]
        public void WhenTypedEntity_IsReturnedFromGroupEntityRepository_ItsRelationProxies_HaveAbsoluteId()
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var providerGroup = GroupedProviderMockHelper.GenerateProviderGroup(1, 0, 50, context);
            var idRoot = new Uri("myroot://yeah/");
            var groupUnitFactory = new GroupUnitFactory(providerGroup.Writers, idRoot, FakeHiveCmsManager.CreateFakeRepositoryContext(context), context);

            // Act & Assert
            using (var uow = groupUnitFactory.Create())
            {
                Assert.NotNull(uow.IdRoot);
                Assert.AreEqual(uow.IdRoot, idRoot);

                var item = uow.Repositories.Get<TypedEntity>(HiveId.Empty);
                AssertIdsOfRelationProxiesForEntity(item, idRoot);

                var items = uow.Repositories.GetAll<TypedEntity>();
                Assert.True(items.Any());
                foreach (var typedEntity in items)
                {
                    AssertIdsOfRelationProxiesForEntity(typedEntity, idRoot);
                }
            }
        }

        private static void AssertIdsOfRelationProxiesForEntity(IRelatableEntity singleItem, Uri idRoot)
        {
            var message = "For: " + singleItem.Id.ToFriendlyString() + " (" + singleItem.GetType() + ")";
            var rpc = singleItem.RelationProxies;
            Assert.True(rpc.IsConnected, message);
            Assert.True(rpc.Any(), message);

            foreach (RelationProxy proxy in rpc)
            {
                Assert.False(proxy.Item.SourceId.IsNullValueOrEmpty());
                Assert.False(proxy.Item.DestinationId.IsNullValueOrEmpty());
                AssertEntityIdIsRooted(idRoot, proxy.Item.SourceId);
                AssertEntityIdIsRooted(idRoot, proxy.Item.DestinationId);
            }

            Assert.That(rpc.Count(), Is.EqualTo(rpc.AllChildRelations().Count() + rpc.AllParentRelations().Count()), message);
        }

        private static void AssertEntityIdIsRooted(Uri idRoot, HiveId hiveId)
        {
            var message = "For: " + hiveId.ToFriendlyString();
            Assert.NotNull(hiveId.ProviderGroupRoot, message);
            Assert.That(hiveId.ToString(HiveIdFormatStyle.AsUri), Is.StringStarting(idRoot.ToString()), message);
            Assert.AreEqual(hiveId.ProviderGroupRoot, idRoot, message);
            Assert.NotNull(hiveId.ProviderId, message);
        }

        private static void AssertRelationsIdsHaveRoot(IEnumerable<IRelationById> relations, Uri idRoot)
        {
            var enumerable = relations.ToArray();
            Assert.True(enumerable.Any());
            foreach (var relation in enumerable)
            {
                AssertEntityIdIsRooted(idRoot, relation.SourceId);
                AssertEntityIdIsRooted(idRoot, relation.DestinationId);
            }
        }
    }
}
