using System;
using System.Linq.Expressions;

namespace Umbraco.Framework.DependencyManagement
{
	/// <summary>Interface for extending options when registering a service via a <see cref="IContainerBuilder"/>.</summary>
	/// <remarks>Doc updated, 14-Jan-2011.</remarks>
	public interface IServiceRegistrarModifier<TContract, TImplementation> : IInstanceRegistrarModifier<TContract>
		where TImplementation : TContract
	{
		/// <summary>Marks the registration as having a specific constructor parameter discovered by type.</summary>
		/// <typeparam name="T">The type of the constructor parameter.</typeparam>
		/// <param name="value">The value of the parameter.</param>
		/// <returns>.</returns>
		IServiceRegistrarModifier<TContract, TImplementation> WithTypedParam<T>(object value);

		/// <summary>Marks the registration as having a specific constructor parameter discovered by name. </summary>
		/// <param name="name">The name of the constructor parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <returns>.</returns>
		IServiceRegistrarModifier<TContract, TImplementation> WithNamedParam(string name, object value);

		/// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container. </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>();

		/// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by the container (when given the name of the service). </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>(string serviceName);

		/// <summary>Marks the registration as having a specific constructor parameter discovered by type, whose value is resolved by a callback. </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		IServiceRegistrarModifier<TContract, TImplementation> WithResolvedParam<T>(Func<IResolutionContext, T> callback);

		/// <summary>Marks the registration as having a specific constructor parameter discovered by name, whose value is resolved by a callback. </summary>
		/// <typeparam name="T">The type of the service to resolve in order to pass in as the value of the parameter.</typeparam>
		/// <returns>.</returns>
		IServiceRegistrarModifier<TContract, TImplementation> WithNamedResolvedParam<T>(string name, Func<IResolutionContext, T> callback);

		/// <summary>Marks the registration as having a specific property value to be injected, discovered by name. </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">The value to be injected.</param>
		/// <returns>.</returns>
		IServiceRegistrarModifier<TContract, TImplementation> WithProperty(string name, object value);

        /// <summary>Associates the registration with metadata, which will be made available to the service on resolution. </summary>
        /// <typeparam name="TMetadata">The <typeparamref name="TMetadata" /> type used for the metadata.</typeparam>
        /// <typeparam name="TProperty">The <typeparamref name="TProperty" /> type used for the property.</typeparam>
        /// <param name="propertyAccessor">The property accessor.</param>
        /// <param name="value">The value.</param>
        /// <returns>.</returns>
        IServiceRegistrarModifier<TContract, TImplementation> WithMetadata<TMetadata, TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value);
	}
}