using System;
using Ninject;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
	public class NinjectRuntimeTypeRegistrar<TContract> : IRuntimeTypeRegistrar<TContract>
	{
		private readonly Type _implementation;
		private readonly IKernel _kernel;

		public NinjectRuntimeTypeRegistrar(Type implementation, IKernel kernel)
		{
			_kernel = kernel;
			_implementation = implementation;
		}

		public IRuntimeTypeRegistrarModifier<TContract> Register()
		{
			var bindingResult = _kernel.Bind<TContract>().To(_implementation);
			return new NinjectRuntimeTypeRegistrarModifier<TContract>(bindingResult);
		}

		public IRuntimeTypeRegistrarModifier<TContract> RegisterNamed(string name)
		{
			var bindingResult = _kernel.Bind<TContract>().To(_implementation);
			bindingResult.Named(name);
			return new NinjectRuntimeTypeRegistrarModifier<TContract>(bindingResult);
		}
	}
}