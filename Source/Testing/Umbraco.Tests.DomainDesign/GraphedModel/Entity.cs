using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Tests.DomainDesign.GraphedModel
{
    public class Entity : IEquatable<Entity>
    {
        /* An object graph would ideally have an Associates property which allows you to get entities based
         * on the relationship type, e.g. standard parent-child and also for things like permissions. However, this would only work if
         * the entire graph was available in a speedy way.
         * What do we actually need for the views and the controllers? Should we make specific calls for "get permissions for this entity"?
         * Is it important to be able to pivot the graph or shall we just stick with a tree instead? 
         * 
         * An alternative is to have some kinds of metadata about an entity, so as well as having 'Children', to also have 'Permissions', 'Related'
         * etc., but to still have a method on the repository which allows for the returning of a list of related entities based on a relation key.
         */

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        protected string StandardRelationshipType = "parent-child";

        protected Entity()
        {
            LocallyStoredChildrenViaDictionary1 = new List<Entity>();
            ClassicLocallyStoredChildren = new HashSet<Entity>();
            Relations = new Dictionary<Entity, HashSet<string>>();
        }

        public Entity(RootEntity rootEntity)
            : this()
        {
            RootEntity = rootEntity;
            GraphCache = RootEntity.Relations2;
        }

        public string Id { get; set; }
        public bool IsRoot { get; set; }
        public Entity Parent { get; set; }

        public IDictionary<Entity, HashSet<string>> Relations { get; private set; }

        public IEnumerable<Entity> ClassicLocallyStoredChildren { get; set; }

        // This is IEnumerable because we don't want standard List operations such as Add on the property, instead caller is responsible for setting entire
        // value overriding contents of the list
        public IEnumerable<Entity> LocallyStoredChildrenViaDictionary1
        {
            get
            {
                //var children = new List<Entity>();

                //foreach (var relation in Relations.Where(x => x.Value.Contains(_standardRelationshipType)))
                //{
                //    children.Add(relation.Key);
                //}

                //return children;

                return GetAssociates(StandardRelationshipType);
            }
            set
            {
                foreach (var entity in value)
                {
                    var relationTypes = Relations.ContainsKey(entity) ? Relations[entity] : new HashSet<string>();

                    if (relationTypes != null)
                    {
                        if (!relationTypes.Any(x => x == StandardRelationshipType))
                        {
                            relationTypes.Add(StandardRelationshipType);
                        }


                        if (Relations.ContainsKey(entity))
                        {
                            Relations[entity] = relationTypes;
                        }
                        else
                        {
                            Relations.Add(entity, relationTypes);
                        }
                    }
                }
            }
        }

        // Test method to get around VS's fucking shit performance analysis on recursive functions
        public IEnumerable<Entity> FakeGetChildren()
        {
            return GraphCache.GetByRelationType(this, StandardRelationshipType).ToList();
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, IsRoot: {1}", Id, IsRoot);
        }

        public IEnumerable<Entity> CentrallyStoredChildren
        {
            get
            {
                //IndexedEntityCollection collection = (GetType() == typeof (RootEntity))
                //                               ? ((RootEntity) this).Relations2
                //                               : RootEntity.Relations2;
                return GraphCache.GetByRelationType(this, StandardRelationshipType);
            }
            set
            {
                foreach (var entity in value)
                {
                    GraphCache.AddWithRelation(this, entity, StandardRelationshipType);
                }

                //var metas = new List<IndexedEntityCollection.Meta>();
                //foreach (var entity in value)
                //{
                //    if (GetType() == typeof (RootEntity))
                //        ((RootEntity) this).Relations2.AddWithRelation(this, entity,
                //                                                       _standardRelationshipType);
                //    else
                //        RootEntity.Relations2.AddWithRelation(this, entity, _standardRelationshipType);
                //}
            }
        }

        public RootEntity RootEntity { get; protected set; }

        protected internal IndexedEntityCollection GraphCache { get; protected set; }

        #region IEquatable<Entity> Members

        public bool Equals(Entity other)
        {
            return other.Id == Id;
        }

        #endregion

        public void SetChildren2(IEnumerable<Entity> list)
        {
            CentrallyStoredChildren = list;
        }

        public IEnumerable<Entity> GetAssociates(string relationType)
        {
            return Relations
                .Where(x => x.Value.Contains(relationType))
                .Select(relation => relation.Key);
        }
    }
}