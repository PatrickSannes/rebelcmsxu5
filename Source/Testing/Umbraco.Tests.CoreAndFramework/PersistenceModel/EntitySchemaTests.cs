using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.PersistenceModel
{
    [TestFixture]
    public class EntitySchemaTests : AbstractPartialTrustFixture<EntitySchemaTests>
    {
        private readonly IAttributeTypeRegistry _attributeTypeRegistry = new CmsAttributeTypeRegistry();

        [Test]
        public void Adding_AttributeDefinitions_AddsToGroupsOnSchema()
        {
            // Arrange
            var attributionSchema = HiveModelCreationHelper.CreateEntitySchema("test", "Test", new AttributeDefinition[] { });
            Assert.AreEqual(0, attributionSchema.AttributeGroups.Count);
            var nameNameType = _attributeTypeRegistry.GetAttributeType(NodeNameAttributeType.AliasValue);
            var fileUploadType = _attributeTypeRegistry.GetAttributeType(FileUploadAttributeType.AliasValue);

            // Act & Assert
            attributionSchema.AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
            {
                Id = new HiveId("mi-name".EncodeAsGuid()),
                AttributeType = nameNameType,
                AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                Ordinal = 0
            });
            Assert.AreEqual(1, attributionSchema.AttributeDefinitions.Count);
            Assert.AreEqual(1, attributionSchema.AttributeGroups.Count);
            Assert.That(attributionSchema.AttributeGroups, Has.Some.EqualTo(FixedGroupDefinitions.GeneralGroup));

            attributionSchema.AttributeDefinitions.Add(new AttributeDefinition("uploadedFile", "Uploaded File")
            {
                Id = new HiveId("mi-upload".EncodeAsGuid()),
                AttributeType = fileUploadType,
                AttributeGroup = FixedGroupDefinitions.FileProperties,
                Ordinal = 0
            });
            Assert.AreEqual(2, attributionSchema.AttributeDefinitions.Count);
            Assert.AreEqual(2, attributionSchema.AttributeGroups.Count);
            Assert.That(attributionSchema.AttributeGroups, Has.Some.EqualTo(FixedGroupDefinitions.GeneralGroup));
            Assert.That(attributionSchema.AttributeGroups, Has.Some.EqualTo(FixedGroupDefinitions.FileProperties));
        }

        [Test]
        public void Adding_AttributeDefinitions_WithoutIds_AddsToGroupsOnSchema()
        {
            // Arrange
            var attributionSchema = HiveModelCreationHelper.CreateEntitySchema("test", "Test", new AttributeDefinition[] { });
            Assert.AreEqual(0, attributionSchema.AttributeGroups.Count);
            var nameNameType = _attributeTypeRegistry.GetAttributeType(NodeNameAttributeType.AliasValue);
            var fileUploadType = _attributeTypeRegistry.GetAttributeType(FileUploadAttributeType.AliasValue);

            // Act & Assert
            attributionSchema.AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
            {
                //Id = new HiveId("mi-name".EncodeAsGuid()),
                AttributeType = nameNameType,
                AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                Ordinal = 0
            });
            Assert.AreEqual(1, attributionSchema.AttributeDefinitions.Count);
            Assert.AreEqual(1, attributionSchema.AttributeGroups.Count);
            CollectionAssert.Contains(attributionSchema.AttributeGroups, FixedGroupDefinitions.GeneralGroup);

            attributionSchema.AttributeDefinitions.Add(new AttributeDefinition("uploadedFile", "Uploaded File")
            {
                //Id = new HiveId("mi-upload".EncodeAsGuid()),
                AttributeType = fileUploadType,
                AttributeGroup = FixedGroupDefinitions.FileProperties,
                Ordinal = 0
            });
            Assert.AreEqual(2, attributionSchema.AttributeDefinitions.Count);
            Assert.AreEqual(2, attributionSchema.AttributeGroups.Count);
            CollectionAssert.Contains(attributionSchema.AttributeGroups, FixedGroupDefinitions.GeneralGroup);
            CollectionAssert.Contains(attributionSchema.AttributeGroups, FixedGroupDefinitions.FileProperties);

        }

        [Test]
        public void Get_Set_String_Config_Xml_Property()
        {
            //Arrange

            var attributionSchema = HiveModelCreationHelper.CreateEntitySchema("test", "Test", new AttributeDefinition[] { });
            
            //Act

            attributionSchema.SetXmlConfigProperty("test", "hello");

            //Assert

            Assert.AreEqual("hello", attributionSchema.GetXmlConfigProperty("test"));
        }

        [Test]
        public void Get_Set_String_List_Config_Xml_Property()
        {
            //Arrange

            var attributionSchema = HiveModelCreationHelper.CreateEntitySchema("test", "Test", new AttributeDefinition[] { });

            //Act

            attributionSchema.SetXmlConfigProperty("test", new[] { "hello", "world" });

            //Assert

            var list = attributionSchema.GetXmlPropertyAsList("test");
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual("hello", list.First());
            Assert.AreEqual("world", list.Last());
        }

        [Test]
        public void Get_Set_Guid_List_Config_Xml_Property()
        {
            //Arrange

            var attributionSchema = HiveModelCreationHelper.CreateEntitySchema("test", "Test", new AttributeDefinition[] { });
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            //Act

            attributionSchema.SetXmlConfigProperty("test", new[] { guid1, guid2 });

            //Assert

            var list = attributionSchema.GetXmlPropertyAsList<Guid>("test");
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(guid1, list.First());
            Assert.AreEqual(guid2, list.Last());
        }

        [Test]
        public void Get_Set_Int_List_Config_Xml_Property()
        {
            //Arrange

            var attributionSchema = HiveModelCreationHelper.CreateEntitySchema("test", "Test", new AttributeDefinition[] { });
            var int1 = 1234;
            var int2 = 4321;

            //Act

            attributionSchema.SetXmlConfigProperty("test", new[] { int1, int2 });

            //Assert

            var list = attributionSchema.GetXmlPropertyAsList<int>("test");
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(int1, list.First());
            Assert.AreEqual(int2, list.Last());
        }

        [Test]
        public void Get_Set_HiveId_List_Config_Xml_Property()
        {
            //Arrange

            var attributionSchema = HiveModelCreationHelper.CreateEntitySchema("test", "Test", new AttributeDefinition[] { });
            var id1 = new HiveId(1234);
            var id2 = new HiveId(4321); 

            //Act

            attributionSchema.SetXmlConfigProperty("test", new[] { id1, id2 });

            //Assert

            var list = attributionSchema.GetXmlPropertyAsList<HiveId>("test");
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(id1, list.First());
            Assert.AreEqual(id2, list.Last());
        }

        public override void TestSetup()
        {
            return;
        }

        public override void TestTearDown()
        {
            return;
        }
    }
}