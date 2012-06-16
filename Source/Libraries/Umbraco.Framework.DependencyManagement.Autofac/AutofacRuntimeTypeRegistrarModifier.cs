using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using Autofac;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
    public class AutofacRuntimeTypeRegistrarModifier<TContract> : IRuntimeTypeRegistrarModifier<TContract>
    {
        private readonly IRuntimeTypeRegistrar<TContract> _serviceRegistrar;
        private readonly IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> _containerRegistrationBuilder;

        internal AutofacRuntimeTypeRegistrarModifier(IRuntimeTypeRegistrar<TContract> serviceRegistrar, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> containerRegistrationBuilder)
        {
            _serviceRegistrar = serviceRegistrar;
            _containerRegistrationBuilder = containerRegistrationBuilder;
        }

        /// <summary>Marks the registration as having a singleton scope.</summary>
        /// <returns>Itself</returns>
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

        /// <summary>Marks the registration as having a singleton scope for the lifetime of a http request.</summary>
        /// <value>The per request scoped.</value>
        /// TODO: Put this in a separate assembly as an extension method? Not all providers in all scenarios may support a dependency on System.Web (APN)
        public IInstanceRegistrarModifier<TContract> ScopedPerHttpRequest()
        {
            _containerRegistrationBuilder.InstancePerHttpRequest();
            return this;
        }

        /// <summary>Marks the registration as having a lifetime which is the responsibility of an external system. The container will not manage disposal of this service.</summary>
        public IInstanceRegistrarModifier<TContract> ExternallyOwned()
        {
            _containerRegistrationBuilder.ExternallyOwned();
            return this;
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type.</summary>
        /// <typeparam name="T">The type of the constructor parameter.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithTypedParam<T>(object value)
        {
            return WithTypedParam(typeof(T), value);
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithTypedParam(Type paramType, object value)
        {
            _containerRegistrationBuilder.WithParameter(new TypedParameter(paramType, value));
            return this;
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by name. </summary>
        /// <param name="name">The name of the constructor parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithNamedParam(string name, object value)
        {
            _containerRegistrationBuilder.WithParameter(name, value);
            return this;
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container. </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>()
        {
            return WithResolvedParam<T>(string.Empty);
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container (when given the name of the service). </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>(string serviceName)
        {
            _containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter<T>(serviceName));
            return this;
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by a callback. </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>(Func<IResolutionContext, T> callback)
        {
            _containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter(callback));
            return this;
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by name, whose value is resolved by a callback. </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithNamedResolvedParam<T>(string name, Func<IResolutionContext, T> callback)
        {
            _containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter(name, callback));
            return this;
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType)
        {
            return WithResolvedParam(paramType, string.Empty);
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType, string serviceName)
        {
            _containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter(paramType, serviceName));
            return this;
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType, Func<IResolutionContext, object> callback)
        {
            _containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter(paramType, callback));
            return this;
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithNamedResolvedParam(Type paramType, string name, Func<IResolutionContext, object> callback)
        {
            _containerRegistrationBuilder.WithParameter(AutofacContainerBuilder.GenerateResolvedParameter(paramType, name, callback));
            return this;
        }

        /// <summary>Marks the registration as having a specific property value to be injected, discovered by name. </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value to be injected.</param>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithProperty(string name, object value)
        {
            _containerRegistrationBuilder.WithProperty(name, value);
            return this;
        }

        public IInstanceRegistrarModifier<TContract> WithMetadata<TMetadata, TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value)
        {
            _containerRegistrationBuilder.WithMetadata<TMetadata>(
              new Action<MetadataConfiguration<TMetadata>>(config => config.For(propertyAccessor, value)));
            return this;
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithMetadata(Func<Type, IEnumerable<KeyValuePair<string, object>>> metadata)
        {
            _containerRegistrationBuilder.WithMetadata(metadata.Invoke(typeof(TContract)));
            return this;
        }

        public IInstanceRegistrarModifier<TContract> AfterActivate(Action<IResolutionContext, TContract> callback)
        {
            _containerRegistrationBuilder.OnActivated(x => callback.Invoke(new AutofacResolutionContext(x.Context), (TContract)x.Instance));
            return this;
        }
    }
}