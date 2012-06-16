using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Umbraco.Framework.Dynamics.Expressions
{
    using System.Linq;
    using Umbraco.Framework.Diagnostics;
    using Umbraco.Framework.Expressions.Remotion;

    public static class DynamicMemberMetadata
    {
        public static object GetMember(string name, object instance = null)
        {
            //LogHelper.TraceIfEnabled(typeof(DynamicMemberMetadata), "In GetMember");

            //var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod());
            //ExpressionNodeModifierRegistry.Current.EnsureRegistered(currentGenericMethod.GetGenericMethodDefinition(), typeof(DynamicMemberFilterExpressionNode));

            //Expression<Func<object>> expr = () => GetMember(name, instance);
            //return source.Provider.CreateQuery<T>(expr.Body);

            throw new UnreachableException();
        }

        //public static object GetMember(this IQueryable<object> source, string name, object instance = null)
        //{
        //    LogHelper.TraceIfEnabled(typeof(DynamicMemberMetadata), "In GetMember");

        //    var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod());
        //    ExpressionNodeModifierRegistry.Current.EnsureRegistered(currentGenericMethod.GetGenericMethodDefinition(), typeof(DynamicMemberFilterExpressionNode));

        //    Expression<Func<object>> expr = () => GetMember(name, instance);
        //    return source.Provider.CreateQuery<object>(expr.Body);
        //}

        public static readonly MethodInfo GetMemberMethod = ExpressionHelper.GetMethodInfo(() => GetMember(null, null));
        //public static readonly MethodInfo GetMemberRegistrationMethod = ExpressionHelper.GetMethodInfo(() => GetMember(null, null, null));

        public static MethodCallExpression GetMethodCallForDynamic(string fakeMemberName, object instance = null)
        {
            return Expression.Call(GetMemberMethod, Expression.Constant(fakeMemberName), Expression.Constant(instance));
        }

        public static Expression<Func<object, bool>> GetAsPredicate(string expression, params object[] substitutions)
        {
            var expr = GetExpression(expression, substitutions);

            return (Expression<Func<object, bool>>)Expression.Lambda(expr, Expression.Parameter(typeof(object), "unused"));
        }

        public static Expression GetExpression(string expression, params object[] substitutions)
        {
            var parser = new ExpressionParser(new[] { Expression.Parameter(typeof(object)) }, expression, substitutions);
            return parser.Parse(typeof(bool));
        }
    }
}