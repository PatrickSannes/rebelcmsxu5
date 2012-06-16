using Sandbox.Hive.BootStrappers.AutoFac;
using Sandbox.Hive.Foundation;
using Sandbox.PersistenceProviders.MockedInMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.Hive.Tests
{
    
    
    /// <summary>
    ///This is a test class for MockedInMemoryProviderTest and is intended
    ///to contain all MockedInMemoryProviderTest Unit Tests
    ///</summary>
  [TestClass()]
  public class MockedInMemoryProviderTest
  {
      private static IPersistenceProvider _provider;

      #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    [ClassInitialize()]
    public static void MyClassInitialize(TestContext testContext)
    {
      AutoFacResolver.InitialiseFoundation();
      _provider = DependencyResolver.Current.Resolve<IPersistenceProvider>("in-memory-01");
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
      Assert.IsNotNull(_provider.ReadWriter);
    }

     [TestMethod()]
    public void ReadWriterAddTest()
     {
       PersistedEntity persistedEntity = TestHelper.CreatePersistedEntity("test1");

       int previousCount = _provider.ReadWriter.Count;
       _provider.ReadWriter.Add(persistedEntity);
       Assert.IsTrue(_provider.ReadWriter.Count == previousCount + 1);
     }

    [TestMethod]
     public void ReadWriterGetTest()
     {
       PersistedEntity persistedEntity = TestHelper.CreatePersistedEntity("test1");
       _provider.ReadWriter.Add(persistedEntity);
       PersistedEntity result = _provider.ReadWriter.Get(persistedEntity.Key);
       Assert.AreEqual(persistedEntity, result);
     }
  }
}
