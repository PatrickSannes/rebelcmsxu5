using System;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Defines a property
    /// </summary>
    public class PropertyDefinition
    {
        public string Name { get; set; }

        public Type Type { get; set; }

        public object Value { get; set; }
    }
}