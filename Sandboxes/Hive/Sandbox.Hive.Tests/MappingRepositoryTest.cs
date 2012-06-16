using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.Hive.Domain.ServiceRepositoryDomain;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.MappingModel;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.PersistenceProviders.MockedInMemory;

namespace Sandbox.Hive.Tests
{
  /// <summary>
  ///This is a test class for MappingRepositoryTest and is intended
  ///to contain all MappingRepositoryTest Unit Tests
  ///</summary>
  [TestClass]
  public class MappingRepositoryTest
  {
    private static MappingRepository _mappingRepository;

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
    //[ClassInitialize]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //  _mappingRepository = new MappingRepository();
    //}

    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test
    [TestInitialize()]
    public void MyTestInitialize()
    {
      _mappingRepository = new MappingRepository();
    }
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //

    #endregion

    ///// <summary>
    /////A test for Get
    /////</summary>
    //[TestMethod]
    //public void GetEntityByIdTest()
    //{
    //  string persistenceProviderKey = "testrepo";

    //  var repo = CreatePersistenceRepository(persistenceProviderKey);
    //  var entity = TestHelper.CreatePersistedEntity(persistenceProviderKey);
    //  repo.Add(entity);

    //  MappedIdentifier idForReference = new MappedIdentifier(){ProviderKey = entity.PersistenceProviderKey, Value = entity.Key};

    //  _mappingRepository.RegisterDataStore(repo);

    //  Assert.IsTrue(_mappingRepository.Get(idForReference) != null);
    //}

    //[TestMethod]
    //public void GetEntityByIdGivenMultipleProvidersTest()
    //{
    //  string keyStub = "persistenceProviderKey";
    //  PersistedEntity persistedEntity = null;

    //  for (int i = 0; i < 10; i++)
    //  {
    //    var persistenceProviderKey = keyStub + i.ToString();

    //    PersistenceRepository persistenceRepository = CreatePersistenceRepository(persistenceProviderKey);
    //    _mappingRepository.RegisterDataStore(persistenceRepository);

    //    for (int j = 0; j < 50000; j++)
    //    {
    //      persistedEntity = TestHelper.CreatePersistedEntity(persistenceProviderKey);
    //      persistenceRepository.Add(persistedEntity);
    //    }
    //  }

    //  MappedIdentifier idForReference = new MappedIdentifier() { ProviderKey = persistedEntity.PersistenceProviderKey, Value = persistedEntity.Key };
    //  var mappedEntity = _mappingRepository.Get(idForReference);
    //  Assert.IsTrue(mappedEntity != null);
    //}

    ///// <summary>
    /////A test for RegisterDataStore
    /////</summary>
    //[TestMethod]
    //public void RegisterDataStoreTest()
    //{
    //  PersistenceRepository persistenceRepository = CreatePersistenceRepository();

    //  int previousCount = _mappingRepository.DataStores.Count;
    //  _mappingRepository.RegisterDataStore(persistenceRepository);
    //  Assert.IsTrue(_mappingRepository.DataStores.Count == previousCount + 1);
    //}

    //[TestMethod]
    //public void RegisterMultipleDataStoreTest()
    //{
    //  int previousCount = _mappingRepository.DataStores.Count;
    //  string keyStub = "persistenceProviderKey";
    //  for (int i = 0; i < 100; i++)
    //  {
    //    ReadWriteRepository persistenceRepository = CreatePersistenceRepository(keyStub + i.ToString());
    //    _mappingRepository.RegisterDataStore(persistenceRepository);
    //  }

    //  Assert.IsTrue(_mappingRepository.DataStores.Count == previousCount + 100);
    //}

    private ReadWriteRepository CreatePersistenceRepository()
    {
      return CreatePersistenceRepository(Guid.NewGuid().ToString());
    }

    private ReadWriteRepository CreatePersistenceRepository(string key)
    {
      var persistenceRepository = new ReadWriteRepository("read-write");
      persistenceRepository.RepositoryKey = key;
      return persistenceRepository;
    }
  }
}