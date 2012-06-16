using System;
using System.Collections.Generic;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Maintains a list of delegates which can map from one type to another, given a key of type <see cref="TypeMapperMetadata"/> which describes the 
    /// source and destination types of the delegate.
    /// </summary>
    /// <remarks></remarks>
    public class DelegatedMapHandlerList : List<KeyValuePair<TypeMapperMetadata, TypeMapDelegatePair>>
    {
        private void Add<TSource, TDestination>(TypeMapperMetadata typeMapperMetadata, Func<TSource, AbstractLookupHelper, AbstractMappingEngine, TDestination> handler, Action<TSource, TDestination, AbstractLookupHelper, AbstractMappingEngine> refHandler)
        {
            Func<object, AbstractLookupHelper, AbstractMappingEngine, object> castExpression = (source, lookup, masterMapper) => handler((TSource)source, lookup, masterMapper);
            Action<object, object, AbstractLookupHelper, AbstractMappingEngine> castRefExpression = (source, dest, lookup, masterMapper) => refHandler.Invoke((TSource)source, (TDestination)dest, lookup, masterMapper);
            var keyValuePair = new KeyValuePair<TypeMapperMetadata, TypeMapDelegatePair>(typeMapperMetadata, new TypeMapDelegatePair(castExpression, castRefExpression));
            Add(keyValuePair);
        }

        ///// <summary>
        ///// Adds the specified handler.
        ///// </summary>
        ///// <typeparam name="TSource">The type of the source.</typeparam>
        ///// <typeparam name="TDestination">The type of the destination.</typeparam>
        ///// <param name="handler">The handler.</param>
        ///// <param name="refHandler">The ref handler.</param>
        ///// <param name="permitTypeInheritance">if set to <c>true</c> [permit type inheritance].</param>
        ///// <remarks></remarks>
        //public void Add<TSource, TDestination>(Func<TSource, TDestination> handler, Action<TSource, TDestination> refHandler, bool permitTypeInheritance)
        //{
        //    Func<object, AbstractLookupHelper, AbstractTypeMapper, object> castExpression = (source, lookup, masterMapper) => handler((TSource)source);
        //    Action<object, object, AbstractLookupHelper, AbstractTypeMapper> castRefExpression = (source, dest, lookup, masterMapper) => refHandler.Invoke((TSource)source, (TDestination)dest);

        //    Add(new TypeMapperMetadata(typeof(TSource), typeof(TDestination), permitTypeInheritance), castExpression, castRefExpression);
        //}

        /// <summary>
        /// Adds the specified handler.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <param name="refHandler">The ref handler.</param>
        /// <param name="permitTypeInheritance">if set to <c>true</c> [permit type inheritance].</param>
        /// <remarks></remarks>
        public void Add<TSource, TDestination>(Func<TSource, AbstractLookupHelper, AbstractMappingEngine, TDestination> handler, Action<TSource, TDestination, AbstractLookupHelper, AbstractMappingEngine> refHandler, bool permitTypeInheritance)
        {
            Add(new TypeMapperMetadata(typeof(TSource), typeof(TDestination), permitTypeInheritance), handler, refHandler);
        }
    }
}