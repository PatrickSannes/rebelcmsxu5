using System;
using System.Linq.Expressions;
using FluentNHibernate.Mapping;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig
{
    public static class NhFluentMappingsHelper
    {
        public static string GenerateIndexName<TSource, TPropValue>(this ClassMap<TSource> mapper, Expression<Func<TSource, TPropValue>> propertyExpression)
        {
            return GenerateIndexName(propertyExpression);
        }

        private static string GenerateIndexName<TSource, TPropValue>(Expression<Func<TSource, TPropValue>> propertyExpression)
        {
            var propExpression = propertyExpression.Body as MemberExpression;
            if (propExpression != null)
                return "U5_IDX_" + typeof (TSource).Name + "_" + propExpression.Member.Name;
            throw new InvalidOperationException("Cannot generate an index name unless the expression represents a member");
        }

        public static string GenerateIndexName<TSource, TPropValue>(this SubclassMap<TSource> mapper, Expression<Func<TSource, TPropValue>> propertyExpression)
        {
            return GenerateIndexName(propertyExpression);
        }

        private static string GenerateFkName<TSource, TPropValue>(Expression<Func<TSource, TPropValue>> propertyExpression)
        {
            var propExpression = propertyExpression.Body as MemberExpression;
            if (propExpression != null)
                return "U5_FK_" + typeof(TSource).Name + "_" + propExpression.Member.Name;
            throw new InvalidOperationException("Cannot generate an FK name unless the expression represents a member");
        }

        public static string GenerateFkName<TSource, TPropValue>(this ClassMap<TSource> mapper, Expression<Func<TSource, TPropValue>> propertyExpression)
        {
            return GenerateFkName(propertyExpression);
        }

        public static string GenerateFkName<TSource, TPropValue>(this SubclassMap<TSource> mapper, Expression<Func<TSource, TPropValue>> propertyExpression)
        {
            return GenerateFkName(propertyExpression);
        }

        public static string GenerateFkName<TSource, TFkParent>(this SubclassMap<TSource> mapper)
        {
            return "U5_FK_" + typeof (TSource).Name + "_" + typeof (TFkParent).Name;
        }
    }
}