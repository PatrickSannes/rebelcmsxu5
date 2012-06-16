using System;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework
{
    public static class MappingExpressionExtensions
    {
        /// <summary>
        /// Ignores a member to map by its name
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="expression"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TTarget> IgnoreMemberByName<TSource, TTarget>(
           this IMappingExpression<TSource, TTarget> expression,
           string memberName)
        {
            var member = TypeFinder.CachedDiscoverableProperties(typeof (TTarget))
                .Where(x => x.Name == memberName)
                .SingleOrDefault();
            if (member == null)
                throw new MissingMemberException("Could not find the property " + memberName + " on type " + typeof (TTarget).FullName);

            expression.MappingContext.AddMemberExpression(member.PropertyType, typeof (TTarget), memberName, opt => opt.Ignore());
            return expression;
        }

        /// <summary>
        /// Shorthand code to ignore a member for maping (Ignore())
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="destinationMember"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TTarget> IgnoreMember<TSource, TTarget, TProperty>(
            this IMappingExpression<TSource, TTarget> expression,
            Expression<Func<TTarget, TProperty>> destinationMember)
        {
            return expression.ForMember(destinationMember, opt => opt.Ignore());
        }

        /// <summary>
        /// Shorthand method to map a member from (MapFrom())
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="destinationMember"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TTarget> MapMemberFrom<TSource, TTarget, TProperty>(
            this IMappingExpression<TSource, TTarget> expression,
            Expression<Func<TTarget, TProperty>> destinationMember,
            Func<TSource, TProperty> source)
        {
            return expression.ForMember(destinationMember, opt => opt.MapFrom(source));
        }

        /// <summary>
        /// Shorthand method to map a member using (MapUsing())
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="destinationMember"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TTarget> MapMemberUsing<TSource, TTarget, TProperty>(
            this IMappingExpression<TSource, TTarget> expression,
            Expression<Func<TTarget, TProperty>> destinationMember,
            MemberMapper<TSource, TProperty> source)
        {
            return expression.ForMember(destinationMember, opt => opt.MapUsing(source));
        }

        /// <summary>
        /// Shorthand method to map a member using (MapUsing())
        /// </summary>
        /// <typeparam name="TMemberMapper"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="destinationMember"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TTarget> MapMemberUsing<TMemberMapper, TSource, TTarget, TProperty>(
            this IMappingExpression<TSource, TTarget> expression,
            Expression<Func<TTarget, TProperty>> destinationMember)
            where TMemberMapper: MemberMapper<TSource, TProperty>, new()
        {
            return expression.ForMember(destinationMember, opt => opt.MapUsing<TMemberMapper>());
        }

    }
}