using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive
{
    public static class EntityExtensions
    {
        public static TypedAttribute CreateShallowCopy(this TypedAttribute attribute)
        {
            Mandate.ParameterNotNull(attribute, "attribute");

            var copied = new TypedAttribute(attribute.AttributeDefinition);
            foreach (var v in attribute.Values)
            {
                copied.Values.Add(v);
            }
            copied.Id = HiveId.Empty;
            copied.UtcCreated = DateTimeOffset.UtcNow;
            copied.UtcModified = attribute.UtcModified;
            copied.UtcStatusChanged = attribute.UtcStatusChanged;
            return copied;
        }

        public static T CreateShallowCopy<T>(this T entity)
            where T : TypedEntity, new()
        {
            Mandate.ParameterNotNull(entity, "entity");

            var copied = new T
                {
                    EntitySchema = entity.EntitySchema
                };
            foreach (var a in entity.Attributes)
            {
                copied.Attributes.Add(a.CreateShallowCopy());
            }
            copied.Id = HiveId.Empty;
            copied.UtcCreated = DateTimeOffset.UtcNow;
            copied.UtcModified = entity.UtcModified;
            copied.UtcStatusChanged = entity.UtcStatusChanged;
            copied.RelationProxies.LazyLoadDelegate = entity.RelationProxies.LazyLoadDelegate;
            return copied;
        }

        public static T CreateDeepCopyToNewParent<T>(this T original, IRelatableEntity newParent, AbstractRelationType relationType, int ordinal, params RelationMetaDatum[] metaData)
            where T : TypedEntity, new()
        {
            Mandate.ParameterNotNull(newParent, "newParent");
            Mandate.ParameterNotNull(relationType, "relationType");

            var copy = CreateCopyAndEnlistChildren(original);

            // We've been given a new parent so add that to the copied item's relatinproxies
            copy.RelationProxies.EnlistParent(newParent, relationType, ordinal, metaData);

            return copy;
        }

        /// <summary>
        /// Creates a deep copy of an object and adds the copied children to the repository. Does not add the root duplicate to the repo, the caller should do this.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original">The original.</param>
        /// <param name="newParent">The new parent.</param>
        /// <param name="relationType">Type of the relation.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="metaData">The meta data.</param>
        /// <returns></returns>
        public static T CreateDeepCopyToNewParentInRepo<T>(this T original, IRelatableEntity newParent, AbstractRelationType relationType, int ordinal, IGroupUnit<IProviderTypeFilter> unitOfWork, params RelationMetaDatum[] metaData)
            where T : TypedEntity, new()
        {
            Mandate.ParameterNotNull(newParent, "newParent");
            Mandate.ParameterNotNull(relationType, "relationType");

            var copy = CreateCopyAndEnlistChildren(original, unitOfWork);

            // We've been given a new parent so add that to the copied item's relatinproxies
            copy.RelationProxies.EnlistParent(newParent, relationType, ordinal, metaData);

            return copy;
        }

        public static T CreateDeepCopy<T>(this T original)
            where T : TypedEntity, new()
        {
            var copy = CreateCopyAndEnlistChildren(original);

            // Go through each of the parent relation proxies, and create a new relation between the original parent
            // and the new copy
            foreach (var relationProxy in original.RelationProxies.AllParentRelations().ToArray())
            {
                var parent = relationProxy.Item.Source;
                if (parent == null)
                {
                    var parentId = relationProxy.Item.SourceId;

                    if (parentId.IsNullValueOrEmpty())
                        throw new InvalidOperationException(
                            "Cannot copy an entity's relations unless the parent entity's id is available, but it's empty");

                    copy.RelationProxies.EnlistParentById(
                        parentId,
                        relationProxy.Item.Type,
                        relationProxy.Item.Ordinal,
                        relationProxy.Item.MetaData.ToArray());
                }
                else
                    copy.RelationProxies.EnlistParent(parent, relationProxy.Item.Type, relationProxy.Item.Ordinal, relationProxy.Item.MetaData.ToArray());
            }

            return copy;
        }

        private static T CreateCopyAndEnlistChildren<T>(T original) where T : TypedEntity, new()
        {
            var copy = original.CreateShallowCopy();

            // Go through each of the child relation proxies and create a new proxy in the copied item
            foreach (var relationProxy in original.RelationProxies.AllChildRelations())
            {
                var childToCopy = relationProxy.Item.Destination;
                if (childToCopy == null)
                    throw new InvalidOperationException(
                        "Cannot copy an entity's relations unless the entity is available as a hydrated object, it's only available as id {0}"
                            .InvariantFormat(relationProxy.Item.DestinationId));

                var castChildToCopy = childToCopy as TypedEntity;
                if (castChildToCopy != null)
                {
                    // Copy the related item and recurse any children it has enlisted
                    var copiedChild = CreateCopyAndEnlistChildren(castChildToCopy);

                    // Add it to our outgoing copy's relation proxies
                    copy.RelationProxies.EnlistChild(copiedChild, relationProxy.Item.Type, relationProxy.Item.MetaData.ToArray());
                }
            }
            return copy;
        }

        private static T CreateCopyAndEnlistChildren<T>(T original, IGroupUnit<IProviderTypeFilter> unitOfWork) where T : TypedEntity, new()
        {
            var copy = original.CreateShallowCopy();

            // Go through each of the child relation proxies and create a new proxy in the copied item
            foreach (var relationProxy in original.RelationProxies.AllChildRelations())
            {
                var childToCopyId = relationProxy.Item.DestinationId;
                var childToCopy = unitOfWork.Repositories.Get<T>(childToCopyId);
                if (childToCopy == null)
                    throw new InvalidOperationException(
                        "Cannot copy an entity's relations unless the entity is available as a hydrated object, it's only available as id {0}"
                            .InvariantFormat(relationProxy.Item.DestinationId));

                // Copy the related item and recurse any children it has enlisted
                var copiedChild = CreateCopyAndEnlistChildren(childToCopy, unitOfWork);

                // Add it to our outgoing copy's relation proxies
                copy.RelationProxies.EnlistChild(copiedChild, relationProxy.Item.Type, relationProxy.Item.MetaData.ToArray());

                // Add the duplicate to the repo
                unitOfWork.Repositories.AddOrUpdate(copiedChild);
            }
            return copy;
        }

        /// <summary>
        /// Based on the type passed in, determines what base entity type it is, if it is not found to be a base entity type, an exception is thrown
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Type GetEntityBaseType(this Type e)
        {
            if (typeof(TypedEntity).IsAssignableFrom(e))
            {
                return typeof(TypedEntity);
            }
            if (typeof(EntitySchema).IsAssignableFrom(e))
            {
                return typeof(EntitySchema);
            }
            if (typeof(AttributeType).IsAssignableFrom(e))
            {
                return typeof(AttributeType);
            }
            if (typeof(AttributeGroup).IsAssignableFrom(e))
            {
                return typeof(AttributeGroup);
            }
            if (typeof(AttributeDefinition).IsAssignableFrom(e))
            {
                return typeof(AttributeDefinition);
            }
            if (typeof(TypedAttribute).IsAssignableFrom(e))
            {
                return typeof(TypedAttribute);
            }
            throw new NotSupportedException("Could not determine the base entity type of the type passed in: " + e.FullName);
        }

        /// <summary>
        /// Determines what kind of entity it is and returns its base type
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Type GetEntityBaseType(this AbstractEntity e)
        {
            return e.GetType().GetEntityBaseType();
        }

        /// <summary>
        /// Iterates through all identifiable items in the object graph and assigns Ids to entities that don't have an id assigned using the idGenerator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="idGenerator"></param>
        public static void AssignIds<T>(this Revision<T> obj, Func<HiveId> idGenerator)
            where T : class, IVersionableEntity
        {
            //check if its a revision, if so then select it's TypedEntity and RevisionData entities,
            // then select all of their related identifiable items that don't have ids assigned
            var entity = ((dynamic)obj).Item;
            var revData = ((dynamic)obj).MetaData;
            var revisions = (from allEntityData in ((TypedEntity)entity).GetAllIdentifiableItems()
                             from allRevData in ((RevisionData)revData).GetAllIdentifiableItems()
                             select new[] { allRevData, allEntityData })
                .SelectMany(x => x)
                .Where(x => x.Id.IsNullValueOrEmpty())
                .ToArray();

            //now that we have a unique list of all new entities that reqiure ids, lets generate them
            foreach (var e in revisions)
            {
                //ensure we're not re-creating an id for the same entity
                if (e.Id.IsNullValueOrEmpty())
                {
                    e.Id = idGenerator();
                }
            }
        }

        /// <summary>
        /// Iterates through all identifiable items in the object graph and assigns Ids to entities that don't have an id assigned using the idGenerator.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="idGenerator"></param>
        public static void AssignIds(this IReferenceByHiveId obj, Func<HiveId> idGenerator)
        {
            //get all identifiable entities that don't have ids assigned
            var entities = (from all in obj.GetAllIdentifiableItems()
                            where all.Id.IsNullValueOrEmpty()
                            select all).ToArray();

            //now that we have a unique list of all new entities that reqiure ids, lets generate them
            foreach (var e in entities)
            {
                //ensure we're not re-creating an id for the same entity
                if (e.Id.IsNullValueOrEmpty())
                {
                    e.Id = idGenerator();
                }
            }
        }

        /// <summary>
        /// Returns a list of all identifyable items in a Revision{T}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="revision"></param>
        /// <returns></returns>
        public static IEnumerable<IReferenceByHiveId> GetAllIdentifiableItems<T>(this Revision<T> revision)
            where T : class, IVersionableEntity
        {
            return revision.Item.GetAllIdentifiableItems()
                .Concat(revision.MetaData.GetAllIdentifiableItems());
        }

        /// <summary>
        /// Returns a list of all associated IReferenceByHiveId entities associated with the entity type passed 
        /// in. This will recusively find all entities attached to the item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<IReferenceByHiveId> GetAllIdentifiableItems(this IReferenceByHiveId item)
        {
            if (item == null) yield break;
            var type = item.GetType();
            yield return item;
            if (TypeFinder.IsTypeAssignableFrom<TypedEntity>(type))
            {
                var casted = (TypedEntity)item;
                foreach (var allIdentifiableItem in casted.EntitySchema.GetAllIdentifiableItems())
                {
                    yield return allIdentifiableItem;
                }
                foreach (var attribute in casted.Attributes)
                {
                    yield return attribute;
                    foreach (var allIdentifiableItem in attribute.AttributeDefinition.GetAllIdentifiableItems())
                    {
                        yield return allIdentifiableItem;
                    }
                }
            }
            if (TypeFinder.IsTypeAssignableFrom<EntitySchema>(type))
            {
                var casted = (EntitySchema)item;
                foreach (var attributeDefinition in casted.AttributeDefinitions)
                {
                    // Don't call back into GetAllIdentifiable items here, rather
                    // separately iterate through the types and groups explicitly set
                    // on the Schema to include those that aren't tied to an attribute definition
                    yield return attributeDefinition;
                }
                foreach (var attributeType in casted.AttributeTypes)
                {
                    yield return attributeType;
                }
                foreach (var attributeGroup in casted.AttributeGroups)
                {
                    yield return attributeGroup;
                }
            }
            if (TypeFinder.IsTypeAssignableFrom<TypedAttribute>(type))
            {
                var casted = (TypedAttribute)item;
                foreach (var allIdentifiableItem in casted.AttributeDefinition.GetAllIdentifiableItems())
                {
                    yield return allIdentifiableItem;
                }
            }
            if (TypeFinder.IsTypeAssignableFrom<AttributeDefinition>(type))
            {
                var casted = (AttributeDefinition)item;
                yield return casted;
                yield return casted.AttributeType;
                if (casted.AttributeGroup != null) yield return casted.AttributeGroup;
            }
        }

        /// <summary>
        /// This recursively finds all 'new' entities in the object graph which require Ids
        /// </summary>
        /// <param name="e">The entity to search on and its related entities</param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: Do we need to support checking Relations here?
        /// </remarks>
        public static IEnumerable<AbstractEntity> FindNewEntitiesInGraph(this AbstractEntity e)
        {

            var list = new List<AbstractEntity>();

            if (e is TypedEntity)
            {
                var te = (TypedEntity)e;
                list.AddRange(te.EntitySchema.FindNewEntitiesInGraph());
                foreach (var a in te.Attributes)
                {
                    list.AddRange(a.FindNewEntitiesInGraph());
                }
            }
            else if (e is EntitySchema)
            {
                var es = (EntitySchema)e;
                foreach (var ad in es.AttributeDefinitions)
                {
                    list.AddRange(ad.FindNewEntitiesInGraph());
                }
                foreach (var g in es.AttributeGroups)
                {
                    list.AddRange(g.FindNewEntitiesInGraph());
                }
            }
            else if (e is AttributeType)
            {
                //no children to add
            }
            else if (e is AttributeGroup)
            {
                //no children to add
            }
            else if (e is AttributeDefinition)
            {
                var ad = (AttributeDefinition)e;
                list.AddRange(ad.AttributeType.FindNewEntitiesInGraph());
            }
            else if (e is TypedAttribute)
            {
                var ta = (TypedAttribute)e;
                list.AddRange(ta.AttributeDefinition.FindNewEntitiesInGraph());
            }

            //now, check this entity to see if it's new and add it to our list
            if (e.Id.IsNullValueOrEmpty())
            {
                list.Add(e);
            }

            return list.Distinct();
        }


        /// <summary>
        /// Returns the published date if it is published, otherwise returns null
        /// </summary>
        /// <param name="rev"></param>
        /// <returns></returns>
        public static DateTimeOffset? PublishedDate(this EntitySnapshot<TypedEntity> rev)
        {
            if (rev == null) return null;

            var lastPublished = rev.GetLatestDate(FixedStatusTypes.Published);
            var lastUnpublished = rev.GetLatestDate(FixedStatusTypes.Unpublished);

            return lastPublished > lastUnpublished
                       ? (DateTimeOffset?)lastPublished
                       : null;
        }

        /// <summary>
        /// Determines whether the specified rev is published.
        /// </summary>
        /// <param name="rev">The rev.</param>
        /// <returns>
        ///   <c>true</c> if the specified rev is published; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPublished(this EntitySnapshot<TypedEntity> rev)
        {
            return rev.PublishedDate() != null;
        }

        /// <summary>
        /// Determines whether the revision has a pending publish to be made
        /// </summary>
        /// <param name="rev">The rev.</param>
        /// <returns>
        ///   <c>true</c> if [is publish pending] [the specified rev]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPublishPending(this EntitySnapshot<TypedEntity> rev)
        {
            if (rev == null) return false;

            var lastPublished = rev.GetLatestDate(FixedStatusTypes.Published);
            var lastSaved = rev.GetLatestDate(FixedStatusTypes.Draft);

            return lastSaved > lastPublished;
        }
    }
}
