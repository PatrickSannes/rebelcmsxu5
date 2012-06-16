using Umbraco.Foundation;

namespace Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData
{
    /// <summary>
    /// Defines an attribute.
    /// </summary>
    public interface IAttributeDefinition : IPersistenceEntity, IReferenceByAlias, IReferenceByOrdinal
    {
        /// <summary>
        /// Gets or sets the type of the attribute.
        /// </summary>
        /// <value>The type of the attribute.</value>
        IAttributeTypeDefinition AttributeType { get; set; }
    }
}