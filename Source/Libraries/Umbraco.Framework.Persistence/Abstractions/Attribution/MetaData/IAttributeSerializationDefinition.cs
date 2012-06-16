
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.Persistence.Model.Attribution;

namespace Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData
{
    using Umbraco.Framework.Data;

    public interface IAttributeSerializationDefinition : IReferenceByName
    {
        /// <summary>
        ///   Gets or sets the type of the data serialization.
        /// </summary>
        /// <value>The type of the data serialization.</value>
        DataSerializationTypes DataSerializationType { get; }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        dynamic Serialize(TypedAttribute value);
    }
}