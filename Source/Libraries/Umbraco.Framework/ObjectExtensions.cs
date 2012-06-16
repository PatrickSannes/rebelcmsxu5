#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Umbraco.Framework.Diagnostics;
using Formatting = Newtonsoft.Json.Formatting;

#endregion

namespace Umbraco.Framework
{
    public static class ObjectExtensions
    {

        /// <summary>
        /// Static constructor initializes and adds a custom type converter to the Boolean object
        /// </summary>
        static ObjectExtensions()
        {
            TypeDescriptor.AddAttributes(typeof(Boolean), new TypeConverterAttribute(typeof(CustomBooleanTypeConverter)));
        }

        private static readonly ConcurrentDictionary<Type, Func<object>> ObjectFactoryCache = new ConcurrentDictionary<Type, Func<object>>();

        public static IEnumerable<T> AsEnumerableOfOne<T>(this T input)
        {
            return Enumerable.Repeat(input, 1);
        }

        public static void DisposeIfDisposable(this object input)
        {
            var disposable = input as IDisposable;
            if (disposable != null) disposable.Dispose();
        }

        /// <summary>
        /// Provides a shortcut way of safely casting an input when you cannot guarantee the <typeparam name="T"></typeparam> is an instance type (i.e., when the C# AS keyword is not applicable)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static T SafeCast<T>(this object input)
        {
            if (ReferenceEquals(null, input) || ReferenceEquals(default(T), input)) return default(T);
            if (input is T) return (T)input;
            return default(T);
        }

        /// <summary>
        /// Tries to convert the input object to the output type using TypeConverters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static AttemptTuple<T> TryConvertTo<T>(this object input)
        {
            var result = TryConvertTo(input, typeof (T));
            return !result.Success ? AttemptTuple<T>.False : new AttemptTuple<T>(true, (T)result.Result);
        }

        /// <summary>
        /// Tries to convert the input object to the output type using TypeConverters. If the destination type is a superclass of the input type,
        /// if will use <see cref="Convert.ChangeType(object,System.Type)"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        public static AttemptTuple<object> TryConvertTo(this object input, Type destinationType)
        {
            if (input == null) return AttemptTuple<object>.False;

            if (input.GetType() == destinationType) return new AttemptTuple<object>(true, input);

			if ( !destinationType.IsGenericType || destinationType.GetGenericTypeDefinition() != typeof(Nullable<>) )
			{
				if (TypeFinder.IsTypeAssignableFrom(destinationType, input.GetType())
				    && TypeFinder.IsTypeAssignableFrom<IConvertible>(input))
				{
					var casted = Convert.ChangeType(input, destinationType);
					return new AttemptTuple<object>(true, casted);
				}
			}

        	var inputConverter = TypeDescriptor.GetConverter(input);
            if (inputConverter != null)
            {
                if (inputConverter.CanConvertTo(destinationType))
                {
                    return new AttemptTuple<object>(true, inputConverter.ConvertTo(input, destinationType));
                }
            }
            var outputConverter = TypeDescriptor.GetConverter(destinationType);
            if (outputConverter != null)
            {
                if (outputConverter.CanConvertFrom(input.GetType()))
                {
                    return new AttemptTuple<object>(true, outputConverter.ConvertFrom(input));
                }
            }

            try
            {
                if (TypeFinder.IsTypeAssignableFrom<IConvertible>(input))
                {
                    var casted = Convert.ChangeType(input, destinationType);
                    return new AttemptTuple<object>(true, casted);
                }
            }
            catch (Exception)
            {
                /* Swallow */
            }

            return AttemptTuple<object>.False;
        }

        public static void CheckThrowObjectDisposed(this IDisposable disposable, bool isDisposed, string objectname)
        {
            //TODO: Localise this exception
            if (isDisposed)
                throw new ObjectDisposedException(objectname);
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        /// <remarks>DOES NOT work in Medium Trust owing to the use of <see cref="BinaryFormatter" />. Use <see cref="DeepCopy{T}(T,System.Nullable{bool})"/> for cloning objects (excluding private members) in medium trust.</remarks>
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("In order to clone, the incoming type '{0}' must be serializable".InvariantFormat(typeof(T).FullName), "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Copies an entire object graph, with a factory for instantiating the initial copy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="hideErrorsInPartialTrust">Hides errors in partial trust by leaving fields / properties as their defaults when getting or setting is not possible.</param>
        /// <returns></returns>
        public static CloneOf<T> DeepCopy<T>(this T source, bool? hideErrorsInPartialTrust = true)
            where T : class, new()
        {
            var item = DeepCopy(source, typeof(T), hideErrorsInPartialTrust: hideErrorsInPartialTrust);
            return new CloneOf<T>(item.PartialTrustCausedPartialClone, item.Value as T);
        }

        /// <summary>
        /// Copies an entire object graph, with a factory for instantiating the initial copy. Useful when <typeparamref name="T"/> does not have a default constructor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="hideErrorsInPartialTrust">Hides errors in partial trust by leaving fields / properties as their defaults when getting or setting is not possible.</param>
        /// <returns></returns>
        public static CloneOf<T> DeepCopy<T>(this T source, Func<T> factory, bool? hideErrorsInPartialTrust = true)
            where T : class
        {
            var item = DeepCopy(source, typeof(T), hideErrorsInPartialTrust: hideErrorsInPartialTrust);
            return new CloneOf<T>(item.PartialTrustCausedPartialClone, item.Value as T);
        }


        /// <summary>
        /// A cached wrapper for <see cref="GetObjectFactory"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static object CreateObject(Type type)
        {
            return ObjectFactoryCache.GetOrAdd(type, GetObjectFactory).Invoke();
        }

        /// <summary>
        /// Creates an object matching the type <paramref name="type"/>. A search is made for constructors that have zero parameters. If the current <see cref="AppDomain"/>
        /// is not fully trusted, it will exclude matching constructors that are SecurityCritical or SecuritySafeCritical.
        /// If none is found, but the <see cref="AppDomain"/> is fully trusted, <see cref="FormatterServices.GetUninitializedObject"/> is used to instantiate an object
        /// without a constructor. 
        /// If in partial trust, the next visible constructor is used having the least parameters for which default values can be passed in.
        /// Finally, if none is found, an exception is thrown.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        [SecuritySafeCritical]
        private static Func<object> GetObjectFactory(Type type)
        {
            if (type.IsInterface)
                throw new InvalidOperationException("Cannot instantiate an interface ({0})".InvariantFormat(type.FullName));

            var isFullyTrusted = AppDomain.CurrentDomain.IsFullyTrusted;

            var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetParameters().Count() == 0 && (isFullyTrusted || (!isFullyTrusted && !x.IsSecurityCritical && !x.IsSecuritySafeCritical)))
                .FirstOrDefault();

            if (constructor != null)
            {
                // This catch block is here because there are certain types of constructor that I can't grab via properties on ConstructorInfo
                // for example NullViewLocationCache is a sealed internal type with no constructor, yet a constructor is found using the above 
                // method!
                try
                {
                    return () => constructor.Invoke(new object[0]);
                }
                catch (SecurityException ex)
                {
                    LogSecurityException(type, isFullyTrusted, ex);
                }
                catch (MemberAccessException ex)
                {
                    ThrowIfFullTrust(type, isFullyTrusted, ex);
                }
            }

            if (isFullyTrusted)
                return () => FormatterServices.GetUninitializedObject(type);

            // We're in partial trust, but haven't found a default constructor, so find the first constructor with nullable parameter types
            var nearestConstructor = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .OrderBy(x => x.GetParameters().Count())
                .FirstOrDefault();

            if (nearestConstructor != null)
            {
                var paramValues = nearestConstructor.GetParameters().Select(x => TypeFinder.IsImplicitValueType(x.ParameterType) ? x.ParameterType.GetDefaultValue() : CreateObject(x.ParameterType)).ToArray();
                try
                {
                    return () => nearestConstructor.Invoke(paramValues);
                }
                catch (SecurityException ex)
                {
                    LogSecurityException(type, isFullyTrusted, ex);
                }
                catch (MemberAccessException ex)
                {
                    ThrowIfFullTrust(type, isFullyTrusted, ex);
                }
            }

            throw new InvalidOperationException(
                "Cannot find a default constructor for {0}, and since this AppDomain is not fully trusted, also tried to find another constructor, but that failed too. Come to think of it, how do YOU make this object?".InvariantFormat(type.FullName));
        }

        private static void ThrowIfFullTrust(Type type, bool isFullyTrusted, MemberAccessException ex)
        {
            if (ex.InnerException != null && ex.InnerException is SecurityException)
                if (LogSecurityException(type, isFullyTrusted, ex.InnerException as SecurityException)) return;
            throw ex;
        }

        private static bool LogSecurityException(Type type, bool isFullyTrusted, SecurityException ex)
        {
            if (!isFullyTrusted)
            {
                // Only log the error and continue to try other methods
                LogHelper.TraceIfEnabled(
                    type,
                    "Cannot run default constructor for {0} because: {1}",
                    () => type.FullName,
                    () => ex.Message);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Copies an object graph.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="recurseCount">The recurse count.</param>
        /// <param name="hideErrorsInPartialTrust">The hide errors in partial trust.</param>
        /// <returns></returns>
        public static CloneOf<object> DeepCopy(this object source, Type targetType, Func<object> factory = null, int recurseCount = 0, bool? hideErrorsInPartialTrust = true)
        {
            if (recurseCount > 50)
                throw new InvalidOperationException("Cannot clone an object graph greater than 50 objects in depth");

            var isPartialClone = false;

            //grab the type and create a new instance of that type
            var sourceType = targetType;
            var target = factory == null ? CreateObject(targetType) : factory.Invoke();

            //grab the properties
            var discoverableProperties = TypeFinder.CachedDiscoverableProperties(sourceType);

            //iterate over the properties and if it has a 'set' method assign it from the source TO the target
            foreach (var info in discoverableProperties)
            {
                // We don't check IsAssembly and IsFamily on the get method to ensure we don't try internal / private get accessors
                // because we rely on catching an exception instead - without comparing the PermissionSets of this code with the target
                // code IsAssembly and IsFamily don't give a clear picture of whether we can read it with reflection anyway

                // The GetMethod might have security parameters on it or might be internal. If security is tight
                // on the get method, it won't be returned, so check for null and double-check it's not internal
                var methodGetter = TypeFinder.DynamicMemberAccess.GetterDelegate(info);

                // We don't check !methodInfo.IsAssembly && !methodInfo.IsFamily here, instead leaving that to the try-catch
                // so that we can report back to the caller. If it was easier to check our PermissionSet with that of the type
                // we're looking at, I'd avoid a try-catch, but clarifying the PermissionSet requires full trust(!) - APN
                if (methodGetter != null)
                {
                    try
                    {
                        var propertyValue = info.GetValue(source, null);
                        var setter = TypeFinder.DynamicMemberAccess.SetterDelegate(info, true);
                        SetValue(info, info.PropertyType, target, propertyValue, setter.Invoke, recurseCount);
                    }
                    catch (MemberAccessException)
                    {
                        if (hideErrorsInPartialTrust.HasValue && hideErrorsInPartialTrust.Value)
                        {
                            isPartialClone = true;
                            continue;
                        }
                        throw;
                    }
                }
            }

            // grab protected fields
            var discoverableFields = TypeFinder.CachedDiscoverableFields(sourceType);

            // Iterate over the properties to see if it is not readonly
            foreach (var fieldInfo in discoverableFields)
            {
                var localFieldInfoCopy = fieldInfo;

                try
                {
                    var sourceValue = fieldInfo.GetValue(source);
                    SetValue(fieldInfo, fieldInfo.FieldType, target, sourceValue, localFieldInfoCopy.SetValue, recurseCount);
                }
                catch (FieldAccessException)
                {
                    if (hideErrorsInPartialTrust.HasValue && hideErrorsInPartialTrust.Value)
                    {
                        isPartialClone = true;
                        continue;
                    }
                    throw;
                }
            }

            //return the new item
            return new CloneOf<object>(isPartialClone, target);
        }

        /// <summary>
        /// Sets a value on a member of an instance <paramref name="target"/> of type <paramref name="sourceType"/>.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="target">The target.</param>
        /// <param name="sourceValue">The source value.</param>
        /// <param name="setter">The setter.</param>
        /// <param name="outerLoopRecurseCount">The outer loop recursion count.</param>
        private static void SetValue(MemberInfo member, Type sourceType, object target, object sourceValue, Action<object, object> setter, int outerLoopRecurseCount)
        {
            if (TypeFinder.IsImplicitValueType(sourceType))
            {
                setter.Invoke(target, sourceValue);
            }
            else
            {
                if (sourceValue == null)
                {
                    setter.Invoke(target, null);
                }
                else
                {
                    if (sourceType.IsArray)
                    {
                        // It's an array, so we need to run this method for each item in the array and return an array of the same type
                        var newArray = DeepCloneArray(sourceType, sourceValue as Array);
                        setter.Invoke(target, newArray);
                        return;
                    }
                    else
                    {
                        // Certain types are not cloneable, e.g. delegates
                        if (IsNotCloneable(member))
                        {
                            // Just set the value to be the same
                            setter.Invoke(target, sourceValue);
                        }
                        else
                        {
                            // Need to get the actual type, not the signature one (e.g. the runtime type not the interface)
                            var reflectedType = sourceValue.GetType();
                            CloneOf<object> deepCopy = DeepCopy(sourceValue, reflectedType, recurseCount: outerLoopRecurseCount + 1);
                            setter.Invoke(target, deepCopy.Value);
                        }
                    }
                }
            }
        }

        private static bool IsNotCloneable(MemberInfo info)
        {
            return TypeFinder.IsTypeAssignableFrom<Delegate>(info.DeclaringType);
        }

        private static Array DeepCloneArray(Type arrayHolderType, Array originalArray)
        {
            Mandate.ParameterNotNull(arrayHolderType, "arrayHolderType");
            Mandate.ParameterNotNull(originalArray, "originalArray");

            var elementType = arrayHolderType.GetElementType();
            var newArray = CreateArray(originalArray, elementType);
            for (var index = 0; index < originalArray.Length; index++)
            {
                if (TypeFinder.IsImplicitValueType(elementType))
                {
                    newArray.SetValue(originalArray.GetValue(index), index);
                }
                else
                {
                    newArray.SetValue(DeepCopy(originalArray.GetValue(index)), index);
                }
            }
            return newArray;
        }

        private static Array CreateArray(Array originalArray, Type elementType)
        {
            return Array.CreateInstance(elementType, originalArray.Length);
        }


        /// <summary>
        /// Convert an object to a JSON string with camelCase formatting
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJsonString(this object obj)
        {
            return obj.ToJsonString(PropertyNamesCaseType.CamelCase);
        }

        public static string EncodeJsString(this string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convert an object to a JSON string with the specified formatting
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="propertyNamesCaseType">Type of the property names case.</param>
        /// <returns></returns>
        public static string ToJsonString(this object obj, PropertyNamesCaseType propertyNamesCaseType)
        {
            var type = obj.GetType();
            var dateTimeStyle = "yyyy-MM-dd HH:mm:ss";

            if (type.IsPrimitive || typeof(string).IsAssignableFrom(type))
            {
                return obj.ToString();
            }

            if (typeof(DateTime).IsAssignableFrom(type) || typeof(DateTimeOffset).IsAssignableFrom(type))
            {
                Convert.ToDateTime(obj).ToString(dateTimeStyle);
            }

            var serializer = new JsonSerializer();

            switch (propertyNamesCaseType)
            {
                case PropertyNamesCaseType.CamelCase:
                    serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    break;
            }

            var dateTimeConverter = new IsoDateTimeConverter
            {
                DateTimeStyles = System.Globalization.DateTimeStyles.None,
                DateTimeFormat = dateTimeStyle
            };

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return JObject.FromObject(obj, serializer).ToString(Formatting.None, dateTimeConverter);
            }

            if (type.IsArray || (typeof(IEnumerable).IsAssignableFrom(type)))
            {
                return JArray.FromObject(obj, serializer).ToString(Formatting.None, dateTimeConverter);
            }

            return JObject.FromObject(obj, serializer).ToString(Formatting.None, dateTimeConverter);
        }


        /// <summary>
        /// Converts an object into a dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="o"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        public static IDictionary<string, TVal> ToDictionary<T, TProperty, TVal>(this T o,
            params Expression<Func<T, TProperty>>[] ignoreProperties)
        {
            return o.ToDictionary<TVal>(ignoreProperties.Select(e => o.GetPropertyInfo(e)).Select(propInfo => propInfo.Name).ToArray());
        }

        /// <summary>
        /// Turns object into dictionary
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreProperties">Properties to ignore</param>
        /// <returns></returns>
        public static IDictionary<string, TVal> ToDictionary<TVal>(this object o, params string[] ignoreProperties)
        {
            if (o != null)
            {
                var props = TypeDescriptor.GetProperties(o);
                var d = new Dictionary<string, TVal>();
                foreach (var prop in props.Cast<PropertyDescriptor>().Where(x => !ignoreProperties.Contains(x.Name)))
                {
                    var val = prop.GetValue(o);
                    if (val != null)
                    {
                        d.Add(prop.Name, (TVal)val);    
                    }
                }
                return d;
            }
            return new Dictionary<string, TVal>();
        }

        public static string ToDebugString(this object obj, int levels = 0)
        {
            if (obj == null) return "{null}";
            try
            {
                if (obj is string)
                {
                    return "\"{0}\"".InvariantFormat(obj);
                }
                if (obj is int || obj is Int16 || obj is Int64 || obj is double || obj is bool || obj is int? || obj is Int16? || obj is Int64? || obj is double? || obj is bool?)
                {
                    return "{0}".InvariantFormat(obj);
                }
                if (obj is Enum)
                {
                    return "[{0}]".InvariantFormat(obj);
                }
                if (obj is IEnumerable)
                {
                    var enumerable = (obj as IEnumerable);

                    var items = (from object enumItem in enumerable let value = GetEnumPropertyDebugString(enumItem, levels) where value != null select value).Take(10).ToList();

                    return items.Count() > 0
                      ? "{{ {0} }}".InvariantFormat(String.Join(", ", items))
                      : null;
                }

                var props = obj.GetType().GetProperties();
                if ((props.Count() == 2) && props[0].Name == "Key" && props[1].Name == "Value" && levels > -2)
                {
                    try
                    {
                        var key = props[0].GetValue(obj, null) as string;
                        var value = props[1].GetValue(obj, null).ToDebugString(levels - 1);
                        return "{0}={1}".InvariantFormat(key, value);
                    }
                    catch (Exception)
                    {
                        return "[KeyValuePropertyException]";
                    }
                }
                if (levels > -1)
                {
                    var items =
                      from propertyInfo in props
                      let value = GetPropertyDebugString(propertyInfo, obj, levels)
                      where value != null
                      select "{0}={1}".InvariantFormat(propertyInfo.Name, value);

                    return items.Count() > 0
                             ? "[{0}]:{{ {1} }}".InvariantFormat(obj.GetType().Name, String.Join(", ", items))
                             : null;
                }
            }
            catch (Exception ex)
            {
                return "[Exception:{0}]".InvariantFormat(ex.Message);
            }
            return null;
        }

        public static string ToXmlString(this object value, Type type)
        {
            var sb = new StringBuilder();

            using (var writer = XmlWriter.Create(sb))
            {
                new XmlSerializer(type).Serialize(writer, value);
            }

            // The XmlSerializer wraps the serialized value in an XML node, so grab the first elements value
            var xml = new XmlDocument();
            xml.LoadXml(sb.ToString());

            return (xml.DocumentElement != null)
                ? xml.DocumentElement.InnerText
                : "";
        }

        private static string GetEnumPropertyDebugString(object enumItem, int levels)
        {
            try
            {
                return enumItem.ToDebugString(levels - 1);
            }
            catch (Exception)
            {
                return "[GetEnumPartException]";
            }
        }

        private static string GetPropertyDebugString(PropertyInfo propertyInfo, object obj, int levels)
        {
            try
            {
                return propertyInfo.GetValue(obj, null).ToDebugString(levels - 1);
            }
            catch (Exception)
            {
                return "[GetPropertyValueException]";
            }
        }

    }

    public enum PropertyNamesCaseType
    {
        CamelCase,
        CaseInsensitive
    }
}
