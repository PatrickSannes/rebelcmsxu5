using System;
using System.Linq.Expressions;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// A class to aid in the Fluent construct of mapping expressions
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <remarks>
    /// This class is used to create the fluent mapping expressions, for each method called it adds 
    /// the expressions to the underyling MappingContext.
    /// </remarks>
    public class MappingExpression<TSource, TTarget> : IMappingExpression<TSource, TTarget>
    {
        public FluentMappingEngineContext<TSource, TTarget> MappingContext { get; private set; }

        public MappingExpression(FluentMappingEngineContext<TSource, TTarget> mappingContext)
        {
            MappingContext = mappingContext;
        }

        /// <summary>
        /// Defines a delegate to execute after the mapping
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="afterMap">The after map.</param>
        /// <returns></returns>
        public IMappingExpression<TSource, TTarget> AfterMap(Action<TSource, TTarget> afterMap)
        {
            if (MappingContext.AfterMapAction != null)
            {
                throw new InvalidOperationException("AfterMap can only be specified once for a mapping operation");
            }

            MappingContext.AfterMapAction = afterMap;
            
            return this;
        }

        /// <summary>
        /// Defines a delegate to create the target
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="createUsing">The create using.</param>
        /// <returns></returns>
        public IMappingExpression<TSource, TTarget> CreateUsing(Func<TSource, TTarget> createUsing)
        {
            if (MappingContext.CreateUsingAction != null)
            {
                throw new InvalidOperationException("CreateUsing can only be specified once for a mapping operation");
            }

            MappingContext.CreateUsingAction = createUsing;
            return this;
        }

        /// <summary>
        /// Defines an operation for a member
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="destinationMember"></param>
        /// <param name="memberOptions"></param>
        /// <returns></returns>
        public IMappingExpression<TSource, TTarget> ForMember<TProperty>(
            Expression<Func<TTarget, TProperty>> destinationMember,
            Action<IMemberMappingExpression<TSource>> memberOptions)
        {
            Mandate.That(destinationMember.Body is MemberExpression,
                         x => new NotSupportedException("ForMember can only be called on a Member/Field of an object"));

            MappingContext.AddMemberExpression(destinationMember, memberOptions);

            return this;
        }

    }
}