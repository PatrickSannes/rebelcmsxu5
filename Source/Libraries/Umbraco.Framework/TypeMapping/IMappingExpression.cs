using System;
using System.Linq.Expressions;

namespace Umbraco.Framework.TypeMapping
{

    public interface IMappingExpression<TSource, TTarget>
    {

        FluentMappingEngineContext<TSource, TTarget> MappingContext { get; }

        /// <summary>
        /// Defines a delegate to execute after the mapping
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="afterMap">The after map.</param>
        /// <returns></returns>
        IMappingExpression<TSource, TTarget> AfterMap(Action<TSource, TTarget> afterMap);

        /// <summary>
        /// Defines a delegate to create the target
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="createUsing">The create using.</param>
        /// <returns></returns>
        IMappingExpression<TSource, TTarget> CreateUsing(Func<TSource, TTarget> createUsing);

        /// <summary>
        /// Defines an operation for a member
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="destinationMember"></param>
        /// <param name="memberOptions"></param>
        /// <returns></returns>
        IMappingExpression<TSource, TTarget> ForMember<TProperty>(
            Expression<Func<TTarget, TProperty>> destinationMember,
            Action<IMemberMappingExpression<TSource>> memberOptions);
    }
}