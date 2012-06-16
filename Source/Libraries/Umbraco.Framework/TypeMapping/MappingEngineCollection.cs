#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


#endregion

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Acts as a wrapper for a collection of <see cref="AbstractMappingEngine"/> based on a key of <see cref="TypeMapperMetadata"/> in order to 
    /// select the appropriate mapper for the requested return type.
    /// </summary>
    /// <remarks>TODO: Use a shortest path algorithm to allow chaining mappers together</remarks>
    public class MappingEngineCollection : AbstractMappingEngine
    {
        private readonly IList<Lazy<AbstractMappingEngine, TypeMapperMetadata>> _binders;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        /// <summary>
        ///   Initializes a new instance of the <see cref = "MappingEngineCollection" /> class.
        /// </summary>
        public MappingEngineCollection(IEnumerable<Lazy<AbstractMappingEngine, TypeMapperMetadata>> binders)
        {
            _binders = new List<Lazy<AbstractMappingEngine, TypeMapperMetadata>>(binders);
        }

        /// <summary>
        /// Gets an enumerable collection of the binders contained in this <see cref="MappingEngineCollection"/>.
        /// </summary>
        /// <remarks></remarks>
        public IEnumerable<Lazy<AbstractMappingEngine, TypeMapperMetadata>> Binders
        {
            get { return _binders; }
        }

        /// <summary>
        /// Adds the specified binder, calling <see cref="AbstractMappingEngine.Configure"/> beforehand.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <remarks></remarks>
        public void Add(Lazy<AbstractMappingEngine, TypeMapperMetadata> binder)
        {
            if (!binder.Value.IsConfigured) binder.Value.Configure();
            _binders.Add(binder);
        }

        /// <summary>
        /// Returns the engine that handles mapping from the source type to destination type, otherwise if none are found then it returns null.
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <param name="allowInheritedTypes">true to search mappers that deal with inherited tpes</param>
        /// <returns></returns>
        public AbstractMappingEngine GetMapHandler(Type sourceType, Type destinationType, bool allowInheritedTypes = true)
        {
            var handlerWithoutInheritance = GetTypeEqualHandler(sourceType, destinationType)
                .Select(x => x.Value).FirstOrDefault();
            
            if (handlerWithoutInheritance != null)
                return handlerWithoutInheritance;

            if (allowInheritedTypes)
            {
                return GetInheritedTypeHandler(sourceType, destinationType).
                    Select(x => x.Value).FirstOrDefault();
            }

            return null;
        }


        #region Overrides of AbstractTypeMapper

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <exception cref="InvalidOperationException">If no mapper is registered for the <paramref name="sourceType"/> and <paramref name="destinationType"/> mapping pair.</exception>
        /// <returns></returns>
        /// <remarks></remarks>
        public override object Map(object source, Type sourceType, Type destinationType)
        {
            // Get the first mapper which can go from source to destination
            // TODO: Use a shortest path algorithm to allow chaining mappers together
            AbstractMappingEngine mapHandler = GetMapHandler(sourceType, destinationType);

            if (mapHandler != null) return mapHandler.Map(source, sourceType, destinationType);

            // If we have no handler mapped, but the input/output types are the same, return the original
            // instance
            if (sourceType == destinationType)
                return source;

            throw new InvalidOperationException(
                String.Format("Cannot map from {0} to {1} - no handler registered with this TypeMapperCollection",
                              sourceType.FullName, destinationType.FullName));
        }


        /// <summary>
        /// Maps the specified source to an existing destination.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <exception cref="InvalidOperationException">If no mapper is registered for the <paramref name="sourceType"/> and <paramref name="destinationType"/> mapping pair.</exception>
        /// <remarks></remarks>
        public override void Map(object source, object destination, Type sourceType, Type destinationType)
        {
            // Get the first mapper which can go from source to destination
            // TODO: Use a shortest path algorithm to allow chaining mappers together
            AbstractMappingEngine mapHandler = GetMapHandler(sourceType, destinationType);

            if (mapHandler != null) 
                mapHandler.Map(source, destination, sourceType, destinationType);
            else
                throw new InvalidOperationException(
                    String.Format("Cannot map from {0} to {1} - no handler registered with this TypeMapperCollection",
                                  sourceType.FullName, destinationType.FullName));
        }


        /// <summary>
        /// Configures this instance.
        /// </summary>
        /// <remarks></remarks>
        public override void Configure()
        {
            using (new WriteLockDisposable(_locker))
            {
                if (IsConfigured) return;
                foreach (var binder in Binders)
                {
                    binder.Value.Configure();
                }
                IsConfigured = true;
            }
        }

        /// <summary>
        ///   Gets supported mappings which can only be declared dynamically at runtime
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public override IEnumerable<TypeMapperMetadata> GetDynamicSupportedMappings()
        {
            // TODO: Implement shortest path algorithm to advertise each edge in the available mapping graph
            return
                Binders
                    .SelectMany(x => x.Value.GetDynamicSupportedMappings())
                    .Union(Binders
                               .Select(x => x.Metadata))
                    .Distinct();
        }

        public override bool TryGetDestinationType(Type sourceType, out Type destinationType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");

            // First check binders for which we have metadata since that's the quickest
            var filtered = Binders
                .Where(x => x.Metadata.SourceType != null && x.Metadata.DestinationType != null)
                .Select(x => x.Metadata);

            destinationType = GuessDestinationType(sourceType, filtered);

            if (destinationType == null)
            {
                // Now try checking binders that advertise their capabilities dynamically
                var dynamicFilter = GetDynamicMappings(Binders);
                destinationType = GuessDestinationType(sourceType, dynamicFilter.Select(x => x.Metadata));
            }

            return destinationType != null;    
        }

        /// <summary>
        /// Tries to find a destination type given the <paramref name="sourceType"/> and a target type that is equal to or inherits from <paramref name="baseDestinationType"/>.
        /// </summary>
        /// <param name="sourceType">The source.</param>
        /// <param name="baseDestinationType">Base type of the destination.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool TryGetDestinationType(Type sourceType, Type baseDestinationType, out Type destinationType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");
            Mandate.ParameterNotNull(baseDestinationType, "baseDestinationType");

            // First check binders for which we have metadata since that's the quickest
            var filtered = Binders
                .Where(x => x.Metadata.SourceType != null && x.Metadata.DestinationType != null)
                .Select(x => x.Metadata);

            destinationType = FindDestinationType(sourceType, baseDestinationType, filtered);

            if (destinationType == null)
            {
                // Now try checking binders that advertise their capabilities dynamically
                var dynamicFilter = GetDynamicMappings(Binders);
                destinationType = FindDestinationType(sourceType, baseDestinationType, dynamicFilter.Select(x => x.Metadata));
            }

            return destinationType != null;    
        }

        
       

        #endregion

        private IEnumerable<Lazy<AbstractMappingEngine, TypeMapperMetadata>> GetInheritedTypeHandler(Type sourceType, Type destinationType)
        {
            // First check binders for which we have metadata
            var filtered = Binders
                .Where(x => x.Metadata.SourceType != null && x.Metadata.DestinationType != null);

            //NOTE: Pretty sure this check is backwards, i think the 2nd way is correct. SD.
            Func<Lazy<AbstractMappingEngine, TypeMapperMetadata>, bool> predicate = x =>
                             x.Metadata.PermitTypeInheritance && x.Metadata.SourceType.IsAssignableFrom(sourceType) &&
                             x.Metadata.DestinationType.IsAssignableFrom(destinationType);
            //Func<Lazy<AbstractTypeMapper, TypeMapperMetadata>, bool> predicate = x =>                
            //                 x.Metadata.PermitTypeInheritance && sourceType.IsAssignableFrom(x.Metadata.SourceType) &&
            //                 destinationType.IsAssignableFrom(x.Metadata.DestinationType);

            

            foreach (var lazy in filtered.Where(predicate))
            {
                yield return lazy;
            }

            // Now add dynamically returned metadata
            filtered = GetDynamicMappings(Binders);
            foreach (var lazy in filtered.Where(predicate))
            {
                yield return lazy;
            }
        }

        private static IEnumerable<Lazy<AbstractMappingEngine, TypeMapperMetadata>> GetDynamicMappings(IEnumerable<Lazy<AbstractMappingEngine, TypeMapperMetadata>> binders)
        {
            foreach (var binder in binders.Where(x => x.Metadata.MetadataGeneratedByMapper))
            {
                var localCopyOfBinder = binder;
                Func<AbstractMappingEngine> binderToReturn = () => localCopyOfBinder.Value;
                foreach (var dynamicSupportedMapping in localCopyOfBinder.Value.GetDynamicSupportedMappings())
                {
                    yield return new Lazy<AbstractMappingEngine, TypeMapperMetadata>(binderToReturn, dynamicSupportedMapping);
                }
            }
        }

        private static Type GuessDestinationType(Type sourceType, IEnumerable<TypeMapperMetadata> metadata)
        {
            return metadata
                       .Where(x => x.SourceType.Equals(sourceType))
                       .Select(x => x.DestinationType).FirstOrDefault() ??
                   metadata
                       .Where(x => x.PermitTypeInheritance && x.SourceType.IsAssignableFrom(sourceType))
                       .Select(x => x.DestinationType).FirstOrDefault() ??
                   metadata
                       .Where(x => x.DestinationType.Equals(sourceType))
                       .Select(x => x.SourceType).FirstOrDefault() ??
                   metadata
                       .Where(x => x.PermitTypeInheritance && x.DestinationType.IsAssignableFrom(sourceType))
                       .Select(x => x.SourceType).FirstOrDefault();
        }

        private static Type FindDestinationType(Type sourceType, Type baseDestinationType, IEnumerable<TypeMapperMetadata> metadata)
        {
            return metadata
                       .Where(x => x.SourceType.Equals(sourceType) && baseDestinationType.IsAssignableFrom(x.DestinationType))
                       .Select(x => x.DestinationType).FirstOrDefault() ??
                   metadata
                       .Where(x => x.PermitTypeInheritance && x.SourceType.IsAssignableFrom(sourceType) && baseDestinationType.IsAssignableFrom(x.DestinationType))
                       .Select(x => x.DestinationType).FirstOrDefault() ??
                   metadata
                       .Where(x => x.DestinationType.Equals(sourceType) && baseDestinationType.IsAssignableFrom(x.SourceType))
                       .Select(x => x.SourceType).FirstOrDefault() ??
                   metadata
                       .Where(x => x.PermitTypeInheritance && x.DestinationType.IsAssignableFrom(sourceType) && baseDestinationType.IsAssignableFrom(x.SourceType))
                       .Select(x => x.SourceType).FirstOrDefault();
        }

        private IEnumerable<Lazy<AbstractMappingEngine, TypeMapperMetadata>> GetTypeEqualHandler(Type sourceType, Type destinationType)
        {
            // First check binders for which we have metadata
            var filtered = Binders
                .Where(x => x.Metadata.SourceType != null && x.Metadata.DestinationType != null);

            Func<Lazy<AbstractMappingEngine, TypeMapperMetadata>, bool> predicate = x => x.Metadata.SourceType.Equals(sourceType) && x.Metadata.DestinationType.Equals(destinationType);
            foreach (var lazy in filtered.Where(predicate))
            {
                yield return lazy;
            }

            // Now add dynamically returned metadata
            filtered = GetDynamicMappings(Binders);
            foreach (var lazy in filtered.Where(predicate))
            {
                yield return lazy;
            }
        }

        
    }
}