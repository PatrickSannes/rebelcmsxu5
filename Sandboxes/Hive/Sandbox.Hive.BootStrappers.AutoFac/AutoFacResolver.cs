using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Sandbox.Hive.BootStrappers.AutoFac.Modules;
using Sandbox.Hive.Foundation;

namespace Sandbox.Hive.BootStrappers.AutoFac
{
  public class AutoFacResolver : IDependencyResolver
  {
    public static void InitialiseFoundation()
    {
      DependencyResolver.Current = new AutoFacResolver();
    }

    private IContainer _container = null;

    public AutoFacResolver()
    {
      _container = GetNewContainer();
    }

    public AutoFacResolver(IContainer container)
    {
      _container = container;
    }

    public T Resolve<T>()
    {
      return _container.Resolve<T>();
    }

    public T Resolve<T>(string name)
    {
      return _container.ResolveNamed<T>(name);
    }

    public object Resolve(Type type)
    {
      return _container.Resolve(type);
    }

    public object Resolve(Type type, string name)
    {
      return _container.ResolveNamed(name, type);
    }

    public ResolutionAttemptTuple<T> TryResolve<T>() where T : class
    {
      try
      {
        T tryResolve = Resolve<T>();
        return new ResolutionAttemptTuple<T>(tryResolve != null, tryResolve);
      }
      catch (Exception)
      {
        return new ResolutionAttemptTuple<T>(false, null);
      }
    }

    public ResolutionAttemptTuple<T> TryResolve<T>(string name) where T : class
    {
      try
      {
        T tryResolve = Resolve<T>(name);
        return new ResolutionAttemptTuple<T>(tryResolve != null, tryResolve);
      }
      catch (Exception)
      {
        return new ResolutionAttemptTuple<T>(false, null);
      }
    }

    public static IContainer GetNewContainer()
    {
      var builder = new ContainerBuilder();

      

      builder.RegisterModule(new LoadConfigurationModule());

      builder.RegisterModule(new ServiceComponentModule());

      //builder.RegisterModule(new PersistenceProviderModule());

      return builder.Build();
    }
  }
}
