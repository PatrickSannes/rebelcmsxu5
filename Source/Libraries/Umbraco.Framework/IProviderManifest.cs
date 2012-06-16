

namespace Umbraco.Framework
{
    public interface IProviderManifest : IReferenceByName
    {
        /// <summary>
        /// Gets the name of the assembly containing this provider.
        /// </summary>
        /// <value>The name of the assembly.</value>
        string AssemblyName { get; }

        /// <summary>
        /// Gets the name of the type this Provider represents.
        /// </summary>
        /// <value>The name of the type.</value>
        string TypeName { get; }

        /// <summary>
        /// Gets or sets the mapping alias, typically a short key used in configuration or an EntityProviderMap
        /// which is a guaranteed constant for referring to this instance of the provider across
        /// multiplle instances of the entire application.
        /// </summary>
        /// <value>The mapping alias.</value>
        string MappingAlias { get; set; }
    }
}