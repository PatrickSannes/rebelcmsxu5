namespace Umbraco.Framework.Persistence.Model.Associations
{
    public interface IRelationById
    {
        HiveId SourceId { get; }
        HiveId DestinationId { get; }
        AbstractRelationType Type { get; }
        RelationMetaDataCollection MetaData { get; }
        int Ordinal { get; }
        bool EqualsIgnoringProviderId(IRelationById other);
    }
}