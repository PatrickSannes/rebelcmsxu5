using System;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Simple class to create new objects of the specified type.
    /// </summary>
    internal static class Creator
    {
        public static object Create(Type type, params object[] ctorParams)
        {
            if (TypeFinder.IsImplicitValueType(type))
            {
                throw new ArgumentException("Cannot create an instance of a Value Type, Primitive Type, or string");
            }

            if (type.IsInterface)
                throw new TypeLoadException("Cannot create an instance of an interface: " + type.Name);

            return Activator.CreateInstance(type, ctorParams);
        }
    }
}