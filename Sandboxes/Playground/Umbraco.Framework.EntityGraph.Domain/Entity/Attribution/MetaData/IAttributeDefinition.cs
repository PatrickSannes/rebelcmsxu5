namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData
{
    /// <summary>
    /// Defines an attribute.
    /// </summary>
    public interface IAttributeDefinition : IEntity, IReferenceByAlias, IReferenceByOrdinal
    {
        /// <summary>
        /// Gets or sets the type of the attribute.
        /// </summary>
        /// <value>The type of the attribute.</value>
        IAttributeTypeDefinition AttributeType { get; set; }
    }
}