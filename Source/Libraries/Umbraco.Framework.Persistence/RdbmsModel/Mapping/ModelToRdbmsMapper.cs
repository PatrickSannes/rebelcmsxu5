using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.RdbmsModel.Mapping
{
    using Umbraco.Framework.Data;

    public static class ModelToRdbmsMapper
    {

        private static void MapVersion(TypedEntity source, NodeVersion destination, Node masterNode, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.DateCreated = source.UtcModified;
            destination.Node = masterNode;

            // First map the schema which will also map the attribute definitions
            var destinationSchema = MapAttributeSchemaDefinition(source.EntitySchema, lookupHelper, masterMapper);
            destination.AttributeSchemaDefinition = destinationSchema;

            //var rdbmsMappedAttributes = MapAttributes(source.Attributes, lookupHelper, masterMapper).ToList();
            //foreach (var rdbmsMappedAttribute in rdbmsMappedAttributes)
            //{
            //    // Ensure attribute has the AttributeDefinition instance created when mapping
            //    // the schema, to avoid saving duplicate definitions
            //    var attribute = rdbmsMappedAttribute;
            //    rdbmsMappedAttribute.AttributeDefinition = rdbmsSchema.AttributeDefinitions.Single(x => x.Alias == attribute.AttributeDefinition.Alias);
            //}

            MergeMapCollections(
                source.Attributes,
                destination.Attributes,
                (sourceTypedAttribute, dest) => MapAttribute(sourceTypedAttribute, dest, lookupHelper, masterMapper),
                sourceTypedAttribute => MapAttribute(sourceTypedAttribute, lookupHelper, masterMapper),
                (sourceAttrib, destAttrib) =>
                {
                    destAttrib.NodeVersion = destination;
                   
                    // Support inherited properties: if the attribute definition has a schema on it, it's from another schema
                    var inheritedDef = sourceAttrib.AttributeDefinition as InheritedAttributeDefinition;
                    if (inheritedDef != null)
                    {
                        var def = lookupHelper.Lookup<AttributeDefinition>(inheritedDef.Id);
                        if (def != null)
                        {
                            destAttrib.AttributeDefinition = def;
                        }
                    }
                    else
                        destAttrib.AttributeDefinition = destinationSchema.AttributeDefinitions.Single(x => x.Alias == sourceAttrib.AttributeDefinition.Alias);
                });

            //destination.Attributes.EnsureClearedWithProxy();
            //rdbmsMappedAttributes.ForEach(x =>
            //                                  {
            //                                      destination.Attributes.Add(x);
            //                                      x.NodeVersion = destination;
            //                                  });

            var revisionData = new RevisionData(FixedStatusTypes.Created);
            var rdbmsStatusHistory = CreateRdbmsStatusHistory(destination, revisionData, lookupHelper, masterMapper);
            destination.NodeVersionStatuses.Add(rdbmsStatusHistory);
        }

        public static void MapFromRevision(Revision<TypedEntity> source, NodeVersion destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var masterNode = destination;
            if (masterNode == null)
                MapTypedEntityForRevision(source.Item, lookupHelper, masterMapper);
            else
                MapTypedEntityForRevision(source.Item, masterNode, lookupHelper, masterMapper);

            // Overwrite the default status history that was created by MapVersion
            // since in this mapping method, we actually have real revision info
            // The MapVersion method, in lieu of any data about a revision, adds a status history of "Created", which we can remove
            var revisionData = source.MetaData;

            var rdbmsStatusHistory = CreateRdbmsStatusHistory(destination, revisionData, lookupHelper, masterMapper);
            var createdStatusHistory = destination.NodeVersionStatuses.FirstOrDefault(x => x.NodeVersionStatusType.Alias == FixedStatusTypes.Created.Alias);
            if (createdStatusHistory != null) destination.NodeVersionStatuses.Remove(createdStatusHistory);
            destination.NodeVersionStatuses.Add(rdbmsStatusHistory);
        }

        private static NodeVersionStatusHistory CreateRdbmsStatusHistory(NodeVersion destination, RevisionData revisionData, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            return new NodeVersionStatusHistory()
                       {
                           Date = revisionData.UtcStatusChanged,
                           NodeVersion = destination,
                           NodeVersionStatusType = MapStatusType(revisionData.StatusType, lookupHelper, masterMapper)
                       };
        }

        public static NodeVersionStatusType MapStatusType(RevisionStatusType source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var output = GetObjectReference(source.Id, lookupHelper, () => new NodeVersionStatusType());
            MapStatusType(source, output, lookupHelper, masterMapper);
            return output;
        }

        public static void MapStatusType(RevisionStatusType source, NodeVersionStatusType destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Id = (Guid)source.Id.Value;
            destination.Name = source.Name;
            destination.Alias = source.Alias;
            destination.IsSystem = source.IsSystem;
        }

        public static NodeVersion MapFromRevision(Revision<TypedEntity> source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var output = GetObjectReference(source.MetaData.Id, lookupHelper, () => new NodeVersion());
            MapFromRevision(source, output, lookupHelper, masterMapper);
            return output;
        }

        public static void MapLanguageToLocale(LanguageInfo source, Locale destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Id = source.Alias.EncodeAsGuid();
            destination.Alias = source.Key; // TODO: Refactor LanguageInfo to use just Alias
            destination.Name = source.Name;
            destination.LanguageIso = source.Key;
        }

        public static Locale MapLanguageToLocale(LanguageInfo source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var output = new Locale(); // var output = GetObjectReference(source.Alias.EncodeAsGuid(), lookupHelper, () => new Locale());
            MapLanguageToLocale(source, output, lookupHelper, masterMapper);
            return output;
        }

        public static void MapAttributeTypeDefinition(Model.Attribution.MetaData.AttributeType source, AttributeType destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Id = (Guid)source.Id.Value;

            destination.Alias = source.Alias ?? StringExtensions.ToUmbracoAlias(source.Name);

            var attributeSerializationDefinition = source.SerializationType;
            if (attributeSerializationDefinition == null)
            {
                LogHelper.Warn(typeof(ModelToRdbmsMapper), ("source.SerializationType is null for AttributeType Id {0} and Alias {1}"), source.Id, source.Alias);
                return;
            }
            destination.PersistenceTypeProvider = attributeSerializationDefinition.GetType().AssemblyQualifiedName;

            //sets the values if it is dirty
            destination.TrySetPropertyFromDirty(toProp => toProp.Alias, source, fromProp => fromProp.Alias, () => source.Alias ?? StringExtensions.ToUmbracoAlias(source.Name));
            destination.TrySetPropertyFromDirty(toProp => toProp.Description, source, fromProp => fromProp.Description, () => (string)source.Description);
            destination.TrySetPropertyFromDirty(toProp => toProp.Name, source, fromProp => fromProp.Name, () => (string)source.Name);
            destination.TrySetPropertyFromDirty(toProp => toProp.RenderTypeProvider, source, fromProp => fromProp.RenderTypeProvider);
            destination.TrySetPropertyFromDirty(toProp => toProp.XmlConfiguration, source, fromProp => fromProp.RenderTypeProviderConfig);
        }

        public static AttributeType MapAttributeTypeDefinition(Model.Attribution.MetaData.AttributeType attributeType, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            Mandate.ParameterNotNull(attributeType, "AttributeType");

            var mapped = GetObjectReference(attributeType.Id, lookupHelper, () => new AttributeType());
            MapAttributeTypeDefinition(attributeType, mapped, lookupHelper, masterMapper);
            return mapped;
        }

        public static void MapAttributeDefinition(Model.Attribution.MetaData.AttributeDefinition source, AttributeDefinition destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Ordinal = source.Ordinal;
            destination.Alias = source.Alias;
            destination.AttributeType = MapAttributeTypeDefinition(source.AttributeType, lookupHelper, masterMapper);
            destination.Description = source.Description;
            //destination.AttributeSchemaDefinition = MapAttributeSchemaDefinition(source.AttributeType.)
            destination.Id = (Guid)source.Id.Value;
            destination.Name = source.Name;
            destination.XmlConfiguration = source.RenderTypeProviderConfigOverride;
        }

        public static AttributeDefinition MapAttributeDefinition(Model.Attribution.MetaData.AttributeDefinition attributeDefinition, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            Mandate.ParameterNotNull(attributeDefinition, "attributeDefinition");

            var mapped = GetObjectReference(attributeDefinition.Id, lookupHelper, () => new AttributeDefinition());
            MapAttributeDefinition(attributeDefinition, mapped, lookupHelper, masterMapper);
            return mapped;
        }

        public static void MapAttribute(TypedAttribute source, Attribute destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.AttributeDefinition = MapAttributeDefinition(source.AttributeDefinition, lookupHelper, masterMapper);
            destination.Id = (Guid)source.Id.Value;

            var mapLanguageToLocale = MapLanguageToLocale(FixedLocales.Default, lookupHelper, masterMapper);

            Func<string, bool> keyNotFoundPredicate = x => source.Values.All(y => y.Key != x);
            destination.AttributeStringValues.RemoveAll(x => keyNotFoundPredicate.Invoke(x.ValueKey));
            destination.AttributeDecimalValues.RemoveAll(x => keyNotFoundPredicate.Invoke(x.ValueKey));
            destination.AttributeLongStringValues.RemoveAll(x => keyNotFoundPredicate.Invoke(x.ValueKey));
            destination.AttributeIntegerValues.RemoveAll(x => keyNotFoundPredicate.Invoke(x.ValueKey));
            destination.AttributeDateValues.RemoveAll(x => keyNotFoundPredicate.Invoke(x.ValueKey));

            foreach (var value in source.Values)
            {
                switch (source.AttributeDefinition.AttributeType.SerializationType.DataSerializationType)
                {
                    case DataSerializationTypes.LongString:
                        var longString = destination.AttributeLongStringValues.FirstOrDefault(x => x.ValueKey == value.Key) ?? new AttributeLongStringValue()
                                             {
                                                 Locale = mapLanguageToLocale,
                                                 ValueKey = value.Key,
                                                 Attribute = destination
                                             };
                        if (value.Value != null) longString.Value = value.Value.ToString();
                        destination.AttributeLongStringValues.Add(longString);
                        break;                    
                    case DataSerializationTypes.Boolean:
                    case DataSerializationTypes.SmallInt:
                    case DataSerializationTypes.LargeInt:
                        var intVal = destination.AttributeIntegerValues.FirstOrDefault(x => x.ValueKey == value.Key) ?? new AttributeIntegerValue()
                                         {
                                             Locale = mapLanguageToLocale,
                                             ValueKey = value.Key,
                                             Attribute = destination
                                         };
                        if (value.Value != null) 
                            intVal.Value = Convert.ToInt32(value.Value);
                        destination.AttributeIntegerValues.Add(intVal);
                        break;
                    case DataSerializationTypes.Decimal:
                        var decimalValue = destination.AttributeDecimalValues.FirstOrDefault(x => x.ValueKey == value.Key) ?? new AttributeDecimalValue()
                        {
                            Locale = mapLanguageToLocale,
                            ValueKey = value.Key,
                            Attribute = destination
                        };
                        if (value.Value != null) decimalValue.Value = (int)value.Value;
                        destination.AttributeDecimalValues.Add(decimalValue);
                        break;
                    case DataSerializationTypes.Date:
                        var dateVal = destination.AttributeDateValues.FirstOrDefault(x => x.ValueKey == value.Key) ?? new AttributeDateValue()
                                          {
                                              Locale = mapLanguageToLocale,
                                              ValueKey = value.Key,
                                              Attribute = destination
                                          };
                        if (value.Value != null)
                        {
                            if (value.Value is DateTimeOffset)
                                dateVal.Value = (DateTimeOffset)value.Value;
                            else if (value.Value is DateTime)
                                dateVal.Value = new DateTimeOffset((DateTime)value.Value);
                        }
                        destination.AttributeDateValues.Add(dateVal);
                        break;
                    case DataSerializationTypes.String:
                    default:
                        var shortString = destination.AttributeStringValues.FirstOrDefault(x => x.ValueKey == value.Key)
                            ?? new AttributeStringValue()
                        {
                            Locale = mapLanguageToLocale,
                            ValueKey = value.Key,
                            Attribute = destination
                        };
                        if (value.Value != null) shortString.Value = value.Value.ToString();
                        destination.AttributeStringValues.Add(shortString);
                        break;
                }
            }
        }

        public static Attribute MapAttribute(TypedAttribute attribute, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            //TODO:
            //- Remove Dates
            //- Add Ordinal to Rdbms for sorting within a tab
            var mapped = GetObjectReference(attribute.Id, lookupHelper, () => new Attribute());
            MapAttribute(attribute, mapped, lookupHelper, masterMapper);
            return mapped;
        }

        public static IEnumerable<Attribute> MapAttributes(IEnumerable<TypedAttribute> attributes, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            return attributes.Select(x => MapAttribute(x, lookupHelper, masterMapper));
        }

        public static void MapAttributeDefinitionGroup(AttributeGroup source, AttributeDefinitionGroup destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Alias = source.Alias;
            destination.Id = (Guid)source.Id.Value;
            destination.Name = source.Name;
            destination.DateCreated = source.UtcCreated;
            destination.Ordinal = source.Ordinal;
        }

        public static AttributeDefinitionGroup MapAttributeDefinitionGroup(AttributeGroup attributeGroup, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var mapped = GetObjectReference(attributeGroup.Id, lookupHelper, () => new AttributeDefinitionGroup());
            MapAttributeDefinitionGroup(attributeGroup, mapped, lookupHelper, masterMapper);
            return mapped;
        }

        /// <summary>
        /// Gets a reference to an object if it already exists in the set maintained by the lookup helper, otherwise calls a factory to create a new instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The id.</param>
        /// <param name="lookupHelper">The lookup helper.</param>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static T GetObjectReference<T>(HiveId id, AbstractLookupHelper lookupHelper, Func<T> factory) where T : class, IReferenceByGuid
        {
            T output = null;
            if (!id.IsNullValueOrEmpty()) output = lookupHelper.Lookup<T>(id);
            return output ?? factory.Invoke();
        }

        public static IEnumerable<AttributeDefinitionGroup> MapAttributeDefinitionGroups(IEnumerable<AttributeGroup> source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            return source.Select(x => MapAttributeDefinitionGroup(x, lookupHelper, masterMapper));
        }

        private static IEnumerable<TDestination> MergeMapCollections<TSource, TDestination>(
            IEnumerable<TSource> source,
            ICollection<TDestination> destination,
            Action<TSource, TDestination> mapInPlace,
            Func<TSource, TDestination> mapWithOutput,
            Action<TSource, TDestination> afterMap,
            Func<TSource, TDestination, bool> extraComparison = null)
            where TSource : AbstractEntity
            where TDestination : IReferenceByGuid
        {
            if (extraComparison == null)
                extraComparison = (x, y) => false;
            //extraComparison = (sourceItem, destItem) => destItem.Alias == sourceItem.Alias;

            //first, iterate through all source items that have an Id
            foreach (var sourceItem in source
                .Where(x => !x.Id.IsNullValueOrEmpty()))
            {
                var sourceItemCopy = sourceItem;
                //iterate all destination items the item id in the destination matches the source item id or the extra comparison matches
                foreach (var existingItem in destination
                    .Where(existingItem => (HiveId)existingItem.Id == sourceItemCopy.Id.ConvertToEmptyIfNullValue() || extraComparison.Invoke(sourceItemCopy, existingItem)))
                {
                    // Remap onto existing instance
                    mapInPlace.Invoke(sourceItemCopy, existingItem);

                    // Invoke the delegate for after a mapping-in-place has happened
                    afterMap.Invoke(sourceItemCopy, existingItem);
                }
            }
            //next, iterate through all source items where the destination doesnt have any items in which 
            // the destination item id matches the any source item id or the extra comparison matches
            foreach (var sourceItemForAdding in source
                .Where(sourceItem => !destination.Any(existingItem => (HiveId)existingItem.Id == sourceItem.Id.ConvertToEmptyIfNullValue() || extraComparison.Invoke(sourceItem, existingItem))))
            {
                var destinationItemForAdding = mapWithOutput.Invoke(sourceItemForAdding);
                afterMap.Invoke(sourceItemForAdding, destinationItemForAdding);
                destination.Add(destinationItemForAdding);
            }
            Func<TDestination, bool> chooseItemsToRemove = existingItem => !((HiveId)existingItem.Id).IsNullValueOrEmpty() && !source.Any(sourceItem => (Guid)sourceItem.Id.Value == existingItem.Id || extraComparison.Invoke(sourceItem, existingItem));
            TDestination[] itemsToRemove = destination.Where(chooseItemsToRemove).ToArray();
            itemsToRemove.ForEach(item => destination.Remove(item));
            return itemsToRemove;
        }

        public static void MapAttributeSchemaDefinition(EntitySchema source, AttributeSchemaDefinition destinationSchema, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destinationSchema.Alias = source.Alias;
            destinationSchema.Id = (Guid)source.Id.Value;
            destinationSchema.Name = source.Name;
            destinationSchema.DateCreated = source.UtcCreated;
            destinationSchema.XmlConfiguration = source.XmlConfiguration.ToString();
            destinationSchema.SchemaType = source.SchemaType;

            // Find groups in both source and destination that exist, and remap them at the destination
            // Then find groups only at the source, and add them
            // Then find groups only at the destination, and remove them
            var groupsToRemove = MergeMapCollections(
                source.AttributeGroups,
                destinationSchema.AttributeDefinitionGroups,
                (sourceGroup, destGroup) => MapAttributeDefinitionGroup(sourceGroup, destGroup, lookupHelper, masterMapper),
                sourceGroup => MapAttributeDefinitionGroup(sourceGroup, lookupHelper, masterMapper),
                (sourceGroup, destGroup) => destGroup.AttributeSchemaDefinition = destinationSchema,
                (sourceGroup, destGroup) => sourceGroup.Alias == destGroup.Alias
                );

            // Now do the same for the attribute definitions
            var defsToRemove = MergeMapCollections(
                source.AttributeDefinitions,
                destinationSchema.AttributeDefinitions,
                (sourceDef, destDef) => MapAttributeDefinition(sourceDef, destDef, lookupHelper, masterMapper),
                sourceDef => MapAttributeDefinition(sourceDef, lookupHelper, masterMapper),
                (sourceDef, destDef) =>
                {
                    destDef.AttributeSchemaDefinition = destinationSchema;
                    if (sourceDef.AttributeGroup == null) return;

                    var localDefCopy = sourceDef;
                    var mappedGroup = destinationSchema.AttributeDefinitionGroups.Single(x => x.Alias == localDefCopy.AttributeGroup.Alias);
                    destDef.AttributeDefinitionGroup = mappedGroup;
                },
                (sourceGroup, destGroup) => sourceGroup.Alias == destGroup.Alias
                );
           
            // Hack for beta: make sure if there are any remaining AttributeDefinitions referenced via a group, but not in the AttributeDefinitions collection,
            // we delete it from the group
            var defsToDelete = new List<AttributeDefinition>();
            foreach (var attributeDefinitionGroup in destinationSchema.AttributeDefinitionGroups)
            {
                foreach (var attributeDefinition in attributeDefinitionGroup.AttributeDefinitions)
                {
                    if (!destinationSchema.AttributeDefinitions.Contains(attributeDefinition))
                    {
                        defsToDelete.Add(attributeDefinition);
                    }
                }
            }
            foreach (var attributeDefinition in defsToDelete)
            {
                foreach (var attributeDefinitionGroup in destinationSchema.AttributeDefinitionGroups)
                {
                    attributeDefinitionGroup.AttributeDefinitions.Remove(attributeDefinition);
                }
            }

            MapRelations(destinationSchema, source, lookupHelper, masterMapper);
        }

        public static AttributeSchemaDefinition MapAttributeSchemaDefinition(EntitySchema entitySchema, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var rdbmsMapped = GetObjectReference(entitySchema.Id, lookupHelper, () => new AttributeSchemaDefinition());
            MapAttributeSchemaDefinition(entitySchema, rdbmsMapped, lookupHelper, masterMapper);
            return rdbmsMapped;
        }


        public static void MapTypedEntityForRevision(TypedEntity source, NodeVersion destinationVersion, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            // Map the parent node of the node-version
            var enclosingNode = destinationVersion.Node;
            if (enclosingNode == null)
            {
                enclosingNode = new Node();
                destinationVersion.Node = enclosingNode;
                enclosingNode.NodeVersions.Add(destinationVersion);
            }

            enclosingNode.DateCreated = source.UtcCreated;
            enclosingNode.Id = (Guid)source.Id.Value;

            MapVersion(source, destinationVersion, enclosingNode, lookupHelper, masterMapper);
            //destinationNode.NodeVersions.Add(nodeVersion);

            MapRelations(enclosingNode, source, lookupHelper, masterMapper);
        }

        private static void MapRelations(Node enclosingNode, IRelatableEntity source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            /* Temporary: do nothing */
            
            return;
        }

        public static NodeVersion MapTypedEntityForRevision(TypedEntity typedEntity, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var mapped = new NodeVersion();
            MapTypedEntityForRevision(typedEntity, mapped, lookupHelper, masterMapper);
            return mapped;
        }

    }
}