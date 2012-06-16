using System;

namespace Umbraco.Framework.Security
{
    /// <summary>
    /// Defines a Permission
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PermissionAttribute : PluginAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionAttribute"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        public PermissionAttribute(string id, string name, string type)
            : base(id)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNullOrEmpty(type, "type");

            Name = name;
            Type = type;
        }

        /// <summary>
        /// Gets the name of the name.
        /// </summary>
        /// <remarks></remarks>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public string Type { get; private set; }
    }
}
