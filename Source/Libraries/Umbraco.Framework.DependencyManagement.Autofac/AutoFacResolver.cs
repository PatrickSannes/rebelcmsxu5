using System;
using System.Collections.Generic;
using Autofac;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
    public class AutofacResolver : IDependencyResolver
    {
        private readonly ILifetimeScope _container = null;

        internal AutofacResolver(ILifetimeScope container)
        {
            _container = container;
        }

        internal ILifetimeScope AutofacContainer
        {
            get { return _container; }
        }

        public T Resolve<T>()
        {
            return AutofacContainer.Resolve<T>();
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return AutofacContainer.Resolve<IEnumerable<T>>();
        }

        public T Resolve<T>(string name)
        {
            return AutofacContainer.ResolveNamed<T>(name);
        }

        public object Resolve(Type type)
        {
            return AutofacContainer.Resolve(type);
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(type);
            var instance = AutofacContainer.Resolve(enumerableServiceType);

            return (IEnumerable<object>)instance;
        }

        public object Resolve(Type type, string name)
        {
            return AutofacContainer.ResolveNamed(name, type);
        }

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
            _container.Dispose();
        }

        #endregion
    }
}
