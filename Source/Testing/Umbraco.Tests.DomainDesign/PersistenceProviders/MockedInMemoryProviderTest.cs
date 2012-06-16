using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Foundation;
using Umbraco.Framework.Bootstrappers.Autofac;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.ModelTest;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders
{


    /// <summary>
    ///This is a test class for MockedInMemoryProviderTest and is intended
    ///to contain all MockedInMemoryProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MockedInMemoryProviderTest
    {
        private static IPersistenceManager _manager;

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            AutoFacResolver.InitialiseFoundation();
            _manager = DependencyResolver.Current.Resolve<IPersistenceManager>("in-memory-01");
        }

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
        ///A test for ReadWriter
        ///</summary>
        [TestMethod()]
        public void ReadWriterTest()
        {
            Assert.IsNotNull(_manager.ReadWriter);
        }

        [TestMethod()]
        public void ReadWriterAddTest()
        {
            VersionedPersistenceEntity persistedEntity = TestHelper.CreateVersionedPersistenceEntity();
            persistedEntity.Id = Guid.NewGuid();
            int previousCount = _manager.ReadWriter.GetCount<VersionedPersistenceEntity>();
            _manager.ReadWriter.Add(persistedEntity);
            Assert.IsTrue(_manager.ReadWriter.GetCount<VersionedPersistenceEntity>() == previousCount + 1);
        }

        [TestMethod]
        public void ReadWriterGetTest()
        {
            VersionedPersistenceEntity persistedEntity = TestHelper.CreateVersionedPersistenceEntity();
            persistedEntity.Id = Guid.NewGuid();
            _manager.ReadWriter.Add(persistedEntity);
            VersionedPersistenceEntity result = _manager.ReadWriter.GetVersionedEntity(persistedEntity.Id);
            Assert.AreEqual(persistedEntity, result);
        }
    }
}
