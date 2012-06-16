using System;
using System.Collections.Generic;

namespace Umbraco.Framework.DependencyManagement
{

    ///<summary>
    /// All dependency resolvers should inherit from this which ensures the correct constructor
    ///</summary>
    public abstract class AbstractDependencyResolver : DisposableObject, IDependencyResolver
    {
        protected IDependencyResolver DependencyResolver { get; private set; }

        protected AbstractDependencyResolver(IDependencyResolver dependencyResolver)
        {
            DependencyResolver = dependencyResolver;
        }

        public abstract T Resolve<T>();
        public abstract IEnumerable<T> ResolveAll<T>();
        public abstract T Resolve<T>(string name);
        public abstract object Resolve(Type type);
        public abstract IEnumerable<object> ResolveAll(Type type);
        public abstract object Resolve(Type type, string name);
        public abstract ResolutionAttempt<T> TryResolve<T>() where T : class;
        public abstract NamedResolutionAttempt<T> TryResolve<T>(string name) where T : class;
        public abstract ResolutionAttempt<object> TryResolve(Type type);
        public abstract NamedResolutionAttempt<object> TryResolve(Type type, string name);
    }
}
