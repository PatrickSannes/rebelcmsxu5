#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

#endregion

namespace Umbraco.Framework
{
    using System.Collections.Concurrent;

    public static class TypeExtensions
    {
        /// <summary>
        /// Checks if the type is an anonymous type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// reference: http://jclaes.blogspot.com/2011/05/checking-for-anonymous-types.html
        /// </remarks>
        public static bool IsAnonymousType(this Type type)
        {
            Mandate.ParameterNotNull(type, "type");

            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherited)
            where T : Attribute
        {
            return type.GetCustomAttributes(inherited).OfType<T>();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo info, bool inherited)
            where T : Attribute
        {
            return info.GetCustomAttributes(inherited).OfType<T>();
        }

        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="inherited">if set to <c>true</c> [inherited].</param>
        /// <returns></returns>
        public static T GetCustomMemberAttribute<T>(this MemberInfo type, bool inherited) where T : MemberInfoAttribute
        {
            var attributes = type.GetCustomMemberAttributes<T>(inherited);
            return attributes.FirstOrDefault();
        }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="inherited">if set to <c>true</c> [inherited].</param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomMemberAttributes<T>(this MemberInfo type, bool inherited) where T : MemberInfoAttribute
        {
            var attributes = type.GetCustomAttributes(typeof(T), inherited).Cast<T>();
            foreach (var attribute in attributes)
            {
                attribute.Target = type;
            }

            return attributes;
        }

        /// <summary>
        /// Gets the properties with custom attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="inherited">if set to <c>true</c> [inherited].</param>
        /// <returns></returns>
        public static Dictionary<PropertyInfo, T> GetPropertiesWithCustomAttributes<T>(this Type type, bool inherited) where T : MemberInfoAttribute
        {
            var typeProperties = new Dictionary<PropertyInfo, T>();
            var properties = type.GetProperties().Where(t => t.GetCustomMemberAttribute<T>(inherited) != null);
            foreach (var propertyInfo in properties)
            {
                var attribute = propertyInfo.GetCustomMemberAttribute<T>(false);
                typeProperties.Add(propertyInfo, attribute);
            }

            return typeProperties;
        }


        /// <summary>
        /// Determines whether the specified type is enumerable.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is enumerable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnumerable(this Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
                    return true;
            }
            else
            {
                if (type.GetInterfaces().Contains(typeof(IEnumerable)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is of generic type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="genericType">Type of the generic.</param>
        /// <returns>
        ///   <c>true</c> if [is of generic type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOfGenericType(this Type type, Type genericType)
        {
            Type[] args;
            return type.TryGetGenericArguments(genericType, out args);
        }

        /// <summary>
        /// Will find the generic type of the 'type' parameter passed in that is equal to the 'genericType' parameter passed in
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericType"></param>
        /// <param name="genericArgType"></param>
        /// <returns></returns>
        public static bool TryGetGenericArguments(this Type type, Type genericType, out Type[] genericArgType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (genericType == null)
            {
                throw new ArgumentNullException("genericType");
            }
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException("genericType must be a generic type");
            }

            Func<Type, Type, Type[]> checkGenericType = (@int, t) =>
                {
                    if (@int.IsGenericType)
                    {
                        var def = @int.GetGenericTypeDefinition();
                        if (def == t)
                        {
                            return @int.GetGenericArguments();
                        }
                    }
                    return null;
                };

            //first, check if the type passed in is already the generic type
            genericArgType = checkGenericType(type, genericType);
            if (genericArgType != null)
                return true;
            
            //if we're looking for interfaces, enumerate them:
            if (genericType.IsInterface)
            {
                foreach (Type @interface in type.GetInterfaces())
                {
                    genericArgType = checkGenericType(@interface, genericType);
                    if (genericArgType != null)
                        return true;
                }
            }
            else
            {
                //loop back into the base types as long as they are generic
                while (type.BaseType != null && type.BaseType != typeof(object))
                {
                    genericArgType = checkGenericType(type.BaseType, genericType);
                    if (genericArgType != null)
                        return true;
                    type = type.BaseType;
                }
                
            }
            

            return false;          
            
        }


        /// <summary>
        /// Determines whether the specified actual type is type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actualType">The actual type.</param>
        /// <returns>
        ///   <c>true</c> if the specified actual type is type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsType<T>(this Type actualType)
        {
            return TypeFinder.IsTypeAssignableFrom<T>(actualType);
        }

        private static readonly ConcurrentDictionary<Type, object> DefaultValueCache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Gets the default value for a ValueType at runtime.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static object GetDefaultValue(this Type t)
        {
            return DefaultValueCache.GetOrAdd(t, key => key.IsValueType ? Activator.CreateInstance(key) : null);
        }
    }
}