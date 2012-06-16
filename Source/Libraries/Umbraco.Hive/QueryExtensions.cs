namespace Umbraco.Hive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Umbraco.Framework;
    using Umbraco.Framework.Diagnostics;
    using Umbraco.Framework.Expressions.Remotion;
    using Umbraco.Hive.Linq.Structure;

    public static class QueryExtensions
    {
        #region Public Methods

        /// <summary>
        /// Adds metadata to a query, to filter the <paramref name="source"/> based on the name of a revision type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="revisionType">Type of the revision.</param>
        /// <returns></returns>
        public static IQueryable<T> OfRevisionType<T>(this IQueryable<T> source, string revisionType) where T : class
        {
            return OfRevisionType(source, new RevisionStatusType(revisionType, revisionType));
        }

        /// <summary>
        /// Adds metadata to a query, to filter the <paramref name="source"/> based on the name of a revision type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="revisionType">Type of the revision.</param>
        /// <returns></returns>
        public static IQueryable<T> OfRevisionType<T>(this IQueryable<T> source, RevisionStatusType revisionType) where T : class
        {
            LogHelper.TraceIfEnabled(typeof(QueryExtensions), "In OfRevisionType");

            var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            ExpressionNodeModifierRegistry.Current.EnsureRegistered(
                currentGenericMethod.GetGenericMethodDefinition(), typeof(RevisionFilterExpressionNode));

            Expression<Func<IQueryable<T>>> expr = () => OfRevisionType(source, revisionType);
            return source.Provider.CreateQuery<T>(expr.Body);
        }

        public static IQueryable<T> InIds<T>(this IQueryable<T> source, IEnumerable<HiveId> ids) where T : class
        {
            return InIds(source, ids.ToArray());
        }

        public static IQueryable<T> InIds<T>(this IQueryable<T> source, params HiveId[] ids) where T : class
        {
            LogHelper.TraceIfEnabled(typeof(QueryExtensions), "In InIds");

            var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            ExpressionNodeModifierRegistry.Current.EnsureRegistered(currentGenericMethod.GetGenericMethodDefinition(), typeof(IdFilterExpressionNode));

            // If no ids were specified, the fact this method was called at all dictates we must filter the results to nothing
            if (ids.Any())
            {
                Expression<Func<IQueryable<T>>> expr = () => InIds(source, ids);
                return source.Provider.CreateQuery<T>(expr.Body);
            }
            else
                return Enumerable.Empty<T>().AsQueryable();
        }

        //[QuerySourceExtension]
        //public static IEnumerable<T> QueryChildren<T>(this T source)
        //{
        //    LogHelper.TraceIfEnabled(typeof(QueryExtensions), "In QueryChildren");

        //    var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
        //    //ExpressionNodeModifierRegistry.Current.EnsureRegistered(currentGenericMethod.GetGenericMethodDefinition(), typeof(HierarchyFilterExpressionNode));

        //    //return source.AsEnumerableOfOne().AsQueryable();
        //    return Enumerable.Empty<T>();
        //}

        #endregion
    }
}