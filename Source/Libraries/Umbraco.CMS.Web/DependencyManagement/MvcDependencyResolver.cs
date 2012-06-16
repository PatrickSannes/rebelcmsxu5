namespace Umbraco.Cms.Web.DependencyManagement
{
    using Umbraco.Framework;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Web.Mvc;
    using AbstractDependencyResolver = Umbraco.Framework.DependencyManagement.AbstractDependencyResolver;

    /// <summary>
    ///   A class which passes requests from ASP.NET Mvc's <see cref="IDependencyResolver" /> to Umbraco's equivalent <see
    ///    cref="Umbraco.Framework.DependencyManagement.IDependencyResolver" /> .
    /// </summary>
    public class MvcDependencyResolver : AbstractDependencyResolver, IDependencyResolver
    {
        #region Constructors and Destructors

        public MvcDependencyResolver(
            global::Umbraco.Framework.DependencyManagement.IDependencyResolver dependencyResolver)
            : base(dependencyResolver)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Resolves singly registered services that support arbitrary object creation.
        /// </summary>
        /// <returns> The requested service or object. </returns>
        /// <param name="serviceType"> The type of the requested service or object. </param>
        public object GetService(Type serviceType)
        {
            return DependencyResolver.Resolve(serviceType);
        }

        /// <summary>
        ///   Resolves multiply registered services.
        /// </summary>
        /// <returns> The requested services. </returns>
        /// <param name="serviceType"> The type of the requested services. </param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return DependencyResolver.ResolveAll(serviceType);
        }

        /// <summary>
        ///   Resolves a service given the required contract type.
        /// </summary>
        /// <typeparam name="T"> The type with which the service implementation was registered. </typeparam>
        /// <returns> An instance of type <typeparamref name="T" /> . </returns>
        public override T Resolve<T>()
        {
            return DependencyResolver.Resolve<T>();
        }

        /// <summary>
        ///   Resolves a service given the required contract type, and the required name.
        /// </summary>
        /// <typeparam name="T"> The type with which the service implementation was registered. </typeparam>
        /// <param name="name"> The name of the service to resolve. </param>
        /// <returns> . </returns>
        public override T Resolve<T>(string name)
        {
            return DependencyResolver.Resolve<T>(name);
        }

        /// <summary>
        ///   Resolves a service given the required contract type.
        /// </summary>
        /// <param name="type"> The type with which the service implementation was registered. </param>
        /// <returns> An instance of type <paramref name="type" /> . </returns>
        public override object Resolve(Type type)
        {
            return DependencyResolver.Resolve(type);
        }

        /// <summary>
        ///   Resolves a service given the required contract type, and the required name.
        /// </summary>
        /// <param name="type"> The type with which the service implementation was registered. </param>
        /// <param name="name"> The name of the service to resolve. </param>
        /// <returns> An instance of type <paramref name="type" /> . </returns>
        public override object Resolve(Type type, string name)
        {
            return DependencyResolver.Resolve(type, name);
        }

        /// <summary>
        ///   Resolves all services given the required contract type.
        /// </summary>
        /// <typeparam name="T" />
        /// <returns> An <see cref="IEnumerable{T}" /> of instances of type <typeparamref name="T" /> . </returns>
        public override IEnumerable<T> ResolveAll<T>()
        {
            return DependencyResolver.ResolveAll<T>();
        }

        /// <summary>
        ///   Resolves all services given the required contract type.
        /// </summary>
        /// <param name="type"> The type. </param>
        /// <returns> An <see cref="IEnumerable{T}" /> of instances of type <paramref name="type" /> . </returns>
        public override IEnumerable<object> ResolveAll(Type type)
        {
            return DependencyResolver.ResolveAll(type);
        }

        /// <summary>
        ///   Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>
        ///   The <typeparamref name="T" /> type parameter must be a reference (class) object in order to assess null resolutions predictably.
        /// </remarks>
        /// <typeparam name="T"> The type to resolve. </typeparam>
        /// <returns> A tuple indicating success and the resolved value, if any. </returns>
        public override Framework.DependencyManagement.ResolutionAttempt<T> TryResolve<T>()
        {
            return DependencyResolver.TryResolve<T>();
        }

        /// <summary>
        ///   Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>
        ///   The <typeparamref name="T" /> type parameter must be a reference (class) object in order to assess null resolutions predictably.
        /// </remarks>
        /// <param name="name"> The name of the service to resolve. </param>
        /// <typeparam name="T"> The type to resolve. </typeparam>
        /// <returns> A tuple indicating success and the resolved value, if any. </returns>
        public override Framework.DependencyManagement.NamedResolutionAttempt<T> TryResolve<T>(string name)
        {
            return DependencyResolver.TryResolve<T>(name);
        }

        /// <summary>
        ///   Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>
        ///   The <paramref name="type" /> parameter must be a reference (class) object in order to assess null resolutions predictably.
        /// </remarks>
        /// <param name="type"> The type to resolve. </param>
        /// <returns> A tuple indicating success and the resolved value, if any. </returns>
        public override Framework.DependencyManagement.ResolutionAttempt<object> TryResolve(Type type)
        {
            return DependencyResolver.TryResolve(type);
        }

        /// <summary>
        ///   Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>
        ///   The <paramref name="type" /> parameter must be a reference (class) object in order to assess null resolutions predictably.
        /// </remarks>
        /// <param name="name"> The name of the service to resolve. </param>
        /// <param name="type"> The type to resolve. </param>
        /// <returns> A tuple indicating success and the resolved value, if any. </returns>
        public override Framework.DependencyManagement.NamedResolutionAttempt<object> TryResolve(Type type, string name)
        {
            return DependencyResolver.TryResolve(type, name);
        }

        #endregion

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            DependencyResolver.IfNotNull(x => x.Dispose());
        }

        #endregion
    }
}