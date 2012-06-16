using System.Diagnostics;

namespace Umbraco.Framework.Persistence.Model.Associations
{
    [DebuggerDisplay("{SourceId} > {DestinationId} ({Type.RelationName})")]
    public class Relation : Relation<IRelatableEntity, IRelatableEntity>
    {
        public Relation(IRelatableEntity source, IRelatableEntity destination, AbstractRelationType type, int ordinal, params RelationMetaDatum[] metaData)
            : base(source, destination, type, ordinal, metaData)
        {
        }

        public Relation(AbstractRelationType type, HiveId sourceId, HiveId destinationId, params RelationMetaDatum[] metaData) : base(type, sourceId, destinationId, metaData)
        {
        }

        public Relation(AbstractRelationType type, HiveId sourceId, HiveId destinationId, int ordinal, params RelationMetaDatum[] metaData) : base(type, sourceId, destinationId, ordinal, metaData)
        {
        }

        public Relation(AbstractRelationType type, IRelatableEntity source, IRelatableEntity destination, params RelationMetaDatum[] metaData) : base(type, source, destination, metaData)
        {
        }

        public Relation(AbstractRelationType type, IRelatableEntity source, IRelatableEntity destination, int ordinal, params RelationMetaDatum[] metaData) : base(type, source, destination, ordinal, metaData)
        {
        }

        public Relation(AbstractRelationType type, HiveId sourceId, IRelatableEntity destination, int ordinal, params RelationMetaDatum[] metaData) : base(type, sourceId, destination, ordinal, metaData)
        {
        }

        public Relation(AbstractRelationType type, IRelatableEntity source, HiveId destinationId, int ordinal, params RelationMetaDatum[] metaData) : base(type, source, destinationId, ordinal, metaData)
        {
        }
    }
}