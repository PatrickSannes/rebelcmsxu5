using System;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Interface to describe mapping a type from one type to another
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    public interface ITypeMapper<TSource, TTarget> : ITypeMapper
    {
        TTarget Map(TSource source);
        void Map(TSource source, TTarget target);

        FluentMappingEngineContext<TSource, TTarget> MappingContext { get; }
        TTarget GetTargetFromScope(TSource source, MappingExecutionScope scope);
        TTarget Map(TSource source, MappingExecutionScope scope);
    }

    /// <summary>
    /// Basic interface to handle mapping two types
    /// </summary>
    /// <remarks>
    /// Used internally in the mapping framework when strongly typed entities are not available.
    /// </remarks>
    public interface ITypeMapper
    {
        Type TargetType { get; }
        Type SourceType { get; }
        void Map(object source, object target);
        object Map(object source);
        object GetTargetFromScope(object source, Type targetType, MappingExecutionScope scope);
        object Map(object source, MappingExecutionScope scope);
    }
}