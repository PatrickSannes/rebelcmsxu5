using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// An object representing the scope of a single root mapping operation.
    /// Used to track mapped objects, ensuring if an incoming map request refers to an instance that
    /// has already been mapped, the existing destination instance is returned instead, to increase performance,
    /// and also to ensure the same referential integrity on the output of a map as existed in the incoming
    /// object graph at the start of the mapping operation.
    /// </summary>
    /// <remarks>Check that out for a long sentence.</remarks>
    public sealed class MappingExecutionScope
    {
        private readonly AbstractFluentMappingEngine _engine;

        public MappingExecutionScope(AbstractFluentMappingEngine forMappingEngine)
        {
            _engine = forMappingEngine;
            ScopeId = Guid.NewGuid();
        }

        /// <summary>
        /// useful for debugging
        /// </summary>
        public Guid ScopeId { get; private set; }

        private readonly HashSet<Tuple<object, object>> _mapCache = new HashSet<Tuple<object, object>>();

        public HashSet<Tuple<object, object>> MapCache { get { return _mapCache; } }

        public AbstractFluentMappingEngine Engine { get { return _engine; } }

        /// <summary>
        /// Checks with <see cref="MapCache"/>; if <paramref name="source"/> has already been mapped to <paramref name="targetType"/> then the original
        /// map result is returned. Otherwise, <paramref name="factory"/> is used to map the object to its target type.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        public object CreateOnce(object source, Type targetType, Func<object> factory)
        {
            var targetToAdd = _mapCache
                                  .Where(x => ReferenceEquals(x.Item1, source) && x.Item2.GetType() == targetType)
                                  .Select(x => x.Item2)
                                  .FirstOrDefault()
                              ??
                              factory.Invoke();

            if (!_mapCache.Any(x => ReferenceEquals(x.Item1, source)))
            {
                _mapCache.Add(new Tuple<object, object>(source, targetToAdd));
            }

            return targetToAdd;
        }

        public TTarget CreateOnce<TTarget>(object source, Func<TTarget> factory)
        {
            return (TTarget)CreateOnce(source, typeof (TTarget), () => factory.Invoke());
        }
    }
}