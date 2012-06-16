using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
    /// <summary>An implementation of <see cref="IDependencyResolver"/> based on the NInject IoC library.</summary>
    /// <remarks>Doc updated, 22-Jan-2011.</remarks>
    public class NinjectResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        protected IKernel Kernel
        {
            get { return _kernel; }
        }

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public T Resolve<T>()
        {
            //TODO: Check if this is the most performant way of resolving non-named services with Ninject
            return Kernel.Get<T>(where => where.Name == null);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return Kernel.GetAll<T>();
        }

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>.</returns>
        public T Resolve<T>(string name)
        {
            return Kernel.Get<T>(name);
        }

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public object Resolve(Type type)
        {
            return Kernel.Get(type);
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return Kernel.GetAll(type);
        }

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public object Resolve(Type type, string name)
        {
            return Kernel.Get(type, name);
        }

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        public ResolutionAttemptTuple<T> TryResolve<T>() where T : class
        {
            var tryGet = Kernel.TryGet<T>();
            return new ResolutionAttemptTuple<T>(tryGet != null, tryGet);
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
            var tryGet = Kernel.TryGet<T>(name);
            return new ResolutionAttemptTuple<T>(tryGet != null, tryGet);
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
