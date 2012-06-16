using System.Diagnostics.Contracts;
using Autofac;
using NHibernate;
using Sandbox.Hive.Domain.DataManagement;
using Sandbox.Hive.Foundation;
using Sandbox.PersistenceProviders.NHibernate.DataManagement;
using ITransaction = Sandbox.Hive.Domain.DataManagement.ITransaction;

namespace Sandbox.PersistenceProviders.NHibernate.Modules
{
  [ProviderSetupModule]
  public class HiveProviderSetupModule : Module, IProviderSetupModule
  {
    public HiveProviderSetupModule()
    {
    }

    public HiveProviderSetupModule(string providerKey)
    {
      Alias = providerKey;
    }

    #region IProviderSetupModule Members

    public string Alias { get; set; }

    #endregion

    protected override void Load(ContainerBuilder builder)
    {
      Contract.Assert(builder != null, "Builder container is null");

      // Cconfigure NHibernate (local to this provider)
      var nHibernateComponentModule = new NHibernateComponentModule();
      builder.RegisterModule(nHibernateComponentModule);

      // Configure type injection for this provider's implementation of the main interfaces (i.e. mappings
      // from the abstractions to NHibernate)
      builder.Register(x => new DataContextFactory(x.Resolve<ISessionFactory>()))
        .Named<IDataContextFactory>(Alias)
        .SingleInstance();

      builder.RegisterType<DataContext>().Named<IDataContext>(Alias);
      builder.RegisterType<Transaction>().Named<ITransaction>(Alias);

      builder.Register(x => x.ResolveNamed<IDataContextFactory>(Alias).CreateDataContext()).As<IDataContext>().
        InstancePerLifetimeScope();
      builder.Register(x => new UnitOfWork(x.ResolveNamed<IDataContextFactory>(Alias))).As<IUnitOfWork>();
    }
  }
}