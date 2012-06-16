using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.Hive.BootStrappers.AutoFac;
using Sandbox.Hive.Domain.HiveDomain;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.Hive.Foundation;
using Sandbox.Hive.Foundation.Configuration;
using Sandbox.PersistenceProviders.MockedInMemory;

namespace Sandbox.Hive.Tests
{
  [TestClass]
  public class ConfigurationTests
  {
    [ClassInitialize()]
    public static void MyClassInitialize(TestContext testContext)
    {
      AutoFacResolver.InitialiseFoundation();
    }

    [TestMethod]
    public void CanGetFoundationSection()
    {
      var instance = DependencyResolver.Current.Resolve<IFoundationConfigurationSection>();
      Assert.IsInstanceOfType(instance, typeof(FoundationConfigurationSection));
      Assert.IsNotNull(instance);
    }

    [TestMethod]
    public void CanGetPersistenceProviderElements()
    {
      var instance = DependencyResolver.Current.Resolve<IFoundationConfigurationSection>();
      var elementCollection = instance.PersistenceProviders;


      Assert.IsNotNull(elementCollection);
      Assert.IsFalse(elementCollection.Count == 0);

      CollectionAssert.AllItemsAreNotNull(elementCollection);
      CollectionAssert.AllItemsAreUnique(elementCollection);
      CollectionAssert.AllItemsAreInstancesOfType(elementCollection, typeof(PersistenceProviderElement));
    }

    [TestMethod]
    public void CanGetPersistenceProviderReaderElements()
    {
      var instance = DependencyResolver.Current.Resolve<IFoundationConfigurationSection>();
      var elementCollection = instance.PersistenceProviders;

      foreach (PersistenceProviderElement providerElement in elementCollection)
      {
        Assert.IsNotNull(providerElement.Reader);
        Assert.IsFalse(string.IsNullOrWhiteSpace(providerElement.Reader.Key));
      }
    }

    [TestMethod]
    public void CanGetPersistenceProviderReaderAutoParent()
    {
      var instance = DependencyResolver.Current.Resolve<IFoundationConfigurationSection>();
      var elementCollection = instance.PersistenceProviders;

      foreach (PersistenceProviderElement providerElement in elementCollection)
      {
        Assert.IsNotNull(providerElement.Reader.Parent);
        Assert.AreSame(providerElement, providerElement.Reader.Parent);
      }
    }

    [TestMethod]
    public void CanGetPersistenceProviderReadWritersAutoParent()
    {
      var instance = DependencyResolver.Current.Resolve<IFoundationConfigurationSection>();
      var elementCollection = instance.PersistenceProviders;

      foreach (PersistenceProviderElement providerElement in elementCollection)
      {
        Assert.IsNotNull(providerElement.ReadWriters.Parent);
        Assert.AreSame(providerElement, providerElement.ReadWriters.Parent);

        foreach (PersistenceReadWriterElement readWriterElement in providerElement.ReadWriters)
        {
          Assert.IsNotNull(readWriterElement.Parent);
          Assert.AreSame(providerElement, readWriterElement.Parent);
        }
      }
    }

    [TestMethod]
    public void CanGetPersistenceProviderReadWriterElements()
    {
      var instance = DependencyResolver.Current.Resolve<IFoundationConfigurationSection>();
      var elementCollection = instance.PersistenceProviders;

      foreach (PersistenceProviderElement providerElement in elementCollection)
      {
        foreach (PersistenceReadWriterElement readWriterElement in providerElement.ReadWriters)
        {
          Assert.IsNotNull(readWriterElement.Key);
          Assert.IsFalse(string.IsNullOrWhiteSpace(readWriterElement.Key));
        }
      }
    }

    //[TestMethod]
    public void TempGenerateConfigXmlFromTypes()
    {
      var instance = DependencyResolver.Current.Resolve<IFoundationConfigurationSection>();

      var providers = instance.PersistenceProviders;

      var writeable = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
      instance = writeable.GetSection("umbraco.foundation") as IFoundationConfigurationSection;

      providers = instance.PersistenceProviders;

      var provider1 = new PersistenceProviderElement();
      provider1.Key = "in-memory-01";
      provider1.Type =
        "Sandbox.PersistenceProviders.InMemoryMocked.Provider, Sandbox.PersistenceProviders.InMemoryMocked";

      var reader1 = new PersistenceReaderElement();
      reader1.Key = "reader";
      reader1.Type =
        "Sandbox.PersistenceProviders.MockedInMemory.ReadWriteRepository, Sandbox.PersistenceProviders.MockedInMemory";

      var writer1 = new PersistenceReadWriterElement();
      writer1.Key = "writer";
      writer1.Type =
        "Sandbox.PersistenceProviders.MockedInMemory.ReadWriteRepository, Sandbox.PersistenceProviders.MockedInMemory";

      var writers = new PersistenceReadWriterElementCollection();

      provider1.Reader = reader1;

      provider1.ReadWriters.Add(writer1);

      providers.Add(provider1);

      var filename = System.IO.Path.Combine(Path.GetDirectoryName(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "test.config");

      System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None).
        SaveAs(filename);

      var a = ((FoundationConfigurationSection) instance).GetXml();
      
      Assert.Inconclusive(a);
    }

  }

}