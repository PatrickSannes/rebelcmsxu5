using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    [TestFixture]
    public class RelationProxyCollectionTests
    {
        [Test]
        public void AddingItemsManually_AsParent_ThenAsChild_ThrowsDueToInfiniteCycle()
        {
            // Arrange
            var connectedParent = HiveModelCreationHelper.MockTypedEntity();
            var middleItem = HiveModelCreationHelper.MockTypedEntity();
            connectedParent.Id = HiveId.ConvertIntToGuid(1); // It's connected with a lazy-loader so it should have an Id
            middleItem.Id = HiveId.ConvertIntToGuid(2); // It's connected with a lazy-loader so it should have an Id
            var rpc = new RelationProxyCollection(middleItem);

            // Act
            // Add parent to this item
            rpc.EnlistParent(connectedParent, FixedRelationTypes.DefaultRelationType);

            // Assert
            // Try to add the parent as a child of the middle item, hoping it throws
            Assert.Throws<InvalidOperationException>(() => rpc.EnlistChild(connectedParent, FixedRelationTypes.DefaultRelationType));
        }


        [Test]
        public void AddingItemsManually_AsParent_CountIsCorrect()
        {
            // Arrange
            var disconnectedParent = HiveModelCreationHelper.MockTypedEntity();
            var disconnectedChild = HiveModelCreationHelper.MockTypedEntity();
            var rpc = new RelationProxyCollection(disconnectedChild);

            // Act
            rpc.EnlistParent(disconnectedParent, FixedRelationTypes.DefaultRelationType);
            
            // Assert
            Assert.AreEqual(1, rpc.Count());
            Assert.AreEqual(1, rpc.GetManualProxies().Count());
        }

        [Test]
        public void AddingItemsLazily_AsParent_CountIsCorrect()
        {
            // Arrange
            var connectedChild = HiveModelCreationHelper.MockTypedEntity();
            connectedChild.Id = HiveId.ConvertIntToGuid(2); // It's connected with a lazy-loader so it should have an Id
            var rpc = new RelationProxyCollection(connectedChild);

            // Act
            rpc.LazyLoadDelegate = ownerIdOfProxyColl =>
                                       {
                                           var parent = new RelationById(HiveId.ConvertIntToGuid(1), ownerIdOfProxyColl, FixedRelationTypes.DefaultRelationType, 0);
                                           return new RelationProxyBucket(new[] { parent }, Enumerable.Empty<RelationById>());
                                       };

            // Assert
            Assert.AreEqual(1, rpc.Count());
            Assert.AreEqual(0, rpc.GetManualProxies().Count());
        }

        [Test]
        public void RelationMetaDataCollection_EqualityCheck()
        {
            // Arrange
            var item1a = new RelationMetaDatum("blah", "1");
            var item2a = new RelationMetaDatum("blah", "2");
            var item3a = new RelationMetaDatum("blah", "3");

            var item1b = new RelationMetaDatum("blah", "1");
            var item2b = new RelationMetaDatum("blah", "2");
            var item3b = new RelationMetaDatum("blah", "3");

            var coll1 = new RelationMetaDataCollection();
            var coll2 = new RelationMetaDataCollection();
            var coll3 = new RelationMetaDataCollection() { item1a, item2a, item3a };
            var coll4 = new RelationMetaDataCollection() { item2b, item1b, item3b };

            // Assert
            Assert.IsTrue(coll1.Equals(coll2));
            Assert.IsTrue(coll3.Equals(coll4));
            Assert.IsFalse(coll1.Equals(coll3));
        }

        [Test]
        public void RelationById_EqualityCheck()
        {
            // Arrange
            var item1 = new RelationById(HiveId.ConvertIntToGuid(5), HiveId.ConvertIntToGuid(10), FixedRelationTypes.DefaultRelationType, 0);
            var item2 = new RelationById(HiveId.ConvertIntToGuid(5), HiveId.ConvertIntToGuid(10), FixedRelationTypes.DefaultRelationType, 0);
            var item3 = new RelationById(HiveId.ConvertIntToGuid(2), HiveId.ConvertIntToGuid(10), FixedRelationTypes.DefaultRelationType, 0);

            // Assert
            Assert.AreEqual(item1, item2);
            Assert.AreNotEqual(item1, item3);
        }

        [Test]
        public void AddingItemsLazily_AndManually_AsParent_CountIsCorrect()
        {
            // Arrange
            var existingItemForParent = HiveModelCreationHelper.MockTypedEntity();
            var unsavedItemForAddingAsParent = HiveModelCreationHelper.MockTypedEntity();
            var connectedChild = HiveModelCreationHelper.MockTypedEntity();

            existingItemForParent.Id = HiveId.ConvertIntToGuid(1); // Assign Ids to mimic a lazy-loading valid scenario
            connectedChild.Id = HiveId.ConvertIntToGuid(2); // It's connected with a lazy-loader so it should have an Id

            var rpc = new RelationProxyCollection(connectedChild);

            // Act
            // This delegate mimics a lazy-loader, which returns RelationById to stipulate that 
            // the lazy loader must return existing items from the datastore
            rpc.LazyLoadDelegate = ownerIdOfCollection =>
            {
                var parent = new RelationById(HiveId.ConvertIntToGuid(1), ownerIdOfCollection, FixedRelationTypes.DefaultRelationType, 0);
                var parentShouldOverrideManuallyAdded = new RelationById(existingItemForParent.Id, ownerIdOfCollection, FixedRelationTypes.DefaultRelationType, 0);
                return new RelationProxyBucket(new[] { parent, parentShouldOverrideManuallyAdded }, Enumerable.Empty<RelationById>());
            };
            rpc.EnlistParent(existingItemForParent, FixedRelationTypes.DefaultRelationType);

            // Assert
            Assert.AreEqual(1, rpc.Count());
            Assert.AreEqual(0, rpc.GetManualProxies().Count());

            // Try adding more; count should not be affected
            rpc.EnlistParent(existingItemForParent, FixedRelationTypes.DefaultRelationType);
            rpc.EnlistParent(existingItemForParent, FixedRelationTypes.DefaultRelationType);
            rpc.EnlistParent(existingItemForParent, FixedRelationTypes.DefaultRelationType);
            rpc.EnlistParent(existingItemForParent, FixedRelationTypes.DefaultRelationType);
            Assert.AreEqual(1, rpc.Count());
            Assert.AreEqual(0, rpc.GetManualProxies().Count());

            // This is a second parent so should affect count
            rpc.EnlistParent(unsavedItemForAddingAsParent, FixedRelationTypes.DefaultRelationType);
            Assert.AreEqual(2, rpc.Count());
            Assert.AreEqual(1, rpc.GetManualProxies().Count());
        }
    }
}
