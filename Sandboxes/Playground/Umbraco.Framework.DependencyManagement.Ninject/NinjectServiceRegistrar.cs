using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Syntax;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
	public class NinjectServiceRegistrar<TContract, TImplementation> : IServiceRegistrar<TContract, TImplementation>
		where TImplementation : TContract
	{
		private readonly IKernel _kernel;

		public NinjectServiceRegistrar(IKernel kernel)
		{
			_kernel = kernel;
		}

		/// <summary>Registers a service with the container.</summary>
		/// <typeparam name="TContract">The <typeparamref name="TContract" /> type used for the contract.</typeparam>
		/// <typeparam name="TImplementation">The <typeparamref name="TImplementation" /> type used for the implementation.</typeparam>
		public IServiceRegistrarModifier<TContract, TImplementation> Register()
		{
			IBindingWhenInNamedWithOrOnSyntax<TContract> bindingResult = _kernel.Bind<TContract>().To<TImplementation>();
			return new NinjectServiceRegistrarModifier<TContract, TImplementation>(this, bindingResult);
		}

		/// <summary>Registers a service with the container.</summary>
		/// <typeparam name="TContract">The <typeparamref name="TContract" /> type used for the contract.</typeparam>
		/// <typeparam name="TImplementation">The <typeparamref name="TImplementation" /> type used for the implementation.</typeparam>
		/// <param name="name">The name of the service.</param>
		public IServiceRegistrarModifier<TContract, TImplementation> Register(string name)
		{
			IBindingWhenInNamedWithOrOnSyntax<TContract> bindingResult = _kernel.Bind<TContract>().To<TImplementation>();
			bindingResult.Named(name);
			return new NinjectServiceRegistrarModifier<TContract, TImplementation>(this, bindingResult);
		}

		/// <summary>Registers a service with the container providing a delegate for instantiation.</summary>
		/// <typeparam name="TContract">The <typeparamref name="TContract" /> type used for the contract.</typeparam>
		/// <typeparam name="TImplementation">The <typeparamref name="TImplementation" /> type used for the implementation.</typeparam>
		/// <param name="delegate">The delegate used for instantiating services.</param>
		/// <returns>.</returns>
		public IInstanceRegistrarModifier<TContract> RegisterFactory(Func<IResolutionContext, TImplementation> @delegate)
		{
			IBindingWhenInNamedWithOrOnSyntax<TContract> bindingResult = _kernel.Bind<TContract>().ToMethod(context => @delegate.Invoke(new NinjectResolutionContext(_kernel)));
			return new NinjectServiceRegistrarModifier<TContract, TImplementation>(this, bindingResult);
		}

		public IInstanceRegistrarModifier<TContract> RegisterFactory(string serviceName, Func<IResolutionContext, TImplementation> @delegate)
		{
			IBindingWhenInNamedWithOrOnSyntax<TContract> bindingResult = _kernel.Bind<TContract>().ToMethod(context => @delegate.Invoke(new NinjectResolutionContext(_kernel)));
			bindingResult.Named(serviceName);
			return new NinjectServiceRegistrarModifier<TContract, TImplementation>(this, bindingResult);
		}
	}
}
