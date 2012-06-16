using System;
using System.Collections.Generic;

using System.Linq.Expressions;
using System.Reflection;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Targets;
using Ninject.Syntax;
using Umbraco.Framework.DependencyManagement.Ninject;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
    public class NinjectRuntimeTypeRegistrarModifier<TContract> : IRuntimeTypeRegistrarModifier<TContract>
    {
        private readonly IBindingWhenInNamedWithOrOnSyntax<TContract> _bindingResult;

        public NinjectRuntimeTypeRegistrarModifier(IBindingWhenInNamedWithOrOnSyntax<TContract> bindingResult)
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

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type.</summary>
        /// <typeparam name="T">The type of the constructor parameter.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithTypedParam<T>(object value)
        {
            throw new NotImplementedException("TODO: Find out how to do this with Ninject");
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithTypedParam(Type paramType, object value)
        {
            throw new NotImplementedException("TODO: Find out how to do this with Ninject");
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by name. </summary>
        /// <param name="name">The name of the constructor parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithNamedParam(string name, object value)
        {
            _bindingResult.WithConstructorArgument(name, value);
            return this;
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container. </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>()
        {
            NinjectResolver resolver = new NinjectResolver(_bindingResult.Kernel);
            var paramValue = resolver.Resolve<T>();
            _bindingResult.WithConstructorArgument(null, paramValue);
            return this;
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container (when given the name of the service). </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>(string serviceName)
        {
            NinjectResolver resolver = new NinjectResolver(_bindingResult.Kernel);
            var paramValue = resolver.Resolve<T>(serviceName);
            _bindingResult.WithConstructorArgument(null, paramValue);
            return this;
        }

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by a callback. </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>(Func<IResolutionContext, T> callback)
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
        public IRuntimeTypeRegistrarModifier<TContract> WithNamedResolvedParam<T>(string name, Func<IResolutionContext, T> callback)
        {
            // Create an IResolutionContext from the current Kernel
            var context = new NinjectResolutionContext(_bindingResult.Kernel);
            // Tell Ninject to inboke our callback when the time comes
            _bindingResult.WithConstructorArgument(name, defer => callback.Invoke(context));

            return this;
        }

        

        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType)
        {
            throw new NotImplementedException();
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType, string serviceName)
        {
            throw new NotImplementedException();
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType, Func<IResolutionContext, object> callback)
        {
            throw new NotImplementedException();
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithNamedResolvedParam(Type paramType, string name, Func<IResolutionContext, object> callback)
        {
            throw new NotImplementedException();
        }

        /// <summary>Marks the registration as having a specific property value to be injected, discovered by name. </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value to be injected.</param>
        /// <returns>.</returns>
        public IRuntimeTypeRegistrarModifier<TContract> WithProperty(string name, object value)
        {
            _bindingResult.WithPropertyValue(name, value);
            return this;
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithMetadata<TMetadata, TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value)
        {
            throw new NotImplementedException();
        }

        public IRuntimeTypeRegistrarModifier<TContract> WithMetadata(Func<Type, IEnumerable<KeyValuePair<string, object>>> metadata)
        {
            throw new NotImplementedException();
        }
    }
}