using System.Diagnostics.Contracts;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData
{
    /// <summary>
    ///   Defines the permitted schema of the attributes on an <see cref = "IEntity" />, in order for it to become an <see cref="ITypedEntity"/>.
    /// Analogous to a document type in Umbraco 4.x
    /// </summary>
    [ContractClass(typeof (AttributionSchemaDefinitionCodeContract))]
    public interface IAttributionSchemaDefinition : IEntity, IReferenceByAlias
    {
        /// <summary>
        ///   Gets or sets the attribute type definitions for this schema.
        /// </summary>
        /// <value>The attribute type definitions.</value>
        IEntityCollection<IAttributeTypeDefinition> AttributeTypeDefinitions { get; }

        /// <summary>
        /// Gets the attribute group definitions.
        /// </summary>
        /// <value>The attribute group definitions.</value>
        IEntityCollection<IAttributeGroupDefinition> AttributeGroupDefinitions { get; }
    }
}