using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web
{
    using Umbraco.Cms.Web.Context;

    using Umbraco.Cms.Web.Model;

    using Umbraco.Framework.Dynamics.Expressions;

    using Umbraco.Hive;

    using Umbraco.Hive.RepositoryTypes;

    using global::System.Web.Mvc;

    public static class RenderViewModelQueryExtensions
    {
        public static IQueryable<object> QueryAll(IHiveManager hiveManager)
        {
            return QueryAll<object>(hiveManager);
        }

        public static IQueryable<object> Where(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            var dynQuery = DynamicMemberMetadata.GetAsPredicate(expression, substitutions);
            return dynamicQueryable.AsQueryable<object>().Where(dynQuery);
        }

        public static IQueryable<T> Where<T>(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            var dynQuery = DynamicMemberMetadata.GetAsPredicate(expression, substitutions);
            return dynamicQueryable.AsQueryable<object>().Where(dynQuery).Cast<T>();
        }

        private static IQueryable<T> PrepareStringExpression<T>(IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            var dynQuery = DynamicMemberMetadata.GetAsPredicate(expression, substitutions);
            return dynamicQueryable.AsQueryable<object>().Where(dynQuery).Cast<T>();
        }

        #region Extension methods with Generic Param
        public static T First<T>(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            return PrepareStringExpression<T>(dynamicQueryable, expression, substitutions).First();
        }

        public static T FirstOrDefault<T>(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            return PrepareStringExpression<T>(dynamicQueryable, expression, substitutions).FirstOrDefault();
        }

        public static T Single<T>(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            return PrepareStringExpression<T>(dynamicQueryable, expression, substitutions).Single();
        }

        public static T SingleOrDefault<T>(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            return PrepareStringExpression<T>(dynamicQueryable, expression, substitutions).SingleOrDefault();
        }

        public static int Count<T>(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            return PrepareStringExpression<T>(dynamicQueryable, expression, substitutions).Count<T>();
        }

        public static bool Any<T>(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            return PrepareStringExpression<T>(dynamicQueryable, expression, substitutions).Any<T>();
        }

        public static bool All<T>(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            var dynQuery = DynamicMemberMetadata.GetAsPredicate(expression, substitutions);
            return dynamicQueryable.AsQueryable<object>().All(dynQuery);
        }

        public static IQueryable<T> Skip<T>(this IQueryable<object> dynamicQueryable, int count, string expression, params object[] substitutions)
        {
            return PrepareStringExpression<T>(dynamicQueryable, expression, substitutions).Skip(count);
        }

        public static IQueryable<T> Take<T>(this IQueryable<object> dynamicQueryable, int count, string expression, params object[] substitutions)
        {
            return PrepareStringExpression<T>(dynamicQueryable, expression, substitutions).Take(count);
        }

        #endregion



        public static dynamic First(this IQueryable<object> dynamicQueryable, string expression, params object[] substitutions)
        {
            var dynQuery = DynamicMemberMetadata.GetAsPredicate(expression, substitutions);
            return dynamicQueryable.AsQueryable<object>().Where(dynQuery).Cast<Content>().First().Bend();
        }

        public static IQueryable<T> QueryAll<T>(IHiveManager hiveManager)
        {
            using (var uow = hiveManager.OpenReader<IContentStore>())
            {
                return uow.Repositories.Select(x => hiveManager.FrameworkContext.TypeMappers.Map<T>(x));
            }
        }

        public static IQueryable<object> DynamicQueryAll()
        {
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return QueryAll<object>(appContext.Hive);
            }
            return Enumerable.Empty<object>().AsQueryable();
        }

        public static IQueryable<Content> QueryAll(this Content content)
        {
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return QueryAll<Content>(appContext.Hive);
            }
            return Enumerable.Empty<Content>().AsQueryable();
        }
    }
}
