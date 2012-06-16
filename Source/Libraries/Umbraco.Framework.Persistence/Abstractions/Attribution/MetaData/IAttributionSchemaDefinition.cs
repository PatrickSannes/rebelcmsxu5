

using Umbraco.Foundation;

namespace Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData
{
    /// <summary>
    ///   Defines the permitted schema of the attributes on an <see cref = "IPersistenceEntity" />, in order for it to become an <see cref="ITypedPersistenceEntity"/>.
    /// Analogous to a document type in Umbraco 4.x
    /// </summary>
    public interface IAttributionSchemaDefinition : IPersistenceEntity, IReferenceByAlias
    {
        /// <summary>
        ///   Gets or sets the attribute type definitions for this schema.
        /// </summary>
        /// <value>The attribute type definitions.</value>
        IPersistenceEntityCollection<IAttributeTypeDefinition> AttributeTypeDefinitions { get; }

        /// <summary>
        /// Gets the attribute group definitions.
        /// </summary>
        /// <value>The attribute group definitions.</value>
        IPersistenceEntityCollection<IAttributeGroupDefinition> AttributeGroupDefinitions { get; }
    }
}