using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders;

namespace Umbraco.Tests.Cms
{
    using Umbraco.Tests.Extensions;

    [TestFixture]
    public class RenderViewModelExtensionsFixture : AbstractRenderViewModelExtensionsFixture
    {
        [Test]
        public void QueryAll_WithoutCriteria_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var queryAll = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager);
            var toList = queryAll.ToList();

            // Assert
            Assert.That(toList.Count(), Is.GreaterThan(0));
        }

        [Test]
        public void QueryAll_WithEntitySchemaAliasCriteria_StaticLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var firstItem = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).FirstOrDefault(x => x.EntitySchema.Alias == "redherring-schema");

            // Assert
            Assert.That(firstItem, Is.Not.Null);
        }

        [Test]
        public void Content_HasSortOrder()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var childGuid = new HiveId(Guid.NewGuid());
            var child2Guid = new HiveId(Guid.NewGuid());
            var parent = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            var child1 = this.AddChildNodeWithId(parent, childGuid, 1);
            var child2 = this.AddChildNodeWithId(parent, child2Guid, 2);

            var child1AsContent = this.HiveManager.FrameworkContext.TypeMappers.Map<Content>(child1);
            var child2AsContent = this.HiveManager.FrameworkContext.TypeMappers.Map<Content>(child2);

            Assert.NotNull(child1AsContent);
            Assert.That(child1AsContent.SortOrder, Is.EqualTo(1));
            Assert.NotNull(child2AsContent);
            Assert.That(child2AsContent.SortOrder, Is.EqualTo(2));
        }

        [Test]
        public void Content_LevelExtensionMethod()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var childGuid = new HiveId(Guid.NewGuid());
            var child1_1Guid = new HiveId(Guid.NewGuid());
            var child1_2Guid = new HiveId(Guid.NewGuid());
            var child1_3Guid = new HiveId(Guid.NewGuid());
            var child1_4Guid = new HiveId(Guid.NewGuid());
            var child1_1_1Guid = new HiveId(Guid.NewGuid());
            var child1_1_2Guid = new HiveId(Guid.NewGuid());
            var child2Guid = new HiveId(Guid.NewGuid());
            var parent = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            var child1 = this.AddChildNodeWithId(parent, childGuid, 1);
            var child1_1 = this.AddChildNodeWithId(child1, child1_1Guid, 1);
            var child1_2 = this.AddChildNodeWithId(child1, child1_2Guid, 2);
            var child1_3 = this.AddChildNodeWithId(child1, child1_3Guid, 3);
            var child1_4 = this.AddChildNodeWithId(child1, child1_4Guid, 4);
            var child1_1_1 = this.AddChildNodeWithId(child1_1, child1_1_1Guid, 1);
            var child1_1_2 = this.AddChildNodeWithId(child1_1, child1_1_2Guid, 1);
            var child2 = this.AddChildNodeWithId(parent, child2Guid, 2);

            var child1AsContent = this.HiveManager.FrameworkContext.TypeMappers.Map<Content>(child1);
            var child1_1_2AsContent = this.HiveManager.FrameworkContext.TypeMappers.Map<Content>(child1_1_2);

            var child1Path = child1AsContent.GetPath(this.HiveManager);
            Assert.That(child1Path.Count(), Is.EqualTo(2), "Path was: " + child1Path.ToString() + ", parent id is: " + parent.Id);
            Assert.That(child1Path.Level, Is.EqualTo(2));
            var child1_1_2Path = child1_1_2AsContent.GetPath(this.HiveManager);
            Assert.That(child1_1_2Path.Count(), Is.EqualTo(4), "Path was: " + child1_1_2Path.ToString() + ", parent id is: " + parent.Id);
        }

        [Test]
        [Ignore("Change test - Requires ISecurityService to build")]
        public void ParentOfT_AsTypedEntity()
        {
            //// Arrange
            //var newGuid = Guid.Parse("55598660-7AAF-4F16-89E5-1B21CE10139C");
            //var newGuidRedHerring = Guid.NewGuid();
            //var childGuid = new HiveId(Guid.Parse("4554D9CD-27B5-4E7B-BE5A-E077B2B3908A"));
            //var parent = HiveModelCreationHelper.SetupTestContentData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            //var child = this.AddChildNodeWithId(parent, childGuid);

            //// Act
            //var getParent = RenderViewModelExtensions.Parent<TypedEntity>(this.HiveManager, child.Id);
            
            //// Assert
            //Assert.That(getParent, Is.Not.Null);
        }

        [Test]
        [Ignore("Change test - Requires ISecurityService to build")]
        public void ParentOfT_AsContent()
        {
            //// Arrange
            //var newGuid = Guid.NewGuid();
            //var newGuidRedHerring = Guid.NewGuid();
            //var childGuid = new HiveId(Guid.NewGuid());
            //var parent = HiveModelCreationHelper.SetupTestContentData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            //var child = this.AddChildNodeWithId(parent, childGuid);

            //// Act
            //var getParent = RenderViewModelExtensions.Parent<Content>(this.HiveManager, child.Id);

            //// Assert
            //Assert.That(getParent, Is.Not.Null);
        }

        [Test]
        public void AncestorsOfT_AsContent()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var childGuid = new HiveId(Guid.NewGuid());
            var grandChildGuid = new HiveId(Guid.NewGuid());
            var parent = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            var child = this.AddChildNodeWithId(parent, childGuid);
            var grandChild = this.AddChildNodeWithId(child, grandChildGuid);

            // Act
            var getParent = RenderViewModelExtensions.GetAncestors<Content>(this.HiveManager, grandChild.Id);

            // Assert
            Assert.That(getParent, Is.Not.Null);
            Assert.That(getParent.Any(), Is.EqualTo(true));
            Assert.That(getParent.Count(), Is.EqualTo(2));
            Assert.That(getParent.First().Id, Is.EqualTo(child.Id));
            Assert.That(getParent.Skip(1).First().Id, Is.EqualTo(parent.Id));
        }

        [Test]
        public void QueryAll_WithoutCriteria_AsContent()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var queryAll = RenderViewModelQueryExtensions.QueryAll<Content>(this.HiveManager);
            var toList = queryAll.ToList();

            // Assert
            Assert.That(toList.Count(), Is.GreaterThan(0));
        }

        [Test]
        public void Field_WithComplexValue_CanAccessSpecificKey()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var entity = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var value2 = entity.Field<string>(NodeNameAttributeDefinition.AliasValue, "Name");
            var value3 = entity.Field<string>(NodeNameAttributeDefinition.AliasValue, "UrlName");

            // Assert
            Assert.That(value2 == "my-test-name");
            Assert.That(value3 == "my-test-route");
        }
    }
}
