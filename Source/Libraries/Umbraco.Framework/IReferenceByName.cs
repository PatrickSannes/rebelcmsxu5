

namespace Umbraco.Framework
{
    /// <summary>
    /// Defines an object that can be referenced by a human-readable alias
    /// </summary>
    public interface IReferenceByName : IReferenceByAlias
    {
        /// <summary>
        /// Gets or sets the name of the object. The name is a string intended to be human-readable, and
        /// is often a more descriptive version of the <see cref="IReferenceByAlias.Alias"/> property.
        /// </summary>
        /// <value>The name.</value>
        LocalizedString Name { get; set; }
    }
}