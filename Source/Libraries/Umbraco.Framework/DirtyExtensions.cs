using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Umbraco.Framework
{
    /// <summary>
    /// Extension methods for ICanBeDirty interface
    /// </summary>
    public static class DirtyExtensions
    {

        /// <summary>
        /// Sets the property value on the destination to the source property if the source property is dirty
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <typeparam name="TDestinationProperty"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TSourceProperty"></typeparam>
        /// <param name="destination"></param>
        /// <param name="prop"></param>
        /// <param name="source"></param>
        /// <param name="dirtyProp"></param>
        /// <returns></returns>
        public static bool TrySetPropertyFromDirty<TDestination, TDestinationProperty, TSource, TSourceProperty>(this TDestination destination,
                                                                                                                 Expression<Func<TDestination, TDestinationProperty>> prop,
                                                                                                                 TSource source,
                                                                                                                 Expression<Func<TSource, TSourceProperty>> dirtyProp)
            where TDestination : class
            where TSource : class, ICanBeDirty
        {
            var propToGet = source.GetPropertyInfo(dirtyProp);
            return TrySetPropertyFromDirty(destination, prop, source, dirtyProp, () => (TDestinationProperty)propToGet.GetValue(source, null));
        }

        /// <summary>
        /// Sets the property value on the destination to the source property if the source property is dirty
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <typeparam name="TDestinationProperty"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TSourceProperty"></typeparam>
        /// <param name="destination"></param>
        /// <param name="prop"></param>
        /// <param name="source"></param>
        /// <param name="dirtyProp"></param>
        /// <param name="valueGetter"></param>
        /// <returns></returns>
        public static bool TrySetPropertyFromDirty<TDestination, TDestinationProperty, TSource, TSourceProperty>(this TDestination destination,
                                                                                                                 Expression<Func<TDestination, TDestinationProperty>> prop,
                                                                                                                 TSource source,
                                                                                                                 Expression<Func<TSource, TSourceProperty>> dirtyProp,
                                                                                                                 Func<TDestinationProperty> valueGetter)
            where TDestination : class
            where TSource : class, ICanBeDirty
        {
            var propToSet = destination.GetPropertyInfo(prop);
            if (source.IsPropertyDirty(dirtyProp))
            {
                propToSet.SetValue(destination, valueGetter(), null);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a property is Dirty based on a property selector
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="e"></param>
        /// <param name="propSelector"></param>
        /// <returns></returns>
        public static bool IsPropertyDirty<TSource, TProperty>(this TSource e, Expression<Func<TSource, TProperty>> propSelector)
            where TSource : class, ICanBeDirty
        {
            var propInfo = e.GetPropertyInfo(propSelector);
            return e.IsPropertyDirty(propInfo.Name);
        }

        /// <summary>
        /// Sets all properties from the source to the destination object for any property that has changed (IsDirty) on the source 
        /// object. If any of the properties are indexed properties, they will not be set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<object, PropertyInfo>> SetAllChangedProperties<T, TProperty>(this T destination, T source, params Expression<Func<T, TProperty>>[] ignoreProperties)
            where T : ICanBeDirty
        {
            var changed = new List<Tuple<object, PropertyInfo>>();
            var props = typeof(T).GetProperties();
            var ignore = ignoreProperties.Select(e => source.GetPropertyInfo(e)).Select(propInfo => propInfo.Name).ToArray();
            foreach (var p in props.Where(x => !x.GetIndexParameters().Any() && !ignore.Contains(x.Name)))
            {
                if (source.IsPropertyDirty(p.Name))
                {
                    var val = p.GetValue(source, null);
                    p.SetValue(destination, p.GetValue(source, null), null);
                    changed.Add(new Tuple<object, PropertyInfo>(val, p));
                }
            }
            return changed;
        }

    }
}