using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Syntax;
using Umbraco.Framework.DependencyManagement.Ninject;
using Ninject.Planning.Targets;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
	public class NinjectServiceRegistrarModifier<TContract, TImplementation> : IServiceRegistrarModifier<TContract, TImplementation>
		where TImplementation : TContract
	{
		private readonly NinjectServiceRegistrar<TContract, TImplementation> _ninjectServiceRegistrar;
		private readonly IBindingWhenInNamedWithOrOnSyntax<TContract> _bindingResult;

		public NinjectServiceRegistrarModifier(NinjectServiceRegistrar<TContract, TImplementation> ninjectServiceRegistrar, IBindingWhenInNamedWithOrOnSyntax<TContract> bindingResult)
		{
			_bindingResult = bindingResult;
			_ninjectServiceRegistrar = ninjectServiceRegistrar;
		}


		/// <summary>Marks the registration as having a singleton scope.</summary>
		/// <returns>Itself</returns>
		/// 
		public IInstanceRegistrarModifier<TContract> ScopedAsSingleton()
		{
			_bindingResult.InSingletonScope();
			return this;
		}

		public IInstanceRegistrarModifier<TContract> ScopedWithNestedLifetime()
		{
			// TODO: Check the validity of the below, not sure it equates to Autofac's nested lifetime scope
			_bindingResult.InScope(context => context.Request);
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
			//TODO: Check if NInject's transient scope is analogous to Autofac's ExternallyOwned
			_bindingResult.InTransientScope();
			return this;
		}

		/// <summary>Marks the registration as having a specific constructor parameter discovered by type.</summary>
		/// <typeparam name="T">The type of the constructor parameter.</typeparam>
		/// <param name="value">The value of the parameter.</param>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithTypedParam<T>(object value)
		{
			throw new NotImplementedException("TODO: Find out if Ninject supports typed parameters");
			//_bindingResult.WithConstructorArgument(string.Empty, (context, target)=>target.)
		}

		/// <summary>Marks the registration as having a specific constructor parameter discovered by name. </summary>
		/// <param name="name">The name of the constructor parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithNamedParam(string name, object value)
		{
			_bindingResult.WithConstructorArgument(name, value);
			return this;
		}

		public IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>()
		{
			var resolver = new NinjectResolver(_bindingResult.Kernel);
			_bindingResult.WithConstructorArgument(null, defer => resolver.Resolve<T>());
			return this;
		}

		/// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container (when given the name of the service). </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>(string serviceName)
		{
			var resolver = new NinjectResolver(_bindingResult.Kernel);
			_bindingResult.WithConstructorArgument(null, defer => resolver.Resolve<T>());
			return this;
		}

		/// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container. </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>(Func<IResolutionContext, T> callback)
		{
			// Create an IResolutionContext from the current Kernel
			var context = new NinjectResolutionContext(_bindingResult.Kernel);
			// Tell Ninject to inboke our callback when the time comes
			_bindingResult.WithConstructorArgument(null, defer => callback.Invoke(context));
			return this;
		}

		/// <summary>Marks the registration as having a specific constructor parameter discovered by name, whose value is resolved by a callback. </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithNamedResolvedParam<T>(string name, Func<IResolutionContext, T> callback)
		{
			// Create an IResolutionContext from the current Kernel
			var context = new NinjectResolutionContext(_bindingResult.Kernel);
			// Tell Ninject to inboke our callback when the time comes
			_bindingResult.WithConstructorArgument(name, defer => callback.Invoke(context));
			return this;
		}

		/// <summary>Marks the registration as having a specific property value to be injected, discovered by name. </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">The value to be injected.</param>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithProperty(string name, object value)
		{
			_bindingResult.WithPropertyValue(name, value);
			return this;
		}
	}

	

    /// <summary>
    /// Modifies an activation process in some way.
    /// </summary>
    public class Parameter : IParameter
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the parameter should be inherited into child requests.
        /// </summary>
        public bool ShouldInherit { get; private set; }

        /// <summary>
        /// Gets or sets the callback that will be triggered to get the parameter's value.
        /// </summary>
        public Func<IContext, ITarget, object> ValueCallback { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="shouldInherit">Whether the parameter should be inherited into child requests.</param>
        public Parameter(string name, object value, bool shouldInherit) : this(name, (ctx, target) => value, shouldInherit) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="valueCallback">The callback that will be triggered to get the parameter's value.</param>
        /// <param name="shouldInherit">Whether the parameter should be inherited into child requests.</param>
        public Parameter(string name, Func<IContext, object> valueCallback, bool shouldInherit)
        {
            Name = name;
            ValueCallback = (ctx, target) => valueCallback(ctx);
            ShouldInherit = shouldInherit;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="valueCallback">The callback that will be triggered to get the parameter's value.</param>
        /// <param name="shouldInherit">Whether the parameter should be inherited into child requests.</param>
        public Parameter(string name, Func<IContext, ITarget, object> valueCallback, bool shouldInherit)
        {
            Name = name;
            ValueCallback = valueCallback;
            ShouldInherit = shouldInherit;
        }
        
        /// <summary>
        /// Gets the value for the parameter within the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <returns>The value for the parameter.</returns>
        public object GetValue(IContext context, ITarget target)
        {
            return ValueCallback(context, target);
        }

        /// <summary>
        /// Determines whether the object equals the specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns><c>True</c> if the objects are equal; otherwise <c>false</c></returns>
        public override bool Equals(object obj)
        {
            var parameter = obj as IParameter;
            return parameter != null ? Equals(parameter) : base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the object.</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Name.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>True</c> if the objects are equal; otherwise <c>false</c></returns>
        public bool Equals(IParameter other)
        {
            return other.GetType() == GetType() && other.Name.Equals(Name);
        }
    }
}
