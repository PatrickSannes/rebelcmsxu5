using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prototyping.Umbraco.Framework.Playground;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;

namespace Prototyping.Umbraco.Framework.Tests
{
    
    
    /// <summary>
    ///This is a test class for ArticleDocTypeTest and is intended
    ///to contain all ArticleDocTypeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ArticleDocTypeTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod]
        public void ArticleDocTypeTest_One_Property_On_DocType()
        {
            //Arrange
            var dt = new ArticleDocType();
            //Act

            //Assert
            var properties = from tab in dt.AttributeSchema.AttributeGroupDefinitions
                             from p in tab.AttributeDefinitions
                             select p;

            Assert.AreEqual(1, properties.Count());
        }

        [TestMethod]
        public void ArticleDocTypeTest_One_Tab_On_DocType()
        {
            //Arrange
            var dt = new ArticleDocType();
            //Act

            //Assert
            Assert.AreEqual(1, dt.AttributeSchema.AttributeGroupDefinitions.Count);
        }

        [TestMethod]
        public void ArticleDocTypeTest_ArticleDocType_Is_Allowed_Child()
        {
            //Arrange
            var dt = new ArticleDocType();
            //Act

            //Assert
            Assert.IsTrue(dt.GraphSchema.PermittedDescendentTypes.IsRegistered<IEntityTypeDefinition, ArticleDocType>());
            Assert.IsTrue(dt.GraphSchema.PermittedDescendentTypes.IsRegistered(typeof(ArticleDocType)));
            //TODO: You can do this with Contains and the ID of the docType but that doesn't seem right, you should be able to do it off types IMO
        }

        [TestMethod]
        public void ArticleDocTypeTest_NewsDocType_Is_Now_Allowed_Child()
        {
            //Arrange
            var dt = new ArticleDocType();
            //Act

            //Assert
            Assert.IsFalse(dt.GraphSchema.PermittedDescendentTypes.IsRegistered<IEntityTypeDefinition, NewsDocType>());
            Assert.IsFalse(dt.GraphSchema.PermittedDescendentTypes.IsRegistered(typeof(NewsDocType)));
            //TODO: You can do this with Contains and the ID of the docType but that doesn't seem right, you should be able to do it off types IMO
        }
    }
}
