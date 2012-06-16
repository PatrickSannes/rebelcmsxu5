using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

using System.Linq;

namespace Umbraco.Framework.Persistence.Model.Associations
{
    [DebuggerDisplay("{SourceId} > {DestinationId} ({Type.RelationName})")]
    public class Relation : IReferenceByOrdinal, INotifyPropertyChanged, ICanBeDirty,
        IReadonlyRelation<IRelatableEntity, IRelatableEntity>
    {
        private HiveId _sourceId = HiveId.Empty;
        private HiveId _destinationId = HiveId.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <remarks></remarks>
        protected Relation(AbstractRelationType type)
        {
            // In the base constructor for the overloads, use the field to access to avoid raising PropertyChanged event
            _type = type;
            _metaData = new RelationMetaDataCollection();
            LoadStatus = RelationLoadStatus.Unsaved;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="metaData">The meta data.</param>
        /// <remarks></remarks>
        protected Relation(AbstractRelationType type, params RelationMetaDatum[] metaData)
            : this(type)
        {
            foreach (var relationMetaDatum in metaData)
            {
                MetaData.Add(relationMetaDatum);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class to relate two entities together by Id only.
        /// </summary>
        public Relation(AbstractRelationType type, HiveId sourceId, HiveId destinationId, params RelationMetaDatum[] metaData)
            : this(type, metaData)
        {
            SourceId = sourceId;
            DestinationId = destinationId;
        }

        public Relation(AbstractRelationType type, HiveId sourceId, HiveId destinationId, int ordinal, params RelationMetaDatum[] metaData)
            : this(type, sourceId, destinationId, metaData)
        {
            Ordinal = ordinal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class to relate two entities together by reference.
        /// </summary>
        public Relation(AbstractRelationType type, IRelatableEntity source, IRelatableEntity destination, params RelationMetaDatum[] metaData) 
            : this(type, metaData)
        {
            Source = source;
            Destination = destination;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class with an ordinal
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="metaData">The meta data.</param>
        public Relation(AbstractRelationType type, IRelatableEntity source, IRelatableEntity destination, int ordinal, params RelationMetaDatum[] metaData)
            : this(type, source, destination, metaData)
        {
            Ordinal = ordinal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class to relate two entities together by a reference to the source instance and referring to the destination by Id only.
        /// </summary>
        public Relation(AbstractRelationType type, IRelatableEntity source, HiveId destinationId, params RelationMetaDatum[] metaData)
            : this(type, metaData)
        {
            Source = source;
            DestinationId = destinationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class with an ordinal
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="source">The source.</param>
        /// <param name="destinationId">The destination id.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="metaData">The meta data.</param>
        public Relation(AbstractRelationType type, IRelatableEntity source, HiveId destinationId, int ordinal, params RelationMetaDatum[] metaData)
            : this(type, source, destinationId, metaData)
        {
            Ordinal = ordinal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class to relate two entities together by a reference to the destination instance and referring to the source by Id only.
        /// </summary>
        public Relation(AbstractRelationType type, HiveId sourceId, IRelatableEntity destination, params RelationMetaDatum[] metaData)
            : this(type, metaData)
        {
            SourceId = sourceId;
            Destination = destination;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relation"/> class with an ordinal
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="sourceId">The source id.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="metaData">The meta data.</param>
        public Relation(AbstractRelationType type, HiveId sourceId, IRelatableEntity destination, int ordinal, params RelationMetaDatum[] metaData)
            : this(type, sourceId, destination, metaData)
        {
            Ordinal = ordinal;
        }

        /// <summary>
        /// Tracks the properties that have changed
        /// </summary>
        private readonly IDictionary<string, bool> _propertyChangedInfo = new Dictionary<string, bool>();

        private AbstractRelationType _type;
        public AbstractRelationType Type
        {
            get { return _type; }
            set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (_type == value) return;
                _type = value;
                OnPropertyChanged(x => x.Type);
            }
        }

        private RelationMetaDataCollection _metaData;
        public RelationMetaDataCollection MetaData
        {
            get { return _metaData; }
            private set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (_metaData == value) return;
                _metaData = value;
                OnPropertyChanged(x => x.MetaData);
            }
        }

        private IRelatableEntity _source;
        public IRelatableEntity Source
        {
            get { return _source; }
            set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (_source == value) return;
                _source = value;
                OnPropertyChanged(x => x.Source);
            }
        }

        private IRelatableEntity _destination;
        public IRelatableEntity Destination
        {
            get { return _destination; }
            set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (_destination == value) return;
                _destination = value;
                OnPropertyChanged(x => x.Destination);
            }
        }

        public RelationLoadStatus LoadStatus { get; set; }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(int other)
        {
            return ((IComparable) Ordinal).CompareTo(other);
        }

        public override int GetHashCode()
        {
            var metadataHashcode = MetaData == null ? 42 : MetaData.GetHashCode();
            var relationName = Type == null ? "nullRelation" : Type.RelationName.ToLowerInvariant();
            return string.Concat(relationName, metadataHashcode, SourceId, DestinationId).GetHashCode();
        }

        public bool EqualsIgnoringProviderId(IReadonlyRelation<IRelatableEntity, IRelatableEntity> other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;

            var objCast = other as Relation;
            if (ReferenceEquals(objCast, null)) return false;

            var equals = true;

            if (Type != other.Type) equals = false;
            if (!SourceId.EqualsIgnoringProviderId(other.SourceId)) equals = false;
            if (!DestinationId.EqualsIgnoringProviderId(other.DestinationId)) equals = false;
            if (!MetaData.OrderBy(x => x.Key).SequenceEqual(objCast.MetaData.OrderBy(x => x.Key))) equals = false;

            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(obj, this)) return true;

            var objCast = obj as Relation;
            if (ReferenceEquals(objCast, null)) return false;

            if (Type != null && objCast.Type != null)
                if (string.Equals(Type.RelationName, objCast.Type.RelationName, StringComparison.InvariantCultureIgnoreCase)
                    && MetaData.OrderBy(x => x.Key).SequenceEqual(objCast.MetaData.OrderBy(x => x.Key))
                    && (SourceId == objCast.SourceId && SourceId != HiveId.Empty || (Source == objCast.Source && Source != null && objCast.Source != null))
                    && (DestinationId == objCast.DestinationId && DestinationId != HiveId.Empty || (Destination == objCast.Destination && Destination != null && objCast.Destination != null)))
                    return true;

            return false;
        }

        private int _ordinal;

        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        public int Ordinal
        {
            get { return _ordinal; }
            set {
                // Check incoming to avoid raising PropertyChanged event
                if (_ordinal == value) return;
                _ordinal = value;
                OnPropertyChanged(x => x.Ordinal);
            }
        }

        /// <summary>
        /// Gets or sets the source id. Note: if the incoming Id differs from the existing <see cref="Source"/> object's Id, <see cref="Source"/> will be set to null.
        /// </summary>
        /// <value>The source id.</value>
        /// <remarks></remarks>
        public HiveId SourceId
        {
            get { return Source == null ? _sourceId : Source.Id; }
            set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (_sourceId == value) return;
                _sourceId = value;
                Source = ClearReferenceIfIdsDiffer(Source, value);
                OnPropertyChanged(x => x.SourceId);
            }
        }

        /// <summary>
        /// Gets or sets the destination id. Note: if the incoming Id differs from the existing <see cref="Destination"/> object's Id, <see cref="Destination"/> will be set to null.
        /// </summary>
        /// <value>The destination id.</value>
        /// <remarks></remarks>
        public HiveId DestinationId
        {
            get { return Destination == null ? _destinationId : Destination.Id; }
            set
            {
                // Check incoming to avoid raising PropertyChanged event
                if (_destinationId == value) return;
                _destinationId = value;
                Destination = ClearReferenceIfIdsDiffer(Destination, value);
                OnPropertyChanged(x => x.DestinationId);
            }
        }

        /// <summary>
        /// Checks <paramref name="entity.Id"/>; if it is different to <paramref name="newId"/> then returns null otherwise returns <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="newId">The new id.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static IRelatableEntity ClearReferenceIfIdsDiffer(IRelatableEntity entity, HiveId newId)
        {
            if (entity == null) return null;
            return entity.Id != newId ? null : entity;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks></remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when a property is changed.
        /// </summary>
        /// <param name="propSelector"></param>
        protected virtual void OnPropertyChanged<TPropertyType>(Expression<Func<Relation, TPropertyType>> propSelector)
        {
            var propInfo = this.GetPropertyInfo(propSelector);

            _propertyChangedInfo[propInfo.Name] = true;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propInfo.Name));
            }
        }

        /// <summary>
        /// Returns true if the property referenced in the selector has been changed on the class
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propSelector"></param>
        /// <returns></returns>
        public bool IsPropertyDirty<TSource, TProperty>(Expression<Func<TSource, TProperty>> propSelector)
            where TSource : class
        {
            var source = this as TSource;
            if (source == null)
                throw new InvalidCastException("Could not cast type " + this.GetType() + " to " + typeof(TSource));
            var propInfo = source.GetPropertyInfo(propSelector);
            return _propertyChangedInfo.Any(x => x.Key == propInfo.Name);
        }

        /// <summary>
        /// Returns true if any properties have been changed on the class
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return _propertyChangedInfo.Any();
        }

    }
}
