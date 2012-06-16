using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class EntityExtensionTests
    {
        [Test]
        public void DeepCopyEntity_ToNewParent_CopiesEntity_IncludingRelations()
        {
            // Arrange
            var originalParent = HiveModelCreationHelper.MockTypedEntity(true);
            originalParent.RelationProxies.LazyLoadDelegate = x => new RelationProxyBucket();
            var originalChild = HiveModelCreationHelper.MockTypedEntity(true);
            originalChild.RelationProxies.LazyLoadDelegate = x => new RelationProxyBucket();

            var newContainer1 = HiveModelCreationHelper.MockTypedEntity(true);
            var newContainer2 = HiveModelCreationHelper.MockTypedEntity(true);

            originalParent.RelationProxies.EnlistChild(
                originalChild,
                FixedRelationTypes.DefaultRelationType,
                0,
                new RelationMetaDatum("testkey", "testvalue"));

            originalChild.RelationProxies.EnlistParent(
                originalParent,
                FixedRelationTypes.DefaultRelationType,
                0,
                new RelationMetaDatum("testkey", "testvalue"));

            // Guard against bad test data
            foreach (var attribute in originalParent.Attributes)
            {
                Assert.AreNotEqual(HiveId.Empty, attribute.Id);
                Assert.NotNull(attribute.AttributeDefinition);
                Assert.AreNotEqual(HiveId.Empty, attribute.AttributeDefinition.Id);
            }

            Thread.Sleep(250); // Ensure copy dates are newer

            //Act & Assert
            var copyLeft = EntityExtensions.CreateDeepCopyToNewParent(originalParent, newContainer1, FixedRelationTypes.DefaultRelationType, 0);
            DoAssertDeepCopiedEntityToNewParent(copyLeft, originalParent, newContainer1);
            DoAssertDeepCopiedEntityChildren(copyLeft, originalParent);

            var copyRight = EntityExtensions.CreateDeepCopyToNewParent(originalChild, newContainer2, FixedRelationTypes.DefaultRelationType, 0);
            DoAssertDeepCopiedEntityToNewParent(copyRight, originalChild, newContainer2);
            DoAssertDeepCopiedEntityChildren(copyRight, originalChild);
        }



        [Test]
        public void DeepCopyEntity_CopiesEntity_IncludingRelations()
        {
            // Arrange
            var originalLeft = HiveModelCreationHelper.MockTypedEntity(true);
            originalLeft.RelationProxies.LazyLoadDelegate = x => new RelationProxyBucket();
            var originalRight = HiveModelCreationHelper.MockTypedEntity(true);
            originalRight.RelationProxies.LazyLoadDelegate = x => new RelationProxyBucket();

            originalLeft.RelationProxies.EnlistChild(
                originalRight,
                FixedRelationTypes.DefaultRelationType,
                0,
                new RelationMetaDatum("testkey", "testvalue"));

            originalRight.RelationProxies.EnlistParent(
                originalLeft,
                FixedRelationTypes.DefaultRelationType,
                0,
                new RelationMetaDatum("testkey", "testvalue"));

            // Guard against bad test data
            foreach (var attribute in originalLeft.Attributes)
            {
                Assert.AreNotEqual(HiveId.Empty, attribute.Id);
                Assert.NotNull(attribute.AttributeDefinition);
                Assert.AreNotEqual(HiveId.Empty, attribute.AttributeDefinition.Id);
            }

            Thread.Sleep(250); // Ensure copy dates are newer

            //Act & Assert
            var copyLeft = EntityExtensions.CreateDeepCopy(originalLeft);
            DoAssertDeepCopiedEntityToSameParent(copyLeft, originalLeft);
            DoAssertDeepCopiedEntityChildren(copyLeft, originalLeft);

            var copyRight = EntityExtensions.CreateDeepCopy(originalRight);
            DoAssertDeepCopiedEntityToSameParent(copyRight, originalRight);
            DoAssertDeepCopiedEntityChildren(copyRight, originalRight);
        }

        private static void DoAssertDeepCopiedEntityToNewParent(TypedEntity copy, TypedEntity original, TypedEntity newContainer)
        {
            DoAssertShallowCopiedEntity(original, copy);

            Assert.AreEqual(1, copy.RelationProxies.AllParentRelations().Count());
            var firstContainerRelation = copy.RelationProxies.AllParentRelations().FirstOrDefault();
            Assert.NotNull(firstContainerRelation);
            Assert.AreEqual(newContainer.Id, firstContainerRelation.Item.SourceId);
            Assert.AreEqual(copy.Id, firstContainerRelation.Item.DestinationId);
        }

        private static void DoAssertDeepCopiedEntityToSameParent(TypedEntity copy, TypedEntity original)
        {
            DoAssertShallowCopiedEntity(original, copy);

            var allOriginalParents = original.RelationProxies.AllParentRelations().ToArray();
            var allCopiedParnets = copy.RelationProxies.AllParentRelations().ToArray();

            Assert.AreEqual(allOriginalParents.Count(), allCopiedParnets.Count());

            for (int i = 0; i < allOriginalParents.Count(); i++)
            {
                var originalRelation = allOriginalParents.ElementAt(i);
                var copiedRelation = allCopiedParnets.ElementAt(i);
                Assert.NotNull(originalRelation);
                Assert.NotNull(copiedRelation);
            }
        }

        private static void DoAssertDeepCopiedEntityChildren(TypedEntity copy, TypedEntity original)
        {
            var allOriginalChildren = original.RelationProxies.AllChildRelations().ToArray();
            var allCopiedChildren = copy.RelationProxies.AllChildRelations().ToArray();

            Assert.AreEqual(allOriginalChildren.Count(), allCopiedChildren.Count());

            for (int i = 0; i < allOriginalChildren.Count(); i++)
            {
                var originalRelation = allOriginalChildren.ElementAt(i);
                var copiedRelation = allCopiedChildren.ElementAt(i);
                Assert.NotNull(originalRelation);
                Assert.NotNull(copiedRelation);
            }
        }

        [Test]
        public void ShallowCopyEntity_ThrowsException_IfEntityIsNull()
        {
            // Arrange
            TypedEntity nullTest = null;

            // Easy assert
            Assert.Throws<ArgumentNullException>(() => nullTest.CreateShallowCopy());
        }

        [Test]
        public void ShallowCopyAttribute_ThrowsException_IfEntityIsNull()
        {
            // Arrange
            TypedAttribute nullTest = null;

            // Easy assert
            Assert.Throws<ArgumentNullException>(() => nullTest.CreateShallowCopy());
        }

        [Test]
        public void ShallowCopyAttribute_CopiesAttribute_ResettingId()
        {
            // Arrange
            var attributeType = HiveModelCreationHelper.CreateAttributeType("test", "test-name", "test-description");
            attributeType.Id = new HiveId(Guid.NewGuid());
            var attributeGroup = HiveModelCreationHelper.CreateAttributeGroup("test", "name", 0);
            var attributeDef = HiveModelCreationHelper.CreateAttributeDefinition("alias", "name", "desc", attributeType, attributeGroup);
            var original = HiveModelCreationHelper.CreateAttribute(attributeDef, "my value");
            original.Id = new HiveId(Guid.NewGuid());

            // Act
            Thread.Sleep(250); // Ensure copy dates are newer
            var copy = EntityExtensions.CreateShallowCopy(original);

            // Assert
            Assert.AreNotEqual(copy.Id, original.Id);
            Assert.AreEqual(HiveId.Empty, copy.Id);
            Assert.That(copy.UtcCreated, Is.GreaterThan(original.UtcCreated));
            Assert.That(copy.UtcModified, Is.EqualTo(original.UtcModified));
            Assert.That(copy.UtcStatusChanged, Is.EqualTo(original.UtcStatusChanged));
        }

        [Test]
        public void ShallowCopyEntity_CopiesEntity_ResettingId()
        {
            // Arrange
            var original = HiveModelCreationHelper.MockTypedEntity(true);
            original.RelationProxies.LazyLoadDelegate = x => new RelationProxyBucket();

            // Guard against bad test data
            foreach (var attribute in original.Attributes)
            {
                Assert.AreNotEqual(HiveId.Empty, attribute.Id);
                Assert.NotNull(attribute.AttributeDefinition);
                Assert.AreNotEqual(HiveId.Empty, attribute.AttributeDefinition.Id);
            }

            // Act
            Thread.Sleep(250); // Ensure copy dates are newer
            var copy = EntityExtensions.CreateShallowCopy(original);

            // Assert
            DoAssertShallowCopiedEntity(original, copy);
        }

        private static void DoAssertShallowCopiedEntity(TypedEntity original, TypedEntity copy)
        {
            Assert.AreNotEqual(copy.Id, original.Id);
            Assert.AreEqual(HiveId.Empty, copy.Id);
            Assert.That(copy.UtcCreated, Is.GreaterThan(original.UtcCreated));
            Assert.That(copy.UtcModified, Is.EqualTo(original.UtcModified));
            Assert.That(copy.UtcStatusChanged, Is.EqualTo(original.UtcStatusChanged));
            Assert.That(copy.EntitySchema, Is.SameAs(original.EntitySchema));
            Assert.That(copy.Attributes, Is.Not.SameAs(original.Attributes));
            Assert.AreEqual(copy.Attributes.Count, original.Attributes.Count);
            Assert.AreEqual(copy.AttributeGroups.Count(), original.AttributeGroups.Count());
            foreach (var attribute in copy.Attributes)
            {
                Assert.AreEqual(HiveId.Empty, attribute.Id);
                Assert.NotNull(attribute.AttributeDefinition);
                Assert.AreNotEqual(HiveId.Empty, attribute.AttributeDefinition.Id);
            }
            Assert.AreEqual(copy.RelationProxies.LazyLoadDelegate, original.RelationProxies.LazyLoadDelegate);
        }
    }
}
