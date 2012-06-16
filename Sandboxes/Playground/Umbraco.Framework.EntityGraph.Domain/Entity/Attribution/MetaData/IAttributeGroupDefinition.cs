namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData
{
    /// <summary>
    /// Defines a collection of <see cref="IAttributeTypeDefinition"/> in order to form a group. 
    /// Analogous to a document type tab in Umbraco 4.x
    /// </summary>
    public interface IAttributeGroupDefinition : IEntity, IReferenceByAlias, IReferenceByOrdinal
    {
        /// <summary>
        /// Gets the attribute definitions.
        /// </summary>
        /// <value>The attribute definitions.</value>
        IEntityCollection<IAttributeDefinition> AttributeDefinitions { get; }
    }
}