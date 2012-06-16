using System.Diagnostics.Contracts;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData
{
    /// <summary>
    ///   Defines a type boundary for an attribute
    /// </summary>
    [ContractClass(typeof (IAttributeTypeDefinitionCodeContract))]
    public interface IAttributeTypeDefinition : IEntity, IReferenceByAlias, IExposesUIData, IReferenceByOrdinal
    {
        IAttributeSerializationDefinition SerializationType { get; set; }
    }
}