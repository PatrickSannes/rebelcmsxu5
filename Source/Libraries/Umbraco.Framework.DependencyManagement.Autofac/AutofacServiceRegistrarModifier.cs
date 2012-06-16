using System;
using System.Linq.Expressions;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Mvc;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
	/// <summary>Autofac container registration builder.</summary>
	/// <remarks>Doc updated, 14-Jan-2011.</remarks>
	/// <typeparam name="TContract">The <typeparamref name="TContract" /> type used for the contract.</typeparam>
	/// <typeparam name="TImplementation">The <typeparamref name="TImplementation" /> type used for the implementation.</typeparam>
	public class AutofacServiceRegistrarModifier<TContract, TImplementation> : IServiceRegistrarModifier<TContract, TImplementation>
		where TImplementation : TContract
	{
		private readonly IServiceRegistrar<TContract, TImplementation> _serviceRegistrar;
		private readonly IRegistrationBuilder<TImplementation, ReflectionActivatorData, SingleRegistrationStyle> _containerRegistrationBuilder;


		internal AutofacServiceRegistrarModifier(IServiceRegistrar<TContract, TImplementation> serviceRegistrar, IRegistrationBuilder<TImplementation, ReflectionActivatorData, SingleRegistrationStyle> containerRegistrationBuilder)
		{
			_serviceRegistrar = serviceRegistrar;
			_containerRegistrationBuilder = containerRegistrationBuilder;
		}

		public IInstanceRegistrarModifier<TContract> ScopedAsSingleton()
		{
			_containerRegistrationBuilder.SingleInstance();

			return this;
		}

		public IInstanceRegistrarModifier<TContract> ScopedWithNestedLifetime()
		{
			_containerRegistrationBuilder.InstancePerLifetimeScope();
			return this;
		}

		public IInstanceRegistrarModifier<TContract> ScopedPerHttpRequest()
		{
			_containerRegistrationBuilder.InstancePerHttpRequest();

			return this;
		}

		public IInstanceRegistrarModifier<TContract> ExternallyOwned()
		{
			_containerRegistrationBuilder.ExternallyOwned();

			return this;
		}

	    /// <summary>Associates the registration with metadata, which will be made available to the service on resolution. </summary>
	    /// <typeparam name="TMetadata">The <typeparamref name="TMetadata" /> type used for the metadata.</typeparam>
	    /// <typeparam name="TProperty">The <typeparamref name="TProperty" /> type used for the property.</typeparam>
	    /// <param name="propertyAccessor">The property accessor.</param>
	    /// <param name="value">The value.</param>
	    /// <returns>.</returns>
	    IServiceRegistrarModifier<TContract, TImplementation> IServiceRegistrarModifier<TContract, TImplementation>.WithMetadata<TMetadata, TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value)
	    {
            _containerRegistrationBuilder.WithMetadata<TMetadata>(
              new Action<MetadataConfiguration<TMetadata>>(config => config.For(propertyAccessor, value)));
            return this;
	    }

	    public IInstanceRegistrarModifier<TContract> WithMetadata<TMetadata, TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value)
	    {
            _containerRegistrationBuilder.WithMetadata<TMetadata>(
             new Action<MetadataConfiguration<TMetadata>>(config => config.For(propertyAccessor, value)));
            return this;
	    }

	    public IServiceRegistrarModifier<TContract, TImplementation> WithTypedParam<T>(object value)
		{
			_containerRegistrationBuilder.WithParameter(new TypedParameter(typeof(T), value));
			return this;
		}

		public IServiceRegistrarModifier<TContract, TImplementation> WithNamedParam(string name, object value)
		{
			_containerRegistrationBuilder.WithParameter(new NamedParameter(name, value));
			return this;
		}

		public IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>()
		{
			return WithResolvedParam<T>((string)null);
		}

		/// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container (when given the name of the service). </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>(string serviceName)
		{
			_containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter<T>(serviceName));
			return this;
		}

		/// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container. </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>(Func<IResolutionContext, T> callback)
		{
			_containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter(callback));
			return this;
		}

		/// <summary>Marks the registration as having a specific constructor parameter discovered by name, whose value is resolved by a callback. </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithNamedResolvedParam<T>(string name, Func<IResolutionContext, T> callback)
		{
			_containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter(name, callback));
			return this;
		}

		/// <summary>Marks the registration as having a specific property value to be injected, discovered by name. </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">The value to be injected.</param>
		/// <returns>.</returns>
		public IServiceRegistrarModifier<TContract, TImplementation> WithProperty(string name, object value)
		{
			_containerRegistrationBuilder.WithProperty(name, value);
			return this;
		}

        public IInstanceRegistrarModifier<TContract> AfterActivate(Action<IResolutionContext, TContract> callback)
        {
            _containerRegistrationBuilder.OnActivated(x => callback.Invoke(new AutofacResolutionContext(x.Context), x.Instance));
            return this;
        }
	}


}