using System.Diagnostics.Contracts;
using Autofac;
using Umbraco.Framework.Persistence;

namespace Umbraco.Framework.Bootstrappers.Autofac.Modules
{
  public class PersistenceProviderModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      Contract.Assert(builder != null, "Builder container is null");

      // Register a singleton PersistenceProviderCollection
      builder.RegisterType<PersistenceProviderCollection>().As<PersistenceProviderCollection>().SingleInstance();
    }
  }
}