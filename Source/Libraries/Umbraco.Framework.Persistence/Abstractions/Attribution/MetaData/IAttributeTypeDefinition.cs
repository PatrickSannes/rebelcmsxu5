

using Umbraco.Foundation;

namespace Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData
{
    /// <summary>
    ///   Defines a type boundary for an attribute
    /// </summary>
    public interface IAttributeTypeDefinition : IPersistenceEntity, IReferenceByAlias, IExposesUIData, IReferenceByOrdinal
    {
        IAttributeSerializationDefinition SerializationType { get; set; }
    }
}