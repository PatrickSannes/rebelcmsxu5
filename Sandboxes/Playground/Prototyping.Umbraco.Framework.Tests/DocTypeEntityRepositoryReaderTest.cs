using Umbraco.CMS.DataPersistence.Repositories.PackageXml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;
using Umbraco.Framework;
using System.Collections.Generic;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;

namespace Prototyping.Umbraco.Framework.Tests
{
    
    
    /// <summary>
    ///This is a test class for DocTypeEntityRepositoryReaderTest and is intended
    ///to contain all DocTypeEntityRepositoryReaderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DocTypeEntityRepositoryReaderTest
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
        ///A test for Get
        ///</summary>
        //[TestMethod()]
        //public void GetTest()
        //{
        //    XDocument xDocument = null; // TODO: Initialize to an appropriate value
        //    DocTypeEntityRepositoryReader target = new DocTypeEntityRepositoryReader(xDocument); // TODO: Initialize to an appropriate value
        //    IEnumerable<IMappedIdentifier> identifiers = null; // TODO: Initialize to an appropriate value
        //    int traversalDepth = 0; // TODO: Initialize to an appropriate value
        //    EntityGraph<IEntityTypeDefinition> expected = null; // TODO: Initialize to an appropriate value
        //    EntityGraph<IEntityTypeDefinition> actual;
        //    actual = target.Get(identifiers, traversalDepth);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}
    }
}
