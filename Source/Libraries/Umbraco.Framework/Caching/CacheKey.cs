using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Caching
{
    using System.Reflection;
    using System.Runtime.Serialization.Formatters;
    using Newtonsoft.Json;
    using Umbraco.Framework.Diagnostics;

    public class CacheKey : AbstractEquatableObject<CacheKey>
    {
        public string ToJson()
        {
            return AddTypeToJson(GetType(), JsonConvert.SerializeObject(this));
        }

        public override string ToString()
        {
            return ToJson();
        }

        public static implicit operator string(CacheKey key)
        {
            return key.ToString();
        }

        protected string AsJson
        {
            get
            {
                return ToJson();
            }
        }

        public static CacheKey<TReturn> Create<TReturn>(TReturn from)
        {
            return new CacheKey<TReturn>(@from);
        }

        public static CacheKey<TKey> Create<TKey>(Action<TKey> setup)
            where TKey : class, new()
        {
            var keyOriginal = new TKey();
            setup.Invoke(keyOriginal);
            return new CacheKey<TKey>(keyOriginal);
        }

        #region Overrides of AbstractEquatableObject<CacheKey>

        /// <summary>
        /// Gets the natural id members.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.AsJson);
        }

        #endregion

        protected static string AddTypeToJson(Type type, string json)
        {
            if (!json.Contains("║"))
            {
                return type.FullName + "║" + json;
            }
            return json;
        }

        protected static string GetAndRemoveTypeFromJson(string json, out Type type)
        {
            if (json.Contains("║"))
            {
                var split = json.Split('║');
                if (split.Length != 2)
                {
                    type = null;
                    return json;
                }
                var typeName = split[0];
                var properJson = split[1];
                type = Type.GetType(typeName, false);
                return properJson;
            }
            type = null;
            return json;
        }
    }

    public class CacheKey<T> : CacheKey
    {
        public CacheKey()
        {
        }

        public CacheKey(T original)
        {
            Original = original;
        }

        public static readonly CacheKey<T> Empty = default(CacheKey<T>);

        public T Original { get; set; }

        public static implicit operator string(CacheKey<T> key)
        {
            return key.ToString();
        }

        public static implicit operator CacheKey<T>(string key)
        {
            Type realType = null;
            var getTypeFromJson = GetAndRemoveTypeFromJson(key, out realType);
            var jsonSerializerSettings = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error, ObjectCreationHandling = ObjectCreationHandling.Replace };
            try
            {
                if (realType == typeof(CacheKey<T>))
                    return JsonConvert.DeserializeObject<CacheKey<T>>(getTypeFromJson, jsonSerializerSettings);
            }
            catch (JsonSerializationException)
            {
                /* Don't log */
            }

            try
            {
                if (realType == typeof(T))
                {
                    var obj = JsonConvert.DeserializeObject<T>(getTypeFromJson, jsonSerializerSettings);
                    return new CacheKey<T>(obj);
                }
            }
            catch (JsonSerializationException ex)
            {
                /* Don't log */
            }

            return CacheKey<T>.Empty;
        }

        protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.Original);
        }
    }
}
