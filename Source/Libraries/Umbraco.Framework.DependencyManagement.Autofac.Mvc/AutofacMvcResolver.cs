using System;
using System.Collections.Generic;
using Autofac.Integration.Mvc;

namespace Umbraco.Framework.DependencyManagement.Autofac.Mvc
{
    public class AutofacMvcResolver : AbstractDependencyResolver, System.Web.Mvc.IDependencyResolver
    {
        private readonly AutofacResolver _dependencyResolver;
        private readonly AutofacDependencyResolver _realDependencyResolver;

        public AutofacMvcResolver(IDependencyResolver resolver)
            : base(resolver)
        {
            _dependencyResolver = resolver as AutofacResolver;
            if (_dependencyResolver == null)
                throw new ArgumentException("Must be of type 'Umbraco.Framework.DependencyManagement.Autofac.AutofacResolver'", "resolver");
            _realDependencyResolver = new AutofacDependencyResolver(_dependencyResolver.AutofacContainer);
        }

        /// <summary>
        /// Resolves singly registered services that support arbitrary object creation.
        /// </summary>
        /// <returns>
        /// The requested service or object if resolved; otherwise, <c>null</c>.
        /// </returns>
        /// <param name="serviceType">The type of the requested service or object.</param>
        public object GetService(Type serviceType)
        {
            //ResolutionAttemptTuple<object> result = TryResolve(serviceType);
            //return result.Value;
            return _realDependencyResolver.GetService(serviceType);
        }

        /// <summary>
        /// Resolves multiply registered services.
        /// </summary>
        /// <returns>
        /// The requested services.
        /// </returns>
        /// <param name="serviceType">The type of the requested services.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _realDependencyResolver.GetServices(serviceType);
        }

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public override T Resolve<T>()
        {
            return _dependencyResolver.Resolve<T>();
        }

        public override IEnumerable<T> ResolveAll<T>()
        {
            return _dependencyResolver.ResolveAll<T>();
        }

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>.</returns>
        public override T Resolve<T>(string name)
        {
            return _dependencyResolver.Resolve<T>(name);
        }

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public override object Resolve(Type type)
        {
            return _dependencyResolver.Resolve(type);
        }

        public override IEnumerable<object> ResolveAll(Type type)
        {
            return _dependencyResolver.ResolveAll(type);
        }

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public override object Resolve(Type type, string name)
        {
            return _dependencyResolver.Resolve(type, name);
        }

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        public override ResolutionAttempt<T> TryResolve<T>()
        {
            return _dependencyResolver.TryResolve<T>();
        }

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <param name="name">The name of the service to resolve.</param>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        public override ResolutionAttempt<T> TryResolve<T>(string name)
        {
            return _dependencyResolver.TryResolve<T>(name);
        }

        public override ResolutionAttempt<object> TryResolve(Type type)
        {
            try
            {
                object tryResolve = Resolve(type);
                return new ResolutionAttempt<object>(tryResolve != null, tryResolve);
            }
            catch (Exception)
            {
                return new ResolutionAttempt<object>(false, null);
            }
        }

        public override ResolutionAttempt<object> TryResolve(Type type, string name)
        {
            try
            {
                object tryResolve = Resolve(type, name);
                return new ResolutionAttempt<object>(tryResolve != null, tryResolve);
            }
            catch (Exception)
            {
                return new ResolutionAttempt<object>(false, null);
            }
        }
    }
}
