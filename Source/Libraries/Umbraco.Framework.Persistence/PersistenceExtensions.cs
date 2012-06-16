using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Framework.Data.Common;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.RdbmsModel;
using AttributeDefinition = Umbraco.Framework.Persistence.Model.Attribution.MetaData.AttributeDefinition;
using AttributeType = Umbraco.Framework.Persistence.Model.Attribution.MetaData.AttributeType;

namespace Umbraco.Framework.Persistence
{

    public static class PersistenceExtensions
    {
        public static string GetProviderAliasFromProviderKey(this string providerKey)
        {
            //Validate this key, it must be prefixed with our standard prefixes and then contain characters after that
            if ((providerKey.StartsWith(PersistenceProviderConstants.ReaderKeyPrefix) || providerKey.StartsWith(PersistenceProviderConstants.WriterKeyPrefix))
                && providerKey.IndexOf('-') < (providerKey.Length - 2))
            {
                return providerKey.Substring(providerKey.IndexOf('-') + 1, providerKey.Length - providerKey.IndexOf('-') - 1);
            }

            throw new FormatException("The provider key must be prefixed with \"r-\" or \"rw-\" and must contain more than 1 character after the prefix");
        }

        /// <summary>
        /// This will remove all of the nodes relations (incoming, outgoing, and cached) even though they are lazy loaded
        /// </summary>
        /// <param name="node"></param>
        public static void ClearAllRelationsWithProxy(this Node node)
        {
            node.OutgoingRelationCaches.EnsureClearedWithProxy();
            node.OutgoingRelations.EnsureClearedWithProxy();
            node.IncomingRelationCaches.EnsureClearedWithProxy();
            node.IncomingRelations.EnsureClearedWithProxy();
        }

        public static void EnsureClearedWithProxy<T>(this ICollection<T> collection)
        {
            // Workaround for a lazy-loading bug in NH: calling Clear on a lazy-loaded collection will do nothing if it's not already loaded
            // FUCKING HELL - official response from Alex
            collection.ToArray();
            collection.Clear();
        }

        /// <summary>
        /// Gets an attribute from the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        /// <remarks>This overload exists because it's not possible to have an optional parameter in an expression tree, so <see cref="Attribute{T}(Umbraco.Framework.Persistence.Model.TypedEntity,string,T)"/> cannot be used in queries.</remarks>
        public static T Attribute<T>(this TypedEntity entity, string alias)
        {
            return Attribute<T>(entity, alias, default(T));
        }

        /// <summary>
        /// Gets an attribute from the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static T Attribute<T>(this TypedEntity entity, string alias, T defaultValue = default(T))
        {
            if (!entity.Attributes.Any() || !entity.Attributes.Contains(alias)) return defaultValue;
            var attribute = entity.Attributes[alias];

            var result = attribute.TryGetDefaultValue<T>();
            return result.Success ? result.Result : defaultValue;

            //if (attribute == null || attribute.DynamicValue == null) return default(T);
            //var converted = ObjectExtensions.TryConvertTo<T>(attribute.DynamicValue);
            //return converted.Success ? converted.Result : default(T);
        }

        /// <summary>
        /// Gets a value from an attribute with the specified value key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="alias"></param>
        /// <param name="valueKey"></param>
        /// <returns></returns>
        public static T Attribute<T>(this TypedEntity entity, string alias, string valueKey)
        {
            if (!entity.Attributes.Contains(alias)) return default(T);
            var attribute = entity.Attributes[alias];
            if (attribute == null) return default(T);
            var value = attribute.Values[valueKey];
            if (value == null) return default(T);
            var converted = value.TryConvertTo<T>();
            return converted.Success ? converted.Result : default(T);
        }


        public static bool TrySetValue(this TypedAttributeCollection attributeCollection, string attributeName, string value)
        {
            var attribute = attributeCollection.SingleOrDefault(x => x.AttributeDefinition.Alias == attributeName);
            if (attribute == null) return false;
            attribute.DynamicValue = value;
            return true;
        }

        /// <summary>
        /// Adds a property to the collection if it doesn't exist and returns true, otherwise returns false
        /// </summary>
        /// <param name="attributeCollection"></param>
        /// <param name="typedAttribute"></param>
        /// <returns></returns>
        public static bool TryAdd(this TypedAttributeCollection attributeCollection, TypedAttribute typedAttribute)
        {
            if (!attributeCollection.Any(x => x.AttributeDefinition.Alias == typedAttribute.AttributeDefinition.Alias))
            {
                //NOTE: Here we need to generate an id for this item.                
                // Though Id generation should generally be performed by Hive providers, a TypedAttribute can exist without an
                // id in the repository when a AttributeDefinition is created on the Schema and there are no TypeEntity revisions
                // for that schema with the updated AttributeDefinition.
                if (typedAttribute.Id.IsNullValueOrEmpty())
                    typedAttribute.Id = new HiveId(Guid.NewGuid());

                attributeCollection.Add(typedAttribute);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets
        /// </summary>
        /// <param name="attributeCollection"></param>
        /// <param name="typedAttribute"></param>
        public static void SetValueOrAdd(this TypedAttributeCollection attributeCollection, TypedAttribute typedAttribute)
        {
            var attribute = attributeCollection.SingleOrDefault(x => x.AttributeDefinition.Alias == typedAttribute.AttributeDefinition.Alias);
            if (attribute == null)
                attributeCollection.Add(typedAttribute);
            else
            {
                attribute.Values.Clear();
                typedAttribute.Values.ForEach(attribute.Values.Add);
            }
        }

        /// <summary>
        /// this will add the entity to the collection if it doesn't exist, otherwise will return false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="entity"></param>
        /// <returns>Returns true if the addition was successful, otherwise false if the entity already existed</returns>
        public static bool TryAdd<T>(this EntityCollection<T> collection, T entity)
            where T : AbstractEntity, IReferenceByAlias
        {
            if (!collection.Any(x => x.Alias == entity.Alias))
            {
                collection.Add(entity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// this will add the definition to the schema's attribute definition collection and attach it to the 'general group' if it doesn't exist 
        /// otherwise will return false.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="def"></param>
        /// <returns>Returns true if the property was added otherwise returns false</returns>
        public static bool TryAddAttributeDefinition(this EntitySchema schema, AttributeDefinition def)
        {
            if (def.AttributeGroup == null)
            {
                //check if we have a general group
                var generalGroup = schema.AttributeGroups.SingleOrDefault(x => x.Alias == FixedGroupDefinitions.GeneralGroupAlias);

                //if a general group already exists, use it. If it doesn't exist it will automatically get created
                if (generalGroup != null)
                    def.AttributeGroup = generalGroup;
            }

            return schema.AttributeDefinitions.TryAdd(def);

            //if (schema.AttributeDefinitions.TryAdd(def))
            //{
            //    //check if we have a general group
            //    var generalGroup =
            //        schema.AttributeGroups.Where(x => x.Alias == FixedGroupDefinitions.GeneralGroupAlias)
            //            .SingleOrDefault();
            //    //if a general group already exists, use it. If it doesn't exist it will automatically get created
            //    if (generalGroup != null)
            //        def.AttributeGroup = generalGroup;
            //    //schema.AttributeDefinitions.Add(def);
            //}
            //return false;
        }

        /// <summary>
        /// Gets the latest date from a collection of <see cref="RevisionData"/> where the element's <see cref="RevisionStatusType"/> matches
        /// <paramref name="type"/>.
        /// </summary>
        public static DateTimeOffset GetLatestDate<T>(this EntitySnapshot<T> snapshot, RevisionStatusType type)
            where T : TypedEntity
        {
            Mandate.ParameterNotNull(snapshot, "coll");
            Mandate.ParameterNotNull(type, "type");

            return GetLatestDate(snapshot, type.Alias);
        }

        /// <summary>
        /// Gets the latest date from an EntitySnapshot
        /// <paramref name="alias"/>.
        /// </summary>
        public static DateTimeOffset GetLatestDate<T>(this EntitySnapshot<T> snapshot, string alias)
            where T : TypedEntity
        {
            Mandate.ParameterNotNull(snapshot, "coll");
            Mandate.ParameterNotNullOrEmpty(alias, "alias");

            //TODO: This is currenty a work around for an issue where NH isn't adding the current item's revision data to the revision list data. SD. 07/11/2011
            var latestInItem = snapshot.Revision.MetaData.StatusType.Alias == alias ? snapshot.Revision.MetaData.UtcStatusChanged : DateTimeOffset.MinValue;

            var latestInList = snapshot.EntityRevisionStatusList.OrderByDescending(x => x.UtcStatusChanged).FirstOrDefault(x => x.StatusType.Alias == alias);

            var latest = latestInList == null
                             ? latestInItem
                             : latestInItem > latestInList.UtcStatusChanged
                                   ? latestInItem
                                   : latestInList.UtcStatusChanged;

            return latest;
        }

        public static dynamic GetDefaultValue(this IEnumerable<TypedAttribute> collection, LocalizedString name)
        {
            var attribute = collection.SingleOrDefault(x => x.AttributeDefinition.Alias == name);
            return attribute != null ? attribute.DynamicValue : null;
        }

        /// <summary>
        /// Returns the value of the key value based on the key, if the key is not found, a null value is returned
        /// </summary>
        /// <param name="a"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetValue(this TypedAttribute a, string key)
        {
            return a.Values.GetValue(key);
        }

        /// <summary>
        /// Returns the value of the key value based on the key as it's string value, if the key is not found, then an empty string is returned
        /// </summary>
        /// <param name="a"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValueAsString(this TypedAttribute a, string key)
        {
            return a.Values.GetValueAsString(key);
        }

        /// <summary>
        /// Gets the dynamic value from the attributes of the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributeAlias"></param>
        /// <returns></returns>
        public static string GetAttributeValueAsString(this TypedEntity entity, string attributeAlias)
        {
            var val = entity.GetAttributeValue(attributeAlias);
            return val == null ? String.Empty : val.ToString();
        }

        /// <summary>
        /// Gets the keyed valued from the attributes of the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributeAlias"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAttributeValueAsString(this TypedEntity entity, string attributeAlias, string key)
        {
            var val = entity.GetAttributeValue(attributeAlias, key);
            return val == null ? String.Empty : val.ToString();
        }

        /// <summary>
        /// Returns the dynamic value from an attribute based on it's alias
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributeAlias"></param>
        /// <returns></returns>
        public static dynamic GetAttributeValue(this TypedEntity entity, string attributeAlias)
        {
            var attribute =
                entity.Attributes.FirstOrDefault(
                    x =>
                    {
                        var attributeDefinition = x.AttributeDefinition;
                        return attributeDefinition != null && attributeDefinition.Alias == attributeAlias;
                    });

            return attribute != null ? attribute.DynamicValue : null;
        }

        /// <summary>
        /// Returns the keyed value from an attribute based on it's alias
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributeAlias"></param>
        /// <param name="valueKey"></param>
        /// <returns></returns>
        public static dynamic GetAttributeValue(this TypedEntity entity, string attributeAlias, string valueKey)
        {
            var attribute =
                entity.Attributes.FirstOrDefault(
                    x =>
                    {
                        var attributeDefinition = x.AttributeDefinition;
                        return attributeDefinition != null && attributeDefinition.Alias == attributeAlias;
                    });

            return attribute != null ? attribute.GetValue(valueKey) : null;
        }

        /// <summary>
        /// Copies the specified revision.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="revision">The revision.</param>
        /// <returns></returns>
        public static Revision<T> CopyToNewRevision<T>(this Revision<T> revision)
            where T : class, IVersionableEntity
        {
            return new Revision<T>(revision.Item);
        }

        /// <summary>
        /// Creates a new revision and sets the status on the entity
        /// </summary>
        /// <param name="revision"></param>
        /// <param name="revisionStatusType"></param>
        public static Revision<T> CopyToNewRevision<T>(this Revision<T> revision, RevisionStatusType revisionStatusType)
            where T : class, IVersionableEntity
        {
            var r = new Revision<T>(revision.Item);

            //create the new revision
            var revisionData = new RevisionData { Id = HiveId.Empty, StatusType = revisionStatusType };

            //set the new revision
            r.MetaData = revisionData;

            return r;
        }

        /// <summary>
        /// Sets up the specified entity with default values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="alias">An alias.</param>
        /// <param name="name">A name.</param>
        /// <returns></returns>
        public static T Setup<T>(this T entity, string alias, string name)
            where T : AbstractEntity, IReferenceByName
        {
            entity.Alias = alias;
            entity.Name = name;
            return entity;
        }

        public static T Setup<T>(this T entity, string alias, string name, string description)
            where T : AttributeType, IReferenceByName
        {
            entity.Setup(alias, name);
            entity.Description = description;
            return entity;
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="T"/> having a reference by string alias.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static T Create<T>(string alias, string name) where T : AbstractEntity, IReferenceByName, new()
        {
            var entity = new T();
            return Setup(entity, alias, name);
        }

        /// <summary>
        /// Creates an <see cref="AttributeDefinition"/> with a given <see cref="AttributeType"/> and references it to a given <see cref="AttributeGroup"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <param name="attributeDefinition">The attribute type definition.</param>
        /// <param name="serializationDefinition">The serialization definition.</param>
        /// <param name="attributeGroup">The attribute group definition.</param>
        /// <returns></returns>
        public static T CreateAttributeIn<T>(string alias, string name, AttributeDefinition attributeDefinition, IAttributeSerializationDefinition serializationDefinition,
                                             AttributeGroup attributeGroup)
            where T : AttributeDefinition, new()
        {
            var entity = Create<T>(alias, name);
            entity.AttributeType = attributeDefinition.AttributeType;
            entity.AttributeType.SerializationType = serializationDefinition;
            attributeDefinition.AttributeGroup = attributeGroup;
            return entity;
        }

        /// <summary>
        /// Creates an <see cref="AbstractEntity"/> and places it in the given <paramref name="entityCollection"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityCollection">The entity collection.</param>
        /// <returns></returns>
        public static T CreateIn<T>(string alias, string name, EntityCollection<T> entityCollection)
            where T : AbstractEntity, IReferenceByName, new()
        {
            var entity = Create<T>(alias, name);
            entityCollection.Add(entity);
            return entity;
        }

        /// <summary>
        /// Determines whether the specified attributes contains key.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified attributes contains key; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsKey(this TypedAttributeCollection attributes, string key)
        {
            //dont' worry about casing..
            return attributes.Any(x => x.AttributeDefinition.Alias.InvariantEquals(key));
        }

        /// <summary>
        /// Sets up the entity from schema.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema.</typeparam>
        /// <param name="entity">The entity.</param>
        public static void SetupFromSchema<TSchema>(this TypedEntity entity)
            where TSchema : EntitySchema, new()
        {
            entity.SetupFromSchema(new TSchema());
        }

        /// <summary>
        /// Sets up the entity from schema.
        /// </summary>
        /// <param name="entity"></param>
        public static void SetupFromSchema(this TypedEntity entity)
        {
            if (entity.EntitySchema == null)
                throw new InvalidOperationException(
                    "Cannot setup this TypedEntity from its EntitySchema since the EntitySchema property is null");
            entity.SetupFromSchema(entity.EntitySchema);
        }

        public static void SetupFromSchema(this TypedEntity entity, EntitySchema schema)
        {
            entity.EntitySchema = schema;

            var compositeSchema = schema as CompositeEntitySchema;
            if (compositeSchema != null)
            {
                foreach (var attributeDefinition in compositeSchema.AllAttributeDefinitions)
                {
                    entity.Attributes.TryAdd(new TypedAttribute(attributeDefinition));
                }
            }
            else
                foreach (var attributeDefinition in entity.EntitySchema.AttributeDefinitions)
                {
                    entity.Attributes.TryAdd(new TypedAttribute(attributeDefinition));
                }
        }

        public static void SetupFromEntity(this TypedEntity entity, TypedEntity other)
        {
            entity.SetupFromSchema(other.EntitySchema);
            entity.Id = other.Id;
            entity.UtcCreated = other.UtcCreated;
            entity.UtcModified = other.UtcModified;
            entity.UtcStatusChanged = other.UtcStatusChanged;
            entity.Attributes.Reset(other.Attributes);
            entity.RelationProxies.LazyLoadDelegate = other.RelationProxies.LazyLoadDelegate;
        }

        //public static string GetHashEntityPropertyId(this TypedEntity entity)
        //{

        //}
    }
}
