using System.Diagnostics.Contracts;
using Autofac;

namespace Sandbox.Hive.BootStrappers.AutoFac.Modules
{
  public class HiveServicesModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      Contract.Assert(builder != null, "Builder container is null");


    }
  }
}
