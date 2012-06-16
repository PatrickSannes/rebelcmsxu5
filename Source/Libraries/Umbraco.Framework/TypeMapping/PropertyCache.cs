using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Used to lookup property descriptors for types/object and cache them internal so we don't need to look them up again
    /// </summary>
    public static class PropertyCache
    {
        private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> Cache = new ConcurrentDictionary<Type, PropertyDescriptorCollection>();         
        
        public static PropertyDescriptorCollection GetProperties(Type type)
        {
            return Cache.GetOrAdd(type, x => TypeDescriptor.GetProperties(type));
        }

        public static PropertyDescriptorCollection GetProperties(this object o)
        {
            return GetProperties(o.GetType());
        }

    }
}