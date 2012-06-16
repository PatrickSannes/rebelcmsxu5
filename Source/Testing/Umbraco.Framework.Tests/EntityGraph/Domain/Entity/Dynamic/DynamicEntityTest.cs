using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain;
using Umbraco.Framework.EntityGraph.Domain.Brokers;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;
using Umbraco.Framework.EntityGraph.Domain.EntityAdaptors;
using Umbraco.TestExtensions;

namespace Umbraco.Framework.Tests.EntityGraph.Domain.Entity.Dynamic
{
    [TestClass]
    public class DynamicEntityTest
    {
        class SimpleDocType : IEntityTypeDefinition
        {
            public SimpleDocType()
            {
                var root = new TypedEntityVertex().SetupRoot();
                this.SetupTypeDefinition("SimpleDocType", "Simple DocType", root);

                // Create a new tab group
                var textGroup = EntityFactory.Create<AttributeGroupDefinition>("textdata", "Text Data");
                this.AttributeSchema.AttributeGroupDefinitions.Add(textGroup);

                // Create a data type
                var textInputField = EntityFactory.Create<AttributeTypeDefinition>("textInputField", "Text Input Field");

                // Create a serialization type for persisting this to the repository
                var stringSerializer = EntityFactory.Create<StringSerializationType>("string", "String");
                stringSerializer.DataSerializationType = DataSerializationTypes.String;

                // Create a new property with that data type in our tab group
                var bodyText = EntityFactory.CreateAttributeIn<AttributeDefinition>("bodyText", "Body Text", textInputField,
                                                                                    stringSerializer, textGroup);

                // Specify that tis type is allowed under itself
                this.GraphSchema.PermittedDescendentTypes.Add(this);
            }

            #region IEntityTypeDefinition Members

            public IEntityGraphSchema GraphSchema
            {
                get;
                set;
            }

            public Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData.IAttributionSchemaDefinition AttributeSchema
            {
                get;
                set;
            }

            #endregion

            #region IReferenceByAlias Members

            public string Alias
            {
                get;
                set;
            }

            public LocalizedString Name
            {
                get;
                set;
            }

            #endregion

            #region IEntityVertex Members

            public Umbraco.Framework.EntityGraph.Domain.Entity.Graph.IEntityGraph DescendentEntities
            {
                get;
                set;
            }

            public Umbraco.Framework.EntityGraph.Domain.Entity.Graph.IEntityVertex ParentEntity
            {
                get;
                set;
            }

            public Umbraco.Framework.EntityGraph.Domain.Entity.Graph.IEntityVertex RootEntity
            {
                get;
                set;
            }

            #endregion

            #region IEntity Members

            public Umbraco.Framework.EntityGraph.Domain.Entity.IEntityStatus Status
            {
                get;
                set;
            }

            public DateTime UtcCreated
            {
                get;
                set;
            }

            public DateTime UtcModified
            {
                get;
                set;
            }

            public DateTime UtcStatusChanged
            {
                get;
                set;
            }

            public IMappedIdentifier Id
            {
                get;
                set;
            }

            public Umbraco.Framework.EntityGraph.Domain.Versioning.IRevisionData Revision
            {
                get;
                set;
            }

            #endregion

            #region ITracksConcurrency Members

            public Data.Common.IConcurrencyToken ConcurrencyToken
            {
                get;
                set;
            }

            #endregion

            #region IVertex Members

            public bool IsRoot
            {
                get;
                set;
            }

            public int Depth
            {
                get;
                set;
            }

            public bool HasDescendents
            {
                get;
                set;
            }

            public int DescendentsDepth
            {
                get;
                set;
            }

            public Umbraco.Framework.EntityGraph.Domain.Entity.IEntityPath Path
            {
                get;
                set;
            }

            public IMappedIdentifier ParentId
            {
                get;
                set;
            }

            public Umbraco.Framework.EntityGraph.Domain.Entity.Graph.EntityAssociationCollection AssociatedEntities
            {
                get;
                set;
            }

            public dynamic ParentDynamic
            {
                get;
                set;
            }

            #endregion
        }

        class SimpleDoc : Content
        {
            public SimpleDoc()
            {
                this.Setup(new SimpleDocType());
            }
        }

        [TestMethod]
        [Owner(TestOwner.Framework)]
        public void DynamicEntityTest_Property_Assignment_Via_Property_Alias()
        {
            //Arrange
            var entity = new SimpleDoc();

            var dynamicEntity = entity.AsDynamic();
            //Act
            dynamicEntity.bodyText = "Hello World";
            
            //Assert
            Assert.IsNotNull(dynamicEntity.bodyText);
            Assert.IsNotNull(entity.Attributes["bodyText"].Value);
        }

        [TestMethod]
        //TODO: What exception should be thrown here?
        [ExpectedException(typeof(Exception))]
        [Owner(TestOwner.Framework)]
        public void DynamicEntityTest_Missing_Property_Throws_Exception()
        {
            //Arrange
            var entity = new SimpleDoc();

            var dynamicEntity = entity.AsDynamic();
            //Act
            var tmp = dynamicEntity.thisDoesntExist;
            //Assert
            Assert.Fail("Exception should have been thrown");
        }

        [TestMethod]
        [Owner(TestOwner.Framework)]
        public void DynamicEntityTest_Attributes_Passed_To_Dynamic()
        {
            //Arrange
            var entity = new SimpleDoc();

            var dynamicEntity = entity.AsDynamic();
            //Act

            //Assert
            Assert.AreSame(entity.Attributes, dynamicEntity.Attributes);
        }

        [TestMethod]
        [Owner(TestOwner.Framework)]
        public void DynamicEntityTest_DynamicEntity_Is_Also_An_IEntity()
        {
            //Arrange
            var entity = new SimpleDoc();

            var dynamicEntity = entity.AsDynamic();

            //Act

            //Assert
            Assert.IsInstanceOfType(dynamicEntity, typeof(IEntity));
        }

        [TestMethod]
        [Owner(TestOwner.Framework)]
        public void DynamicEntityTest_Tab_Accessable_By_Name()
        {
            //Arrange
            var entity = new SimpleDoc();

            var dynamicEntity = entity.AsDynamic();

            //Act

            //Assert
            Assert.IsNotNull(dynamicEntity.textdata);
            Assert.IsTrue(dynamicEntity.textdata.AttributeDefinitions.Count > 0);
            Assert.IsNotNull(dynamicEntity.textdata.bodyText);
        }

        [TestMethod]
        [Owner(TestOwner.Framework)]
        public void DynamicEntityTest_Hierarchical_Entites_Accessible_By_Convention()
        {
            //Arrange
            var entity = new SimpleDoc();
            entity.DescendentEntities.Add(entity.Id, entity);

            var dynamicEntity = entity.AsDynamic();
            //Act

            //Assert
            Assert.IsNotNull(dynamicEntity.SimpleDocs);
            Assert.AreEqual(0, dynamicEntity.SimepleDocs.Count());
            Assert.AreSame(entity, dynamicEntity.SimpleDocs.First());
        }
    }
}
