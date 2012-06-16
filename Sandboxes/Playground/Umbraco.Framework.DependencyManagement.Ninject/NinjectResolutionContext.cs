using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;
using Umbraco.Framework.DependencyManagement.Ninject;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
    public class NinjectResolutionContext : IResolutionContext
    {
        private readonly NinjectResolver _resolver;

        public NinjectResolutionContext(IContext context)
        {
            _resolver = new NinjectResolver(context.Kernel);
        }

        public NinjectResolutionContext(IKernel kernel)
        {
            _resolver = new NinjectResolver(kernel);
        }

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public T Resolve<T>()
        {
            return _resolver.Resolve<T>();
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return _resolver.ResolveAll<T>();
        }

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>.</returns>
        public T Resolve<T>(string name)
        {
            return _resolver.Resolve<T>(name);
        }

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public object Resolve(Type type)
        {
            return _resolver.Resolve(type);
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return _resolver.ResolveAll(type);
        }

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public object Resolve(Type type, string name)
        {
            return _resolver.Resolve(type, name);
        }

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        public ResolutionAttemptTuple<T> TryResolve<T>() where T : class
        {
            return _resolver.TryResolve<T>();
        }

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <param name="name">The name of the service to resolve.</param>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        public ResolutionAttemptTuple<T> TryResolve<T>(string name) where T : class
        {
            return _resolver.TryResolve<T>(name);
        }

        public ResolutionAttemptTuple<object> TryResolve(Type type)
        {
            try
            {
                object tryResolve = Resolve(type);
                return new ResolutionAttemptTuple<object>(tryResolve != null, tryResolve);
            }
            catch (Exception)
            {
                return new ResolutionAttemptTuple<object>(false, null);
            }
        }

        public ResolutionAttemptTuple<object> TryResolve(Type type, string name)
        {
            try
            {
                object tryResolve = Resolve(type, name);
                return new ResolutionAttemptTuple<object>(tryResolve != null, tryResolve);
            }
            catch (Exception)
            {
                return new ResolutionAttemptTuple<object>(false, null);
            }
        }
    }
}
