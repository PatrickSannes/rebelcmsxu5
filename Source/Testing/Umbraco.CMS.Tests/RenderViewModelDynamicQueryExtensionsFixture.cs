namespace Umbraco.Tests.Cms
{
    using System;

    using System.Linq;

    using NUnit.Framework;

    using Umbraco.Cms.Web;

    using Umbraco.Framework;

    using Umbraco.Framework.Persistence.Model;

    using Umbraco.Framework.Persistence.Model.Attribution;

    using Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders;

    using Umbraco.Tests.Extensions;

    [TestFixture]
    public class RenderViewModelDynamicQueryExtensionsFixture : AbstractRenderViewModelExtensionsFixture
    {
        [Test]
        public void QueryAll_First_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var firstItem = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).First<TypedEntity>("aliasForQuerying == @0", "my-new-value");

            // Assert
            Assert.That(firstItem, Is.Not.Null);
            Assert.Throws<InvalidOperationException>(() => RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).First<TypedEntity>("nonExistant == @0", "should throw"));
        }

        [Test]
        public void QueryAll_FirstOrDefault_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var firstItem = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).FirstOrDefault<TypedEntity>("aliasForQuerying == @0", "my-new-value");
            var nullItem = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).FirstOrDefault<TypedEntity>("nonExistant == @0", "anything, as should return null");

            // Assert
            Assert.That(firstItem, Is.Not.Null);
            Assert.That(nullItem, Is.Null);
        }

        [Test]
        public void QueryAll_Single_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var existingEntity = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            this.AddDuplicateEntityTestData(existingEntity);

            // Act
            var singleItem = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Single<TypedEntity>("uniqueAliasForQuerying == @0", "my-new-value");

            // Assert
            Assert.That(singleItem, Is.Not.Null);
            Assert.Throws<InvalidOperationException>(() => RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Single<TypedEntity>("aliasForQuerying == @0", "my-new-value"));
        }

        [Test]
        public void QueryAll_SingleOrDefault_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var existingEntity = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            this.AddDuplicateEntityTestData(existingEntity);

            // Act
            var singleItem = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).SingleOrDefault<TypedEntity>("uniqueAliasForQuerying == @0", "my-new-value");
            var nullItem = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).SingleOrDefault<TypedEntity>("nonExistant == @0", "anything, as should return null");

            // Assert
            Assert.That(singleItem, Is.Not.Null);
            Assert.That(nullItem, Is.Null);
        }

        [Test]
        public void QueryAll_Count_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var existingEntity = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            this.AddDuplicateEntityTestData(existingEntity);

            // Act
            var countUnique = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Count<TypedEntity>("uniqueAliasForQuerying == @0", "my-new-value");
            var countNothing = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Count<TypedEntity>("nonExistant == @0", "anything, as should return null");
            var countDupes = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Count<TypedEntity>("aliasForQuerying == @0", "my-new-value");

            // Assert
            Assert.That(countUnique, Is.EqualTo(1));
            Assert.That(countNothing, Is.EqualTo(0));
            Assert.That(countDupes, Is.EqualTo(2));
        }

        [Test]
        public void QueryAll_Skip_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var existingEntity = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            this.AddDuplicateEntityTestData(existingEntity);

            // Act
            var countDupes = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Count<TypedEntity>("aliasForQuerying == @0", "my-new-value");
            var countSkipDupes = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Skip<TypedEntity>(1, "aliasForQuerying == @0", "my-new-value").ToList().Count();

            // Assert
            Assert.That(countDupes, Is.EqualTo(2));
            Assert.That(countSkipDupes, Is.EqualTo(1));
        }

        [Test]
        public void QueryAll_Take_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var existingEntity = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            this.AddDuplicateEntityTestData(existingEntity);

            // Act
            var countDupes = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Count<TypedEntity>("aliasForQuerying == @0", "my-new-value");
            var countTakeOneDupe = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Take<TypedEntity>(1, "aliasForQuerying == @0", "my-new-value").ToList().Count();

            // Assert
            Assert.That(countDupes, Is.EqualTo(2));
            Assert.That(countTakeOneDupe, Is.EqualTo(1));
        }

        [Test]
        public void QueryAll_Any_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var existingEntity = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            this.AddDuplicateEntityTestData(existingEntity);

            // Act
            var anyUnique = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Any<TypedEntity>("uniqueAliasForQuerying == @0", "my-new-value");
            var anyNothing = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Any<TypedEntity>("nonExistant == @0", "anything, as should return null");
            var anyDupes = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).Any<TypedEntity>("aliasForQuerying == @0", "my-new-value");

            // Assert
            Assert.That(anyUnique, Is.EqualTo(true));
            Assert.That(anyNothing, Is.EqualTo(false));
            Assert.That(anyDupes, Is.EqualTo(true));
        }

        [Test]
        public void QueryAll_All_WithAliasCriteria_StringLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var existingEntity = HiveModelCreationHelper.SetupTestData(newGuid, Guid.Empty, this.Setup.ProviderSetup);

            this.AddDuplicateEntityTestData(existingEntity);

            // Act
            var allUnique = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).All<TypedEntity>("uniqueAliasForQuerying == @0", "my-new-value");
            var allNothing = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).All<TypedEntity>("nonExistant == @0", "anything, as should return null");
            var allDupes = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).All<TypedEntity>("aliasForQuerying == @0", "my-new-value");

            // Assert
            Assert.That(allUnique, Is.EqualTo(false));
            Assert.That(allNothing, Is.EqualTo(false));
            Assert.That(allDupes, Is.EqualTo(true));
        }

        private void AddDuplicateEntityTestData(TypedEntity existingEntity)
        {
            using (var uow = this.Setup.ProviderSetup.UnitFactory.Create())
            {
                // Save another item so that we should have two matching for the same attribute value
                existingEntity.Id = HiveId.Empty;
                // Add another attribute to this "clone" that should correctly then match a Single query for testing
                var existingDef = existingEntity.EntitySchema.AttributeDefinitions[0];
                var newDef = HiveModelCreationHelper.CreateAttributeDefinition(
                    "uniqueAliasForQuerying", "", "", existingDef.AttributeType, existingDef.AttributeGroup, true);
                existingEntity.EntitySchema.AttributeDefinitions.Add(newDef);
                existingEntity.Attributes.Add(new TypedAttribute(newDef, "my-new-value"));

                uow.EntityRepository.AddOrUpdate(existingEntity);
                uow.Complete();
            }
        }
    }
}