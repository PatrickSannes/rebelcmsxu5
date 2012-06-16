using Prototyping.Umbraco.Framework.Playground;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Umbraco.Framework;

namespace Prototyping.Umbraco.Framework.Tests
{
    
    
    /// <summary>
    ///This is a test class for FakeControllerTest and is intended
    ///to contain all FakeControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FakeControllerTest
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




        /// <summary>
        ///A test for IndexAction
        ///</summary>
        [TestMethod()]
        public void IndexActionTest()
        {
            FakeController target = new FakeController(); // TODO: Initialize to an appropriate value

            IMappedIdentifier entityId = new GuidIdentifier(); // TODO: Initialize to an appropriate value
            var standardProvider = new StandardProviderManifest();
            standardProvider.MappingAlias = "prototype";
            entityId.MappedProvider = standardProvider;

            target.IndexAction(entityId);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
