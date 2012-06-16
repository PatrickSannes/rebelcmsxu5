using System.Diagnostics.Contracts;
using Autofac;
using Sandbox.Hive.Domain.HiveDomain;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.PersistenceProviders.MockedInMemory;
using Sandbox.PersistenceProviders;

namespace Sandbox.Hive.BootStrappers.AutoFac.Modules
{
  public class PersistenceProviderModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      Contract.Assert(builder != null, "Builder container is null");

      // Register a singleton PersistenceProviderCollection
      // TODO: Pass in configuration for it to know how to load providers (they are loaded below by hand atm)
      builder.RegisterType<PersistenceProviderCollection>().As<PersistenceProviderCollection>().SingleInstance();

      HardcodedProviderRegistration(builder);

      HardcodedPersistenceRepositoryLoading(builder);
    }

    private void HardcodedProviderRegistration(ContainerBuilder builder)
    {
      builder.RegisterModule(new PersistenceProviders.NHibernate.Modules.HiveProviderSetupModule("nhibernate-01"));
      builder.RegisterModule(new PersistenceProviders.MockedInMemory.Modules.HiveProviderSetupModule("in-memory-01"));
    }

    private void HardcodedPersistenceRepositoryLoading(ContainerBuilder builder)
    {
      // TODO: Use Owned<> to have explicit lifetime control for the persistence repositories of a provider

      string inMemoryReadWriteKey = "in-memory-01/read-write";
      builder.RegisterType<ReadWriteRepository>().Named<IPersistenceRepository>(inMemoryReadWriteKey);

      string providerKey = "in-memory-01";
      IPersistenceRepository readWriter;

      builder.Register(c =>
                         {
                           readWriter = c.ResolveNamed<IPersistenceRepository>(inMemoryReadWriteKey);

                           return new Provider(providerKey, readWriter, readWriter);
                         })
        .Named<IPersistenceProvider>(providerKey);







      string sqlRepoKey = "nhibernate-01/read-write";
      builder.RegisterType<PersistenceProviders.NHibernate.ReadWriteRepository>().Named<IPersistenceRepository>(sqlRepoKey);

      string nhProviderKey = "nhibernate-01";
      IPersistenceRepository nhReadWriter;

      builder.Register(c =>
                         {
                           readWriter = c.ResolveNamed<IPersistenceRepository>(sqlRepoKey);

                           return new PersistenceProviders.NHibernate.Provider(nhProviderKey, readWriter, readWriter);
                         })
        .Named<IPersistenceProvider>(nhProviderKey);
    }
  }
}