using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Sandbox.Hive.BootStrappers.AutoFac;
using Sandbox.Hive.Domain.HiveDomain;
using Sandbox.Hive.Domain.ServiceRepositoryDomain;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.Hive.Foundation;

namespace Sandbox.Hive.Tests
{
    
    
    /// <summary>
    ///This is a test class for HiveRepositoryTest and is intended
    ///to contain all HiveRepositoryTest Unit Tests
    ///</summary>
  [TestClass()]
  public class HiveRepositoryTest
  {
    [ClassInitialize()]
    public static void MyClassInitialize(TestContext testContext)
    {
      AutoFacResolver.InitialiseFoundation();
    }

    [TestMethod()]
    public void ResolveHiveRepositoryTest()
    {
      HiveRepository target = DependencyResolver.Current.Resolve<HiveRepository>();
      Assert.IsNotNull(target);
      Assert.IsInstanceOfType(target, typeof(HiveRepository));
    }

    /// <summary>
    ///A test for Get
    ///</summary>
    [TestMethod()]
    public void GetTest()
    {
      HiveRepository target = DependencyResolver.Current.Resolve<HiveRepository>();

      string persistenceProviderKey = "in-memory-01";

      MappedIdentifier identifier = new MappedIdentifier() {ProviderKey = persistenceProviderKey, Value = 100};

      PersistedEntity persistedEntity = TestHelper.CreatePersistedEntity(persistenceProviderKey);
      persistedEntity.Key = "100";

      var mockedInMemoryRepo = DependencyResolver.Current.TryResolve<IPersistenceProvider>(identifier.ProviderKey);
      mockedInMemoryRepo.Value.Reader.Add(persistedEntity);

      int traversalDepth = 0;

      var actual = target.Get(identifier, traversalDepth);

      Assert.IsNotNull(actual);
      Assert.AreEqual(persistedEntity.Value, actual.Value);
      Assert.AreEqual(identifier, actual.Id);
    }
  }
}
