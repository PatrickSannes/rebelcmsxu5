using System;
using Ninject.Syntax;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
	public class NinjectInstanceRegistrarModifier<TContract> : IInstanceRegistrarModifier<TContract>
	{
		private readonly IBindingInSyntax<TContract> _bindingResult;

		public NinjectInstanceRegistrarModifier(IBindingInSyntax<TContract> bindingResult)
		{
			_bindingResult = bindingResult;
		}

		/// <summary>Marks the registration as having a singleton scope.</summary>
		/// <returns>Itself</returns>
		public IInstanceRegistrarModifier<TContract> ScopedAsSingleton()
		{
			_bindingResult.InSingletonScope();
			return this;
		}

		public IInstanceRegistrarModifier<TContract> ScopedWithNestedLifetime()
		{
			//TODO: Check if NInject's transient scope is analogous to Autofac's ExternallyOwned
			_bindingResult.InTransientScope();
			return this;
		}

		/// <summary>Marks the registration as having a singleton scope for the lifetime of a http request.</summary>
		/// <value>The per request scoped.</value>
		/// TODO: Put this in a separate assembly as an extension method? Not all providers in all scenarios may support a dependency on System.Web (APN)
		public IInstanceRegistrarModifier<TContract> ScopedPerHttpRequest()
		{
			_bindingResult.InRequestScope();
			return this;
		}

		/// <summary>Marks the registration as having a lifetime which is the responsibility of an external system. The container will not manage disposal of this service.</summary>
		public IInstanceRegistrarModifier<TContract> ExternallyOwned()
		{
			_bindingResult.InTransientScope();
			return this;
		}
	}
}