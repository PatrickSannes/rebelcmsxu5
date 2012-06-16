using System;
using System.Diagnostics;
using Umbraco.Framework.Context;

namespace Umbraco.Framework.TypeMapping
{
    [DebuggerDisplay("Mapper: {SourceType} -> {TargetType} (Id: {MapperId})")]   
    public abstract class AbstractTypeMapper<TSource, TTarget> : ITypeMapper<TSource, TTarget>
    {
        protected AbstractTypeMapper(AbstractFluentMappingEngine engine)
        {
            //create a new context
            MappingContext = new FluentMappingEngineContext<TSource, TTarget>(engine);
            MapperId = Guid.NewGuid();
        }

        /// <summary>
        /// Gets or sets the mapping context.
        /// </summary>
        /// <value>The mapping context.</value>
        public FluentMappingEngineContext<TSource, TTarget> MappingContext { get; private set; }

        /// <summary>
        /// Useful for debugging
        /// </summary>
        internal Guid MapperId { get; private set; }

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>The type of the target.</value>
        public Type TargetType
        {
            get { return typeof(TTarget); }
        }

        /// <summary>
        /// Gets the type of the source.
        /// </summary>
        /// <value>The type of the source.</value>
        public Type SourceType
        {
            get { return typeof(TSource); }
        }

        public void Map(object source, object target)
        {
            Map((TSource)source, (TTarget)target);
        }

        public object Map(object source)
        {
            return Map((TSource)source);
        }

        public object GetTargetFromScope(object source, Type targetType, MappingExecutionScope scope)
        {
            return GetTargetFromScope((TSource)source, scope);
        }

        public object Map(object source, MappingExecutionScope scope)
        {
            return Map((TSource) source, scope);
        }

        /// <summary>
        /// Maps the specified source to a new destination object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public TTarget Map(TSource source)
        {
            return Map(source, new MappingExecutionScope(MappingContext.Engine));
        }

        public virtual TTarget GetTargetFromScope(TSource source, MappingExecutionScope scope)
        {
            return scope.CreateOnce(
                source,
                () => MappingContext.CreateUsingAction == null
                          ? (TTarget) Creator.Create(typeof (TTarget)) //try to just create it with no params
                          : MappingContext.CreateUsingAction(source));
        }

        /// <summary>
        /// Maps from the source to the new destination type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public virtual TTarget Map(TSource source, MappingExecutionScope scope)
        {
            var target = GetTargetFromScope(source, scope);
                
            Map(source, target, scope);

            return target;
        }

        /// <summary>
        /// Maps the specified to the destination
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public void Map(TSource source, TTarget target)
        {
            Map(source, target, new MappingExecutionScope(MappingContext.Engine));
        }

        /// <summary>
        /// Maps the source to the destination.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="scope"></param>
        /// <remarks>>
        /// </remarks>
        public void Map(TSource source, TTarget target, MappingExecutionScope scope)
        {
            PerformMap(source, target, scope);

            this.ExecuteAfterMap(source, target);
        }

        /// <summary>
        /// Virtual method to override for custom mapping operations
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="scope"></param>
        protected virtual void PerformMap(TSource source, TTarget target, MappingExecutionScope scope)
        {
            var mapper = new ObjectMapper<TSource, TTarget>(this);
            mapper.Map(source, target, scope);
        }

        /// <summary>
        /// Executes the after map.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        protected void ExecuteAfterMap(TSource source,TTarget target)
        {
            if (MappingContext.AfterMapAction != null)
            {
                MappingContext.AfterMapAction(source, target);
            }
        }
    }
}