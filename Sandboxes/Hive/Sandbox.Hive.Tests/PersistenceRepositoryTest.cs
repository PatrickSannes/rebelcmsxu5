using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.PersistenceProviders.MockedInMemory;

namespace Sandbox.Hive.Tests
{
  /// <summary>
  ///This is a test class for PersistenceRepositoryTest and is intended
  ///to contain all PersistenceRepositoryTest Unit Tests
  ///</summary>
  [TestClass]
  public class PersistenceRepositoryTest
  {
    private static IPersistenceRepository _repository;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext { get; set; }

    #region Additional test attributes

    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    [ClassInitialize]
    public static void MyClassInitialize(TestContext testContext)
    {
      _repository = new ReadWriteRepository("read-write");
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
    ///A test for Get
    ///</summary>
    [TestMethod]
    [DeploymentItem("Sandbox.Hive.BootStrappers.AutoFac.dll")]
    public void GetTest()
    {
      PersistedEntity persistedEntity = TestHelper.CreatePersistedEntity("test1");
      _repository.Add(persistedEntity);
      PersistedEntity result = _repository.Get(persistedEntity.Key);
      Assert.AreEqual(persistedEntity, result);
    }

    /// <summary>
    ///A test for Add
    ///</summary>
    [TestMethod]
    [DeploymentItem("Sandbox.Hive.BootStrappers.AutoFac.dll")]
    public void AddTest()
    {
      PersistedEntity persistedEntity = TestHelper.CreatePersistedEntity("test1");

      int previousCount = _repository.Count;
      _repository.Add(persistedEntity);
      Assert.IsTrue(_repository.Count == previousCount + 1);
    }

    /// <summary>
    ///A test for Add
    ///</summary>
    [TestMethod]
    [DeploymentItem("Sandbox.Hive.BootStrappers.AutoFac.dll")]
    public void AddMultipleTest()
    {
      int previousCount = _repository.Count;
      for (int i = 0; i < 100; i++)
      {
        PersistedEntity persistedEntity = TestHelper.CreatePersistedEntity("test1");

        _repository.Add(persistedEntity);
      }

      Assert.IsTrue(_repository.Count == previousCount + 100);
    }
  }
}