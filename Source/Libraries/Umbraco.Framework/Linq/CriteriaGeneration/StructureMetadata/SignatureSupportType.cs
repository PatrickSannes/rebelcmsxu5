namespace Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata
{
    /// <summary>
    /// An enumeration of ways in which a method or member signature is supported.
    /// </summary>
    /// <remarks></remarks>
    public enum SignatureSupportType
    {
        NotSupported,
        SupportedAsFieldName,
        SupportedAsFieldPredicate,
        SupportedAsFieldValue,
        SupportedAsSchemaAlias,
        SupportedAsSchemaPredicate,
        SupportedAsSchemaMetaDataValue,
        SupportedAsEntityName,
        SupportedAsEntityPredicate,
        SupportedAsEntityMetaDataValue
    }
}