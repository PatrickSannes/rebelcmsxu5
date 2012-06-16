using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.Model.Associations._Revised;

namespace Umbraco.Framework.Persistence.Model.Associations
{
    [DebuggerDisplay("{SourceId} > {DestinationId} ({Type.RelationName})")]
    public class Relation<TSource, TDestination> : RelationById,
        IRelation<TSource, TDestination>, IReadonlyRelation<TSource, TDestination> 
        where TSource : class, IRelatableEntity
        where TDestination : class, IRelatableEntity
    {
        private TSource _source;
        private TDestination _destination;

        public Relation(TSource source, TDestination destination, AbstractRelationType type, int ordinal, params RelationMetaDatum[] metaData)
            : base(type, ordinal, metaData)
        {
            _source = source;
            _destination = destination;
        }

        private HiveId GetFromEntityOrEmpty(IReferenceByHiveId entity)
        {
            return entity == null ? HiveId.Empty : entity.Id;
        }

                /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class to relate two entities together by Id only.
        /// </summary>
        public Relation(AbstractRelationType type, HiveId sourceId, HiveId destinationId, params RelationMetaDatum[] metaData)
            : base(sourceId, destinationId, type, 0, metaData)
        { }

        public Relation(AbstractRelationType type, HiveId sourceId, HiveId destinationId, int ordinal, params RelationMetaDatum[] metaData)
            : base(sourceId, destinationId, type, ordinal, metaData)
        { }

        public Relation(AbstractRelationType type, TSource source, TDestination destination, params RelationMetaDatum[] metaData) 
            : base(type, 0, metaData)
        {
            Source = source;
            Destination = destination;
        }

        public Relation(AbstractRelationType type, TSource source, HiveId destinationId, int ordinal, params RelationMetaDatum[] metaData)
            : base(type, ordinal, metaData)
        {
            Source = source;
            base.DestinationId = destinationId;
        }

        public Relation(AbstractRelationType type, HiveId sourceId, TDestination destination, int ordinal, params RelationMetaDatum[] metaData)
            : base(type, ordinal, metaData)
        {
            base.SourceId = sourceId;
            Destination = destination;
        }

        public Relation(AbstractRelationType type, TSource source, TDestination destination, int ordinal, params RelationMetaDatum[] metaData)
            : this(type, source, destination, metaData)
        {
            Ordinal = ordinal;
        }

        public virtual TSource Source
        {
            get { return _source; }
            set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (_source == value) return;
                _source = value;
            }
        }

        public virtual TDestination Destination
        {
            get { return _destination; }
            set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (_destination == value) return;
                _destination = value;
            }
        }

        public bool EqualsIgnoringProviderId(IReadonlyRelation<IRelatableEntity, IRelatableEntity> other)
        {
            return base.EqualsIgnoringProviderId(other);
        }

        public override HiveId SourceId
        {
            get { return _source == null ? base.SourceId : _source.Id; }
            protected set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (base.SourceId == value) return;
                base.SourceId = value;
                _source = ClearReferenceIfIdsDiffer(_source, value);
            }
        }

        public override HiveId DestinationId
        {
            get { return _destination == null ? base.DestinationId : _destination.Id; }
            protected set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (base.DestinationId == value) return;
                base.DestinationId = value;
                _destination = ClearReferenceIfIdsDiffer(_destination, value);
            }
        }

        private static T ClearReferenceIfIdsDiffer<T>(T entity, HiveId newId)
            where T : class, IRelatableEntity
        {
            if (entity == null) return null;
            return entity.Id != newId ? null : entity;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(obj, this)) return true;

            var objCast = obj as Relation<TSource, TDestination>;
            if (ReferenceEquals(objCast, null)) return false;

            if (Type != null && objCast.Type != null)
                if (string.Equals(Type.RelationName, objCast.Type.RelationName, StringComparison.InvariantCultureIgnoreCase)
                    && MetaData.OrderBy(x => x.Key).SequenceEqual(objCast.MetaData.OrderBy(x => x.Key))
                    && (SourceId == objCast.SourceId && SourceId != HiveId.Empty || (Source == objCast.Source && Source != null && objCast.Source != null))
                    && (DestinationId == objCast.DestinationId && DestinationId != HiveId.Empty || (Destination == objCast.Destination && Destination != null && objCast.Destination != null)))
                    return true;

            return false;
        }
    }
}
