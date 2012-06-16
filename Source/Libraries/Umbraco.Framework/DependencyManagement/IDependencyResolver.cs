using System;
using System.Collections.Generic;

namespace Umbraco.Framework.DependencyManagement
{
    /// <summary>Interface for dependency resolving. In specific implementations, allows for the resolution of types by a chosen IoC container.</summary>
    /// <remarks>Doc updated, 14-Jan-2011.</remarks>
    public interface IDependencyResolver : IDisposable
    {
        /// <summary>Resolves a service given the required contract type.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        T Resolve<T>();

        /// <summary>
        /// Resolves all services given the required contract type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> of instances of type <typeparamref name="T"/>.</returns>
        IEnumerable<T> ResolveAll<T>();

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <typeparam name="T">The type with which the service implementation was registered.</typeparam>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>.</returns>
        T Resolve<T>(string name);

        /// <summary>Resolves a service given the required contract type.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolves all services given the required contract type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of instances of type <paramref name="type"/>.
        /// </returns>
        IEnumerable<object> ResolveAll(Type type);

        /// <summary>Resolves a service given the required contract type, and the required name.</summary>
        /// <param name="type">The type with which the service implementation was registered.</param>
        /// <param name="name">The name of the service to resolve.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        object Resolve(Type type, string name);

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        ResolutionAttempt<T> TryResolve<T>() where T : class;

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <param name="name">The name of the service to resolve.</param>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        NamedResolutionAttempt<T> TryResolve<T>(string name) where T : class;

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <paramref name="type"/> parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <param name="type">The type to resolve.</param>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        ResolutionAttempt<object> TryResolve(Type type);

        /// <summary>
        /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
        /// </summary>
        /// <remarks>The <paramref name="type"/> parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
        /// <param name="name">The name of the service to resolve.</param>
        /// <param name="type">The type to resolve.</param>
        /// <returns>A tuple indicating success and the resolved value, if any.</returns>
        NamedResolutionAttempt<object> TryResolve(Type type, string name);
    }
}