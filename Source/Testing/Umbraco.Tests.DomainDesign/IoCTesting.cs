using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Foundation;
using Umbraco.Framework.Bootstrappers.Autofac;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Tests.DomainDesign
{
  [TestClass]
  public class IoCTesting
  {
    private static TestContext _testContext;

    [ClassInitialize()]
    public static void MyClassInitialize(TestContext testContext)
    {
      _testContext = testContext;
      AutoFacResolver.InitialiseFoundation();
    }

    [TestMethod]
    public void CanGetPersistenceRepositoryByName()
    {
 
      //TODO: Move this and other tests to a test class focussing on the Provider-loading module
      var instance = DependencyResolver.Current.Resolve<IPersistenceManager>("in-memory-01");
      Assert.IsInstanceOfType(instance, typeof(IPersistenceManager));
      Assert.IsNotNull(instance);
    }

    [TestMethod]
    public void PersistenceProviderIsSingleInstance()
    {
      var instance1 = DependencyResolver.Current.Resolve<IPersistenceManager>("in-memory-01");
      var instance2 = DependencyResolver.Current.Resolve<IPersistenceManager>("in-memory-01");

      Assert.AreSame(instance1, instance2);
      Assert.AreSame(instance1.Reader, instance2.Reader);
      //TODO: CollectionAssert.AreEquivalent(instance1.ReadWriter, instance2.ReadWriter);
    }

    [TestMethod]
    public void CanGetPersistenceProviderAlias()
    {
      var instance = DependencyResolver.Current.Resolve<IPersistenceManager>("in-memory-01");
      Assert.IsNotNull(instance);
      Assert.AreNotEqual(string.Empty, instance.Alias);
      _testContext.WriteLine("Provider alias was '{0}'", instance.Alias);
    }

    [TestMethod]
    public void CanGetPersistenceProviderReaderAlias()
    {
      var instance = DependencyResolver.Current.Resolve<IPersistenceManager>("in-memory-01");
      Assert.IsNotNull(instance.Reader, "Reader is null - check provider config");
      Assert.IsFalse(string.IsNullOrWhiteSpace(instance.Reader.RepositoryKey));
      _testContext.WriteLine("Provider alias was '{0}'", instance.Reader.RepositoryKey);
    }

    [TestMethod]
    public void CanGetPersistenceProviderReaderWriterAlias()
    {
      var instance = DependencyResolver.Current.Resolve<IPersistenceManager>("in-memory-01");
      Assert.IsNotNull(instance.ReadWriter, "ReadWriter is null - check provider config");
      Assert.IsFalse(string.IsNullOrWhiteSpace(instance.ReadWriter.RepositoryKey));
      _testContext.WriteLine("Provider alias was '{0}'", instance.ReadWriter.RepositoryKey);
    }
  }
}