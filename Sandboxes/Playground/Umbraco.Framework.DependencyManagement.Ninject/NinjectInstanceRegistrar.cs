using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
	public class NinjectInstanceRegistrar<TContract> : IInstanceRegistrar<TContract>
	{
		private readonly IKernel _kernel;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public NinjectInstanceRegistrar(IKernel kernel)
		{
			_kernel = kernel;
		}

		public IInstanceRegistrarModifier<TContract> Register<TImplementation>(TImplementation instance) where TImplementation : class, TContract
		{
			var bindingResult = _kernel.Bind<TContract>().ToConstant(instance);
			return new NinjectInstanceRegistrarModifier<TContract>(bindingResult);
		}

		public IInstanceRegistrarModifier<TContract> Register<TImplementation>(TImplementation instance, string name) where TImplementation : class, TContract
		{
			var bindingResult = _kernel.Bind<TContract>().ToConstant(instance);
			bindingResult.Named(name);
			return new NinjectInstanceRegistrarModifier<TContract>(bindingResult);
		}
	}
}
