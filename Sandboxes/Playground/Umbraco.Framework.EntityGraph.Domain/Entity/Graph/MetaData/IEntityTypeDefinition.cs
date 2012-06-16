using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData
{
    /// <summary>
    /// Represents the schema for an entity by defining constraints about its permitted relationships with
    /// other entities in the graph, together with its permitted attributes and their type.
    /// </summary>
    public interface IEntityTypeDefinition : IReferenceByAlias, IEntityVertex
    {
        /// <summary>
        /// Gets the graph schema.
        /// </summary>
        /// <value>The graph schema.</value>
        IEntityGraphSchema GraphSchema { get; set; }

        /// <summary>
        /// Gets the attribute schema.
        /// </summary>
        /// <value>The attribute schema.</value>
        IAttributionSchemaDefinition AttributeSchema { get; set; }
    }
}