using System.Collections.Generic;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework
{
    public static class MappingEngineExtensions
    {


        /// <summary>
        /// This creates an IEnumerable type mapping for the TS & TT types if neither are already IEnumerable
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="engine"></param>
        private static void CreateEnumerableMap<TS, TT>(this AbstractFluentMappingEngine engine)
        {
            //if niether are enumerable, we can create an Enumerable type map for them with the EnumerableTypeMapper
            if (!typeof(TS).IsEnumerable() && !typeof(TT).IsEnumerable())
            {
                var metadata = new TypeMapperMetadata(typeof (IEnumerable<TS>), typeof (IEnumerable<TT>), false);
                var mapper = new EnumerableTypeMapper<IEnumerable<TS>, IEnumerable<TT>>(engine);
                engine.CreateMap(mapper, metadata);
            }

        }

        /// <summary>
        /// Maps a type to itself. This is used to clone an object.
        /// </summary>
        /// <typeparam name = "TSource"></typeparam>
        /// <returns></returns>
        public static IMappingExpression<TSource, TSource> SelfMap<TSource>(this AbstractFluentMappingEngine engine)
        {
            return engine.CreateMap<TSource, TSource>();
        }

        /// <summary>
        /// Maps a type to itself. This is used to clone an object.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="engine"></param>
        /// <param name="permitInheritance"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TSource> SelfMap<TSource>(this AbstractFluentMappingEngine engine, bool permitInheritance)
        {
            return engine.CreateMap<TSource, TSource>(permitInheritance);
        }


        /// <summary>
        /// Creates a new mapper with the specified mapper and default metadata
        /// </summary>
        /// <typeparam name="TS">The type of the S.</typeparam>
        /// <typeparam name="TT">The type of the T.</typeparam>
        /// <param name="engine"></param>
        /// <param name="mapper"></param>
        public static IMappingExpression<TS, TT> CreateMap<TS, TT>(this AbstractFluentMappingEngine engine, ITypeMapper<TS, TT> mapper)
        {
            //create the standard map
            var metadata = new TypeMapperMetadata(typeof (TS), typeof (TT), false);
            engine.AddMap(mapper, metadata);

            //create enumerable type map for the types automatically if they aren't already IEnumerable
            engine.CreateEnumerableMap<TS, TT>();

            //return the mapping expression for the standard map
            var expression = new MappingExpression<TS, TT>(mapper.MappingContext);
            return expression;
        }

        /// <summary>
        /// Creates a new mapper with the specified mapper and default metadata with the permitInheritance flag
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="engine"></param>
        /// <param name="mapper"></param>
        /// <param name="permitInheritance"></param>
        /// <returns></returns>
        public static IMappingExpression<TS, TT> CreateMap<TS, TT>(this AbstractFluentMappingEngine engine, ITypeMapper<TS, TT> mapper, bool permitInheritance)
        {
            var metadata = new TypeMapperMetadata(typeof(TS), typeof(TT), permitInheritance);
            engine.AddMap(mapper, metadata);

            //create enumerable type map for the types automatically if they aren't already IEnumerable
            engine.CreateEnumerableMap<TS, TT>();

            var expression = new MappingExpression<TS, TT>(mapper.MappingContext);
            return expression;
        }

        /// <summary>
        /// Creates a map with the specified mapper & metadata
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="engine"></param>
        /// <param name="mapper"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IMappingExpression<TS, TT> CreateMap<TS, TT>(this AbstractFluentMappingEngine engine, ITypeMapper<TS, TT> mapper, TypeMapperMetadata metadata)
        {
            engine.AddMap(mapper, metadata);

            //create enumerable type map for the types automatically if they aren't already IEnumerable
            engine.CreateEnumerableMap<TS, TT>();

            var expression = new MappingExpression<TS, TT>(mapper.MappingContext);
            return expression;
        }

        /// <summary>
        /// Creates a new map with the default metadata and default mapper
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="engine"></param>
        /// <param name="permitInheritance"></param>
        /// <returns></returns>
        public static IMappingExpression<TS, TT> CreateMap<TS, TT>(this AbstractFluentMappingEngine engine, bool permitInheritance)
        {
            return engine.CreateMap<TS, TT>(new TypeMapperMetadata(typeof (TS), typeof (TT), permitInheritance));
        }

        /// <summary>
        /// Creates a new map with the specified metadata and default mapper
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="engine"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static IMappingExpression<TS, TT> CreateMap<TS, TT>(this AbstractFluentMappingEngine engine, TypeMapperMetadata metadata)
        {
            //first we need to check if it's enumerable, if so we need to create an Enumerable injector mapper
            if (typeof(TS).IsEnumerable() && typeof(TT).IsEnumerable())
            {
                var mapper = new EnumerableTypeMapper<TS, TT>(engine);
                return engine.CreateMap(mapper, metadata);
            }
            else
            {
                var mapper = new TypeMapper<TS, TT>(engine);
                return engine.CreateMap(mapper, metadata);
            }
        }

        /// <summary>
        /// Creates a new map with the default mapper and metadata
        /// </summary>
        /// <typeparam name="TS">The type of the S.</typeparam>
        /// <typeparam name="TT">The type of the T.</typeparam>
        /// <param name="engine">The engine.</param>
        /// <returns></returns>
        public static IMappingExpression<TS, TT> CreateMap<TS, TT>(this AbstractFluentMappingEngine engine)
        {
            return engine.CreateMap<TS, TT>(new TypeMapperMetadata(typeof (TS), typeof (TT), false));
        }
    }
}