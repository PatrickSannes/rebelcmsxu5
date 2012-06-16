
using Umbraco.Framework.Persistence.Abstractions.Attribution;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;

namespace Umbraco.Framework.Persistence.Abstractions
{
    /// <summary>
    /// Represents an entity with a forced schema defining its attributes.
    /// </summary>
    public interface ITypedPersistenceEntity : IPersistenceEntity
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
    }
}