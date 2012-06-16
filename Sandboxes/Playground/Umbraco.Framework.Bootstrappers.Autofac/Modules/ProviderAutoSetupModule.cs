using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Umbraco.Foundation;
using Module = Autofac.Module;

namespace Umbraco.Framework.Bootstrappers.Autofac.Modules
{
	public class ProviderAutoSetupModule : Module
	{
		Assembly _assembly;
		private string _providerKey;

		public ProviderAutoSetupModule(Assembly assembly, string providerKey)
		{
			_providerKey = providerKey;
			_assembly = assembly;
		}

		protected override void Load(ContainerBuilder builder)
		{
			foreach (var component in _assembly.GetTypes())
			{
				Type componentCopyForDelegate = component;

				//TODO: Here we're calling all the matching setup modules, but the provider may
				//request multiple instances of the same persistence repo so we need to make
				//sure we don't invoke the modules several times
				component
					.GetCustomAttributes(typeof(ProviderSetupModuleAttribute), false)
					.OfType<ProviderSetupModuleAttribute>()
					.ToList()
					.ForEach(x =>
										 {
											 var module = Activator.CreateInstance(componentCopyForDelegate) as Module;
											 if (module != null)
											 {
											 	var providerSetupModule = module as IProviderSetupModule;
											 	if (providerSetupModule != null)
											 		providerSetupModule.Alias = _providerKey;
											 	builder.RegisterModule(module);
											 }
										 });
			}
		}
	}
}