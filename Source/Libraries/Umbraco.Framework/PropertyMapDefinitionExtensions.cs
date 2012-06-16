using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework
{
    /// <summary>
    /// Extension methods for TypeMapDefinition class
    /// </summary>
    internal static class PropertyMapDefinitionExtensions
    {
        /// <summary>
        /// Returns a HashCode for that target of the TypeMapDefinition object based on the target member's name & type, it's declaring object & declaring type
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        /// <remarks>
        /// The TypeMapDefinition class or any of it's child objects implement GetHashCode so we need to have our own.
        /// </remarks>
        internal static int GetTargetObjectHashCode(this PropertyMapDefinition c)
        {
            var hash = 31;
            if (c != null)
            {
                //the hash code should be unique to the the parent type, the property name and the property type
                hash ^= c.Target.Value.GetHashCode();
                hash ^= c.Target.Type.GetHashCode();
                hash ^= c.TargetProp.Name.GetHashCode();
                hash ^= c.TargetProp.Type.GetHashCode();
            }
            return hash;
        }

    }
}