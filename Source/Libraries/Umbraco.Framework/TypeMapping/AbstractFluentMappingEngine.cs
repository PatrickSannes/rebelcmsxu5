using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Framework.Context;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// A mapping engine allowing for fluent mapping declarations
    /// </summary>
    public abstract class AbstractFluentMappingEngine : AbstractMappingEngine
    {
        public IFrameworkContext FrameworkContext { get; private set; }

        protected AbstractFluentMappingEngine(IFrameworkContext frameworkContext)
        {
            FrameworkContext = frameworkContext;
        }

        /// <summary>
        /// Tracks all of the mappers
        /// </summary>
        private readonly ConcurrentDictionary<TypeMapDefinition, MapperDefinition> _mappers = new ConcurrentDictionary<TypeMapDefinition, MapperDefinition>();

        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        /// <summary>
        /// Gets a mapper.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns></returns>
        internal MapperDefinition GetMapperDefinition<TSource, TTarget>()
        {
            return GetMapperDefinition(typeof(TSource), typeof(TTarget));
        }

        /// <summary>
        /// Gets a mapper.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="createIfNotExists">if set to <c>true</c> [create if not exists].</param>
        /// <returns></returns>
        internal MapperDefinition GetMapperDefinition<TSource, TTarget>(bool createIfNotExists)
        {
            return GetMapperDefinition(typeof(TSource), typeof(TTarget), createIfNotExists);
        }

        /// <summary>
        /// Gets a mapper, or creates a new implicit map if an explicity one is not defined
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        internal MapperDefinition GetMapperDefinition(Type source, Type target)
        {
            return GetMapperDefinition(source, target, true);
        }

        /// <summary>
        /// Gets a mapper, or creates a default mapper if one hasn't been defined if createIfNotExists is true
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="createIfNotExists">Creates a default mapper if one hasn't been defined</param>
        /// <returns></returns>
        internal MapperDefinition GetMapperDefinition(Type source, Type target, bool createIfNotExists)
        {
            var tryGetMapper = TryGetMapper(source, target, createIfNotExists);
            if (!tryGetMapper.Success)
            {
                throw new NotSupportedException("No mapper found to map from " + source.Name + " to " + target.Name);
            }
            return tryGetMapper.Result;
        }

        internal AttemptTuple<MapperDefinition> TryGetMapper(Type source, Type target, bool createIfNonExistant)
        {

            Func<AttemptTuple<MapperDefinition>> findTypeMapper = () =>
                {
                    //if we have a specified TypeMapper for <TSource,Target>
                    MapperDefinition mapper;
                    if (_mappers.TryGetValue(new TypeMapDefinition(source, target), out mapper))
                    {
                        return new AttemptTuple<MapperDefinition>(true, mapper);
                    }


                    //now, check if we have the specified mapper using type inheritance            
                    var found = _mappers.Where(x => x.Value.Metadata.PermitTypeInheritance && x.Value.Metadata.SourceType.IsAssignableFrom(source)
                                                     && x.Value.Metadata.DestinationType.IsAssignableFrom(target)).ToArray();

                    if (found.Any())
                    {
                        return new AttemptTuple<MapperDefinition>(true, found.Single().Value);
                    }
                    return AttemptTuple<MapperDefinition>.False;
                };

            var result = findTypeMapper();
            if (result.Success)
            {
                return result;
            }

            //if we've got this far, then create a new default map and add it to our mappers
            if (createIfNonExistant)
            {
                //need to lock as there could be many threads trying to do the same thing with the same types
                using (new WriteLockDisposable(_locker))
                {
                    //we'll now actually need to re-check if it exists
                    result = findTypeMapper();
                    if (result.Success)
                    {
                        return result;
                    }

                    //if both Source and Target types are Enumerables return new EnumerableTypeMapper<TSource,TTarget>()
                    if (source.IsEnumerable() && target.IsEnumerable())
                    {
                        var mapper = (ITypeMapper)
                               Activator.CreateInstance(typeof(EnumerableTypeMapper<,>).MakeGenericType(source, target)
                                                        , this); //pass this factory into the constructor
                        //create the map and store it against our list
                        var meta = new TypeMapperMetadata(source, target, false);
                        var mapperDef = new MapperDefinition(mapper, meta);
                        return new AttemptTuple<MapperDefinition>(true,
                            _mappers.GetOrAdd(new TypeMapDefinition(source, target), mapperDef));
                    }
                    else
                    {
                        //if not enumerable, just create a normal map
                        var concreteTypeMapper = typeof(TypeMapper<,>).MakeGenericType(new[] { source, target });
                        var mapper = (ITypeMapper)Creator.Create(concreteTypeMapper, this);
                        var defaultMeta = new TypeMapperMetadata(source, target, false);
                        var defaultMapperDef = new MapperDefinition(mapper, defaultMeta);
                        return new AttemptTuple<MapperDefinition>(true,
                            _mappers.GetOrAdd(new TypeMapDefinition(source, target), defaultMapperDef));
                    }
                }
            }

            return AttemptTuple<MapperDefinition>.False;
        }

        /// <summary>
        /// Adds a map
        /// </summary>
        public void AddMap(ITypeMapper mapper, TypeMapperMetadata metadata)
        {
            if (!_mappers.TryAdd(new TypeMapDefinition(metadata.SourceType, metadata.DestinationType), new MapperDefinition(mapper, metadata)))
            {
                throw new InvalidOperationException("Cannot add more than one mapping for the types " + metadata.SourceType.Name + ", " + metadata.DestinationType.Name);
            }
        }

        /// <summary>
        /// Clears the mappers collection, used internall for testing
        /// </summary>
        internal void ClearMappers()
        {
            _mappers.Clear();
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public override TDestination Map<TSource, TDestination>(TSource source)
        {
            return Map<TSource, TDestination>(source, new MappingExecutionScope(this));
        }

        /// <summary>
        /// Maps from a source to a new destination
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source, MappingExecutionScope scope)
        {
            if ((object)source == typeof(TSource).GetDefaultValue())
                return default(TDestination);

            var mapperDefinition = GetMapperDefinition<TSource, TDestination>();
            return (TDestination)mapperDefinition.Mapper.Map(source, scope);
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public override void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            if ((object)source == typeof(TSource).GetDefaultValue())
                return;

            var mapperDefinition = GetMapperDefinition<TSource, TDestination>();
            mapperDefinition.Mapper.Map(source, destination);
        }

        /// <summary>
        /// Maps the specified source to a new instance, given an explicitly defined source <paramref name="sourceType"/> and destination <paramref name="destinationType"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        public override object Map(object source, Type sourceType, Type destinationType)
        {
            return Map(source, sourceType, destinationType, new MappingExecutionScope(this));
        }

        /// <summary>
        /// Maps an existing object to a new object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        public object Map(object source, Type sourceType, Type destinationType, MappingExecutionScope scope)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");
            Mandate.ParameterNotNull(destinationType, "destinationType");

            if (source == (sourceType.GetDefaultValue()))
                return sourceType.GetDefaultValue();

            var mapperDefinition = GetMapperDefinition(sourceType, destinationType);

            //here we need to check if the mapper definition supports inheritance, and if so, we 
            //then need to check if the destination type is a subclass type of the destination 
            //type specified in the metadata. If it is, all we can do is try to instantiate a new
            //destination type and then try mapping to the existin object
            if (mapperDefinition.Metadata.PermitTypeInheritance)
            {
                return MapToSubclass(source, sourceType, mapperDefinition.Metadata.DestinationType, destinationType, mapperDefinition, scope);
            }

            return mapperDefinition.Mapper.Map(source, scope);
        }

        /// <summary>
        /// Maps the source object and source object type to the destinationSubclassType based on a mapper that supports inheritance 
        /// with a destination type specified as the destinationBaseclassType.
        /// </summary>
        /// <remarks>
        /// An example of this is if a map was defined to go from an Examine SearchResult to a TypedEntity with the allow inheritance flag
        /// set to true and then a mapping operation occured from a SearchResult to a User (a subclass of TypedEntity).
        /// 
        /// This method will first check to see if there is a mapping operation declared to go between the baseclass to the subclass, if so
        /// it will perform the normal mapping from the source object to the destinationBaseclassType, and then map from the base class type
        /// to the sub class type.
        /// 
        /// If this method doesn't find a map to go from the base class type to the sub class type, it will attempt to instantiate a new
        /// instance of the subclass and then do a map to the new existing object using the mapper already defined.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationBaseclassType"></param>
        /// <param name="destinationSubclassType"></param>
        /// <param name="mapperDefinition"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        protected virtual object MapToSubclass(object source, Type sourceType, Type destinationBaseclassType, Type destinationSubclassType, MapperDefinition mapperDefinition, MappingExecutionScope scope)
        {
            if (TypeFinder.IsTypeAssignableFrom(destinationSubclassType, destinationBaseclassType))
            {
                //if its assignable, just map it
                return mapperDefinition.Mapper.Map(source, scope);
            }
            if (destinationSubclassType.IsSubclassOf(destinationBaseclassType))
            {
                //we need to see if there's a mapping registered to go from the base class to the sub class, 
                //if so, we'll do the normal mapping to the base class, and then map again to the sub class.
                var subclassTypeMapDef = TryGetMapper(destinationBaseclassType, destinationSubclassType, false);
                if (subclassTypeMapDef.Success)
                {
                    //there's actually a map declared to go from the base class to subclass type, so well first
                    //map to the base class type from the source, then from the base class type to the sub class type.
                    var baseTypeMapped = mapperDefinition.Mapper.Map(source);
                    return subclassTypeMapDef.Result.Mapper.Map(baseTypeMapped, scope);
                }

                //there's no map declared so we need to now try to create the 'subclass' type and then map to that existing object
                try
                {
                    var mapTo = Activator.CreateInstance(destinationSubclassType);
                    Map(source, mapTo, sourceType, destinationSubclassType);
                    return mapTo;
                }
                catch (MissingMethodException ex)
                {
                    throw new InvalidOperationException(
                        string.Format("Could not map from {0} to {1} because {1} does not have a parameterless constructor", sourceType.Name, destinationSubclassType.Name), ex);
                }

            }
            throw new InvalidCastException("Cannot map from type " + sourceType + " to subclass type " + destinationSubclassType + " from the map specified for the baseclass type " + destinationBaseclassType);
        }

        /// <summary>
        /// Maps the specified source onto an existing instance <paramref name="destination"/>, given an explicitly defined source <paramref name="sourceType"/> and destination <paramref name="destinationType"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        public override void Map(object source, object destination, Type sourceType, Type destinationType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");
            Mandate.ParameterNotNull(destinationType, "destinationType");

            if (source == sourceType.GetDefaultValue())
                return;

            destination = destination ?? Creator.Create(destinationType);

            var mapperDefinition = GetMapperDefinition(sourceType, destinationType);
            mapperDefinition.Mapper.Map(source, destination);
        }

        /// <summary>
        /// Sealed method which is called by the TypeMapper to configure mappings, this method will check if it is configured
        /// and if not call the abstract ConfigureMappings method, then set the IsConfigured flag to true
        /// </summary>
        public sealed override void Configure()
        {
            if (IsConfigured) return;

            ConfigureMappings();

            IsConfigured = true;
        }

        /// <summary>
        /// Abstract method used to configure all of the mappings
        /// </summary>
        public abstract void ConfigureMappings();

        public override IEnumerable<TypeMapperMetadata> GetDynamicSupportedMappings()
        {
            return _mappers.Select(x => x.Value.Metadata);
        }

        public override bool TryGetDestinationType(Type sourceType, out Type destinationType)
        {
            return this.TryGetDestinationTypeFromDynamicSupportedMappings(sourceType, out destinationType);
        }

        public override bool TryGetDestinationType(Type sourceType, Type baseDestinationType, out Type destinationType)
        {
            return this.TryGetDestinationTypeFromDynamicSupportedMappings(sourceType, baseDestinationType, out destinationType);
        }
    }
}