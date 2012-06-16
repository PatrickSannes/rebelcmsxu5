using System.Diagnostics.Contracts;
using Autofac;
using Sandbox.Hive.Foundation;

namespace Sandbox.PersistenceProviders.MockedInMemory.Modules
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

      // Configure type injection for this provider's implementation of the main interfaces 
    }
  }
}