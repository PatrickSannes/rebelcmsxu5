using System;
using System.Linq.Expressions;

namespace Umbraco.Framework.DependencyManagement
{
	public interface IInstanceRegistrarModifier<TContract>
	{
		/// <summary>Marks the registration as having a singleton scope.</summary>
		/// <returns>Itself</returns>
		IInstanceRegistrarModifier<TContract> ScopedAsSingleton();

		/// <summary>Marks the registration as having a scope which lasts for the lifetime of its parent. </summary>
		/// <returns>Itself</returns>
		IInstanceRegistrarModifier<TContract> ScopedWithNestedLifetime();

		/// <summary>Marks the registration as having a singleton scope for the lifetime of a http request.</summary>
		/// <value>The per request scoped.</value>
		/// TODO: Put this in a separate assembly as an extension method? Not all providers in all scenarios may support a dependency on System.Web (APN)
		IInstanceRegistrarModifier<TContract> ScopedPerHttpRequest();

		/// <summary>Marks the registration as having a lifetime which is the responsibility of an external system. The container will not manage disposal of this service.</summary>
		IInstanceRegistrarModifier<TContract> ExternallyOwned();

	    /// <summary>
	    /// Associates the registration with metadata, which will be made available to the service on resolution.
	    /// </summary>
	    /// <typeparam name="TMetadata">The <typeparamref name="TMetadata"/> type used for the metadata.</typeparam>
	    /// <typeparam name="TProperty">The <typeparamref name="TProperty"/> type used for the property.</typeparam>
	    /// <param name="propertyAccessor">The property accessor.</param>
	    /// <param name="value">The value.</param>
	    /// <returns>.</returns>
	    /// <remarks></remarks>
        IInstanceRegistrarModifier<TContract> WithMetadata<TMetadata, TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value);

        /// <summary>
        /// Specifies a delegate to be called after the component has been activated.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        IInstanceRegistrarModifier<TContract> AfterActivate(Action<IResolutionContext, TContract> callback);
	}
}