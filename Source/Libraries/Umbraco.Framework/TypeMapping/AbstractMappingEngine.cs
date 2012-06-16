using System;
using System.Collections.Generic;

using Umbraco.Framework.DependencyManagement;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// An abstract implementation of an object value mapper.
    /// </summary>
    /// <remarks></remarks>
    public abstract class AbstractMappingEngine
    {

        /// <summary>
        /// Maps the specified source to a new instance, given an explicitly defined source <typeparamref name="TSource"/> and destination <typeparamref name="TDestination"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual TDestination Map<TSource, TDestination>(TSource source)
        {
            return (TDestination)Map(source, typeof(TSource), typeof(TDestination));        
        }

        /// <summary>
        /// Maps the specified source to an instance of type <typeparamref name="TDestination"/>.
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public TDestination Map<TDestination>(object source)
        {
            Mandate.ParameterNotNull(source, "source");
            return (TDestination)Map(source, source.GetType(), typeof(TDestination));
        }

        /// <summary>
        /// Maps the specified source onto an existing instance <paramref name="destination"/>, given an explicitly defined source <typeparamref name="TSource"/> and destination <typeparamref name="TDestination"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <remarks></remarks>
        public virtual void Map<TSource, TDestination>(TSource source, TDestination destination) where TDestination : class
        {
            Map(source, destination, typeof(TSource), typeof(TDestination));
        }

        /// <summary>
        /// Maps the specified source to a new instance, given an explicitly defined source <paramref name="sourceType"/> and destination <paramref name="destinationType"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract object Map(object source, Type sourceType, Type destinationType);

        /// <summary>
        /// Maps the specified source onto an existing instance <paramref name="destination"/>, given an explicitly defined source <paramref name="sourceType"/> and destination <paramref name="destinationType"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <remarks></remarks>
        public abstract void Map(object source, object destination, Type sourceType, Type destinationType);

        /// <summary>
        /// Configures this instance.
        /// </summary>
        /// <remarks></remarks>
        public abstract void Configure();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is configured.
        /// </summary>
        /// <value><c>true</c> if this instance is configured; otherwise, <c>false</c>.</value>
        /// <remarks></remarks>
        public virtual bool IsConfigured { get; protected set; }

        /// <summary>
        /// Gets supported mappings which can only be declared dynamically at runtime
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract IEnumerable<TypeMapperMetadata> GetDynamicSupportedMappings();

       

        /// <summary>
        /// Maps the specified source to an instance of type <typeparamref name="TDestination"/>, and runs a delegate <paramref name="afterMapIfNotNull"/> 
        /// before returning if the mapping operation does not result in a null value.
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="afterMapIfNotNull">The after map if not null.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public TDestination Map<TDestination>(object source, Action<TDestination> afterMapIfNotNull)
        {
            var output = Map<TDestination>(source);
            if (!ReferenceEquals(output, null)) afterMapIfNotNull.Invoke(output);
            return output;
        }

        /// <summary>
        /// Maps <paramref name="source"/> to <paramref name="destination"/> by trying to find a map configuration using the precise type of the 
        /// provided instances, rather than explicit typing.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <remarks></remarks>
        public void WeakMap(object source, object destination)
        {
            Mandate.ParameterNotNull(source, "source");
            Mandate.ParameterNotNull(destination, "destination");

            Map(source, destination, source.GetType(), destination.GetType());
        }

        /// <summary>
        /// Tries to find a destination type given the <paramref name="sourceType"/>.
        /// </summary>
        /// <param name="sourceType">The source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool TryGetDestinationType(Type sourceType, out Type destinationType);

        /// <summary>
        /// Tries to find a destination type given the <paramref name="sourceType"/> and a target type that is equal to or inherits from <paramref name="baseDestinationType"/>.
        /// </summary>
        /// <param name="sourceType">The source.</param>
        /// <param name="baseDestinationType">Base type of the destination.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool TryGetDestinationType(Type sourceType, Type baseDestinationType, out Type destinationType);

        /// <summary>
        /// Finds a map handler that can map from the type of <paramref name="source"/>, to a target type that is equal to or inherits from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public T MapToIntent<T>(object source) where T : class
        {
            Mandate.ParameterNotNull(source, "source");

            return (T)MapToIntent(source, typeof(T));
        }

        /// <summary>
        /// Finds a map handler that can map from the type of <paramref name="source"/>, to a target type that is equal to or inherits from <paramref name="baseDestinationType"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="baseDestinationType">Base type of the destination.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object MapToIntent(object source, Type baseDestinationType)
        {
            Mandate.ParameterNotNull(source, "source");

            var sourceType = source.GetType();

            Type destinationType;
            if (!TryGetDestinationType(sourceType, baseDestinationType, out destinationType))
                throw new InvalidOperationException(
                    string.Format("Cannot find a destination type for the source type {0} using this mapper", sourceType.FullName));

            return Map(source, sourceType, destinationType);
        }

        /// <summary>
        /// Tries to find a destination type given the <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool TryGetDestinationType<TSource>(out Type destinationType)
        {
            return TryGetDestinationType(typeof (TSource), out destinationType);
        }

        /// <summary>
        /// Tries to find a destination type given the <typeparamref name="TSource"/>, and a target type that is equal to or inherits from <typeparamref name="TBaseOfDestination"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TBaseOfDestination">The type of the base of destination.</typeparam>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool TryGetDestinationType<TSource, TBaseOfDestination>(out Type destinationType)
        {
            return TryGetDestinationType(typeof(TSource), typeof(TBaseOfDestination), out destinationType);
        }
    }
}