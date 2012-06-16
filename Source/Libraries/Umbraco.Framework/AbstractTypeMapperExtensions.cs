using System;
using System.Linq;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework
{
    /// <summary>
    /// Extension methods for the AbstractTypemapper
    /// </summary>
    public static class AbstractTypeMapperExtensions
    {
        /// <summary>
        /// Maps the specified mapper with an after map "if null" delegate
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="source">The source.</param>
        /// <param name="afterMapIfNotNull">The after map if not null.</param>
        /// <returns></returns>
        public static TDestination Map<TSource, TDestination>(this AbstractMappingEngine mapper, TSource source, Action<TDestination> afterMapIfNotNull)
        {
            var output = mapper.Map<TSource, TDestination>(source);
            if (!ReferenceEquals(output, null)) afterMapIfNotNull.Invoke(output);
            return output;
        }

        /// <summary>
        /// Tries to get destination type from dynamic supported mappings.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        public static bool TryGetDestinationTypeFromDynamicSupportedMappings(this AbstractMappingEngine mapper, Type sourceType, out Type destinationType)
        {
            var mappings = mapper.GetDynamicSupportedMappings();
            destinationType = mappings.Where(x => x.SourceType == sourceType).Select(x => x.DestinationType).FirstOrDefault();
            return destinationType != null;
        }

        /// <summary>
        /// Tries to get destination type from dynamic supported mappings.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="baseDestinationType">Type of the base destination.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        public static bool TryGetDestinationTypeFromDynamicSupportedMappings(this AbstractMappingEngine mapper, Type sourceType, Type baseDestinationType, out Type destinationType)
        {
            var mappings = mapper.GetDynamicSupportedMappings();
            destinationType = mappings
                .Where(x => x.SourceType == sourceType && baseDestinationType.IsAssignableFrom(baseDestinationType))
                .Select(x => x.DestinationType).FirstOrDefault();
            return destinationType != null;
        }
    }
}