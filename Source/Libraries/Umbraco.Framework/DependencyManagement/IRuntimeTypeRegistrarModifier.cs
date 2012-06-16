using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Umbraco.Framework.DependencyManagement
{
    /// <summary>Interface a dependency registrar modifier whose implementation types are specified at runtime.</summary>
    /// <remarks>Doc updated, 23-Jan-2011.</remarks>
    /// <typeparam name="TContract">The <typeparamref name="TContract" /> type used for the type-binding contracts.</typeparam>
    public interface IRuntimeTypeRegistrarModifier<TContract> : IInstanceRegistrarModifier<TContract>
    {
        /// <summary>Marks the registration as having a specific constructor parameter discovered by type.</summary>
        /// <typeparam name="T">The type of the constructor parameter.</typeparam>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithTypedParam<T>(object value);

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type.</summary>
        /// <param name="paramType">The type of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithTypedParam(Type paramType, object value);

        /// <summary>Marks the registration as having a specific constructor parameter discovered by name. </summary>
        /// <param name="name">The name of the constructor parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithNamedParam(string name, object value);

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container. </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>();

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container (when given the name of the service). </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>(string serviceName);

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by a callback. </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam<T>(Func<IResolutionContext, T> callback);

        /// <summary>Marks the registration as having a specific constructor parameter discovered by name, whose value is resolved by a callback. </summary>
        /// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithNamedResolvedParam<T>(string name, Func<IResolutionContext, T> callback);


        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container. </summary>
        /// <param name="paramType">The type of the parameter.</param> 
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType);

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container (when given the name of the service).</summary>
        /// <param name="paramType">The type of the parameter.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType, string serviceName);

        /// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by a callback.</summary>
        /// <param name="paramType">The type of the parameter.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithResolvedParam(Type paramType, Func<IResolutionContext, object> callback);

        /// <summary>Marks the registration as having a specific constructor parameter discovered by name, whose value is resolved by a callback.</summary>
        /// <param name="paramType">The type of the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithNamedResolvedParam(Type paramType, string name, Func<IResolutionContext, object> callback);

        /// <summary>Marks the registration as having a specific property value to be injected, discovered by name. </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value to be injected.</param>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithProperty(string name, object value);

        /// <summary>Associates the registration with metadata, which will be made available to the service on resolution.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <returns>.</returns>
        IRuntimeTypeRegistrarModifier<TContract> WithMetadata(Func<Type, IEnumerable<KeyValuePair<string, object>>> metadata);
    }
}