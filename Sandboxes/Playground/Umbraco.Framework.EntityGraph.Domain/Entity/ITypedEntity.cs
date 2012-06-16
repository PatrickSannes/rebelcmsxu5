using System.Diagnostics.Contracts;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;

namespace Umbraco.Framework.EntityGraph.Domain.Entity
{
    /// <summary>
    /// Represents an entity with a forced schema defining its attributes.
    /// </summary>
    [ContractClass(typeof (ITypedEntityCodeContract))]
    public interface ITypedEntity : IEntity
    {
        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        ITypedAttributeCollection Attributes { get; set; }
        /// <summary>
        /// Gets or sets the attribute groups.
        /// </summary>
        /// <value>The attribute groups.</value>
        IAttributeGroupCollection AttributeGroups { get; set; }
        /// <summary>
        ///   Gets the attribute schema.
        /// </summary>
        /// <value>The attribute schema.</value>
        IAttributionSchemaDefinition AttributionSchema { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>The type of the entity.</value>
        IEntityTypeDefinition EntityType { get; set; }
    }
}