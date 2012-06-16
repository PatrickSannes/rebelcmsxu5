using System;

namespace Umbraco.Framework.DependencyManagement
{
	public interface IServiceRegistrar<TContract, TImplementation> where TImplementation : TContract
	{
		/// <summary>Registers a service with the container.</summary>
		/// <typeparam name="TContract">The <typeparamref name="TContract" /> type used for the contract.</typeparam>
		/// <typeparam name="TImplementation">The <typeparamref name="TImplementation" /> type used for the implementation.</typeparam>
		IServiceRegistrarModifier<TContract, TImplementation> Register();

		/// <summary>Registers a service with the container.</summary>
		/// <typeparam name="TContract">The <typeparamref name="TContract" /> type used for the contract.</typeparam>
		/// <typeparam name="TImplementation">The <typeparamref name="TImplementation" /> type used for the implementation.</typeparam>
		/// <param name="name">The name of the service.</param>
		IServiceRegistrarModifier<TContract, TImplementation> Register(string name);

		/// <summary>Registers a service with the container, providing a delegate for instantiation.</summary>
		/// <typeparam name="TContract">The <typeparamref name="TContract" /> type used for the contract.</typeparam>
		/// <typeparam name="TImplementation">The <typeparamref name="TImplementation" /> type used for the implementation.</typeparam>
		/// <param name="delegate">The delegate used for instantiating services.</param>
		/// <returns>.</returns>
		IInstanceRegistrarModifier<TContract> RegisterFactory(
			Func<IResolutionContext, TImplementation> @delegate);

		/// <summary>Registers a named service with the container, providing a delegate for instantiation.</summary>
		/// <param name="serviceName">The name of the service.</param>
		/// <param name="delegate">The delegate used for instantiating services.</param>
		IInstanceRegistrarModifier<TContract> RegisterFactory(string serviceName, Func<IResolutionContext, TImplementation> @delegate);
	}
}