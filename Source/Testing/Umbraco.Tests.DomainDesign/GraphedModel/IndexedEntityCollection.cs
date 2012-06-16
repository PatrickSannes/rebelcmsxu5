using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Tests.DomainDesign.GraphedModel
{
    public class IndexedEntityCollection
    {
        /*
         * List of Entities
         * List of how one entity is related to another
         */

        private readonly Dictionary<string, Entity> _cachedById = new Dictionary<string, Entity>();
        private readonly TestEntityAndRelationshipCache _cacheByRelationshipType = new TestEntityAndRelationshipCache();
        private readonly HashSet<Entity> _store = new HashSet<Entity>();

        public void Add(Entity entity)
        {
            EnsureIsAdded(entity);
        }

        public void AddRange(IEnumerable<Meta> entities)
        {
        }

        public IEnumerable<Entity> GetAll()
        {
            return _store;
        }

        public IEnumerable<Meta> GetByRelationType(string relationType)
        {
            var result = _cacheByRelationshipType[relationType].Values;
            return result.SelectMany(metas => metas);
        }

        public IEnumerable<Entity> GetByRelationType(Entity vertex, string relationType)
        {
            //return _cacheByRelationshipType.GetSubGroup(relationType, vertex.Id).Select(x => x.AssociatedEntity);
            //foreach (var entity in GetByRelationType(relationType).Where(x => x.Entity.Equals(vertex)))
            //{
            //    yield return entity.AssociatedEntity;
            //} 
            //return GetByRelationType(relationType).Where(x => x.Entity.Id == vertex.Id).Select(x => x.AssociatedEntity).ToList();
            var dictionary = _cacheByRelationshipType[relationType];

            //Works with TestIndex v2
            //if (dictionary.Contains(vertex))
            //    return dictionary[vertex].Select(x => x.AssociatedEntity);
            //else
            //    return new Entity[] { };

            //Works with TestIndex v1 etc
            //if (dictionary.ContainsKey(vertex.Id))
            //    return dictionary[vertex.Id].Select(x => x.AssociatedEntity);
            //else
            //    return new Entity[] { };
            //    
            //Works with TestIndex v1 etc
            return dictionary.ContainsKey(vertex.Id) ? dictionary[vertex.Id].Select(x => x.AssociatedEntity) : new Entity[] { };
        }

        public Entity GetById(string id)
        {
            if (_cachedById.ContainsKey(id)) return _cachedById[id];
            var result = _store.FirstOrDefault(x => x.Id == id);
            if (result != null) _cachedById.Add(id, result);
            return result;
        }

        private void EnsureIsAdded(Entity entity)
        {
            if (!_store.Contains(entity)) _store.Add(entity);
            if (!_cachedById.ContainsKey(entity.Id)) _cachedById.Add(entity.Id, entity);
        }

        private void EnsureAssociationIsRegistered(Entity entity, Entity associatedEntity, string associationType)
        {
            var meta = new Meta { Entity = entity, AssociatedEntity = associatedEntity, AssociationType = associationType };

            //if (!_relationStore.Any(
            //    x =>
            //    x.Entity.Equals(entity) && x.AssociatedEntity.Equals(associatedEntity) &&
            //    x.AssociationType == associationType))
            //    _relationStore.Add(meta);

            _cacheByRelationshipType.Add(associationType, meta);
        }

        public void AddWithRelation(Entity entity, Entity associatedEntity, string associationType)
        {
            EnsureIsAdded(entity);
            EnsureIsAdded(associatedEntity);
            EnsureAssociationIsRegistered(entity, associatedEntity, associationType);
        }

        #region Nested type: Meta

        public class Meta : IEquatable<Meta>
        {
            public bool Equals(Meta other)
            {
                var entityEquals = Entity.Equals(other.Entity);
                var associationEquals = AssociatedEntity.Equals(other.AssociatedEntity);
                var typeEquals = AssociationType == other.AssociationType;

                if (entityEquals && associationEquals && typeEquals)
                    return true;
                return false;
            }

            public override string ToString()
            {
                return string.Format("EntityId: {0}, AssociateId: {1}, Type: {2}", Entity.Id, AssociatedEntity.Id,
                                     AssociationType);
            }
            public Entity Entity { get; set; }
            public Entity AssociatedEntity { get; set; }
            public string AssociationType { get; set; }
        }

        #endregion
    }
}