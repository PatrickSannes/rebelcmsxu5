using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Autofac;
using Autofac.Core;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
    public class AutofacResolutionContext : IResolutionContext
    {
        private readonly IComponentContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AutofacResolutionContext(IComponentContext context)
        {
            _context = context;
        }

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public T Resolve<T>()
        {
            Debug.WriteLine("Resolve call for {0} on thread {1}".InvariantFormat(typeof(T).Name, Thread.CurrentThread.ManagedThreadId));
            try
            {
                return _context.Resolve<T>();
            }
            catch (Exception)
            {
                var registeredIds = _context.ComponentRegistry.Registrations.Select(x => x.Id).ToArray();
                var distinctIds = registeredIds.Distinct().ToArray();
                throw;
            }
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            // We're going to find each service which was registered
            // with a key, and for those which match the type T we'll store the key
            // and later supplement the default output with individual resolve calls to those
            // keyed services
            var allKeys = new List<object>();
            foreach (var componentRegistration in _context.ComponentRegistry.Registrations)
            {
                // Get the services which match the KeyedService type
                var typedServices = componentRegistration.Services.Where(x => x is KeyedService).Cast<KeyedService>();
                // Add the key to our list so long as the registration is for the correct type T
                allKeys.AddRange(typedServices.Where(y => y.ServiceType == typeof (T)).Select(x => x.ServiceKey));
            }

            // Get the default resolution output which resolves all un-keyed services
            
            var allUnKeyedServices = new List<T>(_context.Resolve<IEnumerable<T>>());
            // Add the ones which were registered with a key
            allUnKeyedServices.AddRange(allKeys.Distinct().Select(key => _context.ResolveKeyed<T>(key)));

            // Return the total resultset
            return allUnKeyedServices;
        }

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>.</returns>
        public T Resolve<T>(string name)
        {
            return _context.ResolveNamed<T>(name);
        }

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public object Resolve(Type type)
        {
            return _context.Resolve(type);
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(type);
            var instance = _context.Resolve(enumerableServiceType);

            return (IEnumerable<object>)instance;
        }

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public object Resolve(Type type, string name)
        {
            return _context.ResolveNamed(name, type);
        }

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        public ResolutionAttempt<T> TryResolve<T>() where T : class
        {
            try
            {
                T tryResolve = Resolve<T>();
                return new ResolutionAttempt<T>(tryResolve != null, tryResolve);
            }
            catch (Exception ex)
            {
                return new ResolutionAttempt<T>(ex);
            }
        }

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <param name="name">The name of the service to resolve.</param>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        public NamedResolutionAttempt<T> TryResolve<T>(string name) where T : class
        {
            try
            {
                T tryResolve = Resolve<T>(name);
                return new NamedResolutionAttempt<T>(name, tryResolve != null, tryResolve);
            }
            catch (Exception ex)
            {
                return new NamedResolutionAttempt<T>(name, ex);
            }
        }

        public ResolutionAttempt<object> TryResolve(Type type)
        {
            try
            {
                object tryResolve = Resolve(type);
                return new ResolutionAttempt<object>(tryResolve != null, tryResolve);
            }
            catch (Exception ex)
            {
                return new ResolutionAttempt<object>(ex);
            }
        }

        public NamedResolutionAttempt<object> TryResolve(Type type, string name)
        {
            try
            {
                object tryResolve = Resolve(type, name);
                return new NamedResolutionAttempt<object>(name, tryResolve != null, tryResolve);
            }
            catch (Exception ex)
            {
                return new NamedResolutionAttempt<object>(name, ex);
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _context.IfNotNull(x => x.ComponentRegistry.IfNotNull(y => y.Dispose()));
        }

        #endregion
    }
}
