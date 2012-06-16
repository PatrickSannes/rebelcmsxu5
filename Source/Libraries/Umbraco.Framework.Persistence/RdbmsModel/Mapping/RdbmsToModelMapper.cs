using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.TypeMapping;
using Rdbms = Umbraco.Framework.Persistence.RdbmsModel;
using StackModel = Umbraco.Framework.Persistence.Model;

namespace Umbraco.Framework.Persistence.RdbmsModel.Mapping
{
    public class RdbmsToModelMapper
    {
        private readonly ProviderMetadata _hiveProvider;

        public RdbmsToModelMapper(ProviderMetadata hiveProvider)
        {
            _hiveProvider = hiveProvider;
        }

        public RevisionStatusType MapEntityStatusType(NodeVersionStatusType source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            return new RevisionStatusType((HiveId)source.Id, source.Alias, source.Name, source.IsSystem);
        }

        public void MapEntityStatusType(NodeVersionStatusType source, RevisionStatusType destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Id = (HiveId)source.Id;
            destination.Alias = source.Alias;
            destination.Name = source.Name;
            destination.IsSystem = source.IsSystem;
        }

        public void MapRevision(NodeVersion source, Revision<TypedEntity> destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            if (destination.Item == null)
                destination.Item = MapTypedEntityForRevision(source, lookupHelper, masterMapper);
            else
                MapTypedEntityForRevision(source, destination.Item, lookupHelper, masterMapper);

            var latestStatus = GetLatestNodeVersionStatus(source, lookupHelper, masterMapper);

            var utcStatusChanged = latestStatus == null ? DateTimeOffset.MinValue : latestStatus.Date;
            var statusType = latestStatus == null
                                 ? new RevisionStatusType("unknown", "Unknown / Not specified")
                                 : MapEntityStatusType(latestStatus.NodeVersionStatusType, lookupHelper, masterMapper);

            var metaData = new RevisionData()
                               {
                                   Id = (HiveId)source.Id,
                                   StatusType = statusType,
                                   UtcCreated = source.NodeVersionStatuses.First().Date,
                                   UtcModified = utcStatusChanged,
                                   UtcStatusChanged = utcStatusChanged
                               };

            destination.Item.UtcModified = source.DateCreated;
            destination.Item.UtcStatusChanged = utcStatusChanged;
            destination.MetaData = metaData;
        }

        public Revision<TypedEntity> MapRevision(NodeVersion source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var output = new Revision<TypedEntity>();
            MapRevision(source, output, lookupHelper, masterMapper);
            return output;
        }

        public void MapLocaleToLanguage(Locale source, LanguageInfo destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Alias = source.Alias;
            destination.Name = source.Name;
            destination.Key = source.LanguageIso; // TODO: Refactor LanguageInfo to just use Alias
            destination.InferCultureFromKey();
            // TODO: Lookup Fallbacks using a delegate to the Localization system
        }

        public LanguageInfo MapLocaleToLanguage(Locale source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var output = new LanguageInfo();
            MapLocaleToLanguage(source, output, lookupHelper, masterMapper);
            return output;
        }

        public void MapAttributeTypeDefinition(AttributeType source, Model.Attribution.MetaData.AttributeType destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Alias = source.Alias;
            destination.Id = (HiveId)source.Id;
            destination.Description = source.Description;
            destination.Name = source.Name;
            destination.RenderTypeProvider = source.RenderTypeProvider;
            destination.RenderTypeProviderConfig = source.XmlConfiguration;

            if (!string.IsNullOrEmpty(source.PersistenceTypeProvider))
            {
                // TODO: Use TypeFinder, but requires TypeFinder to be moved into Framework. Otherwise, add caching
                var reverseSerializationType = Type.GetType(source.PersistenceTypeProvider, false, false);
                if (reverseSerializationType != null)
                {
                    var obj = Activator.CreateInstance(reverseSerializationType) as IAttributeSerializationDefinition;
                    destination.SerializationType = obj;
                }
                else
                {
                    throw new TypeLoadException(string.Format("Cannot load type '{0}' which is specified for this AttributeType in the database; either the Assembly has not been loaded into the AppDomain or it's been renamed since this item was last saved.", source.PersistenceTypeProvider));
                }
            }
        }

        /// <summary>
        /// Used to map AttributeTypes when mapping to the EntitySchema
        /// </summary>
        /// <param name="attributeType"></param>
        /// <param name="lookupHelper"></param>
        /// <param name="masterMapper"></param>
        /// <param name="attributeTypeCache">A local cache to resolve already resolved AttributeTypes</param>
        /// <returns></returns>
        private Model.Attribution.MetaData.AttributeType MapAttributeTypeDefinition(
            AttributeType attributeType,
            AbstractLookupHelper lookupHelper,
            AbstractMappingEngine masterMapper,
            ICollection<Model.Attribution.MetaData.AttributeType> attributeTypeCache = null)
        {
            //check if its already been resolved by alias
            if (attributeTypeCache != null)
            {
                var found = attributeTypeCache.SingleOrDefault(x => (Guid)x.Id.Value == attributeType.Id);
                if (found != null)
                    return found;
            }

            //TODO:
            //- Remove Ordinal, SerializationType, Status, Dates
            //- Add PreValue xml
            //- 
            var mapped = new Model.Attribution.MetaData.AttributeType();
            MapAttributeTypeDefinition(attributeType, mapped, lookupHelper, masterMapper);

            //add the mapped object to the cache if it exists
            if (attributeTypeCache != null)
            {
                attributeTypeCache.Add(mapped);
            }

            return mapped;
        }

        public Model.Attribution.MetaData.AttributeType MapAttributeTypeDefinition(
            AttributeType attributeType, 
            AbstractLookupHelper lookupHelper, 
            AbstractMappingEngine masterMapper)
        {
            return MapAttributeTypeDefinition(attributeType, lookupHelper, masterMapper, null);
        }

        /// <summary>
        /// Used to map attribute definitions when mapping the EntitySchema
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="lookupHelper"></param>
        /// <param name="masterMapper"></param>
        /// <param name="attributeTypeCache">A local cache object to resolve already resolved AttributeTypes</param>
        private void MapAttributeDefinition(
            AttributeDefinition source,
            Model.Attribution.MetaData.AttributeDefinition destination,
            AbstractLookupHelper lookupHelper,
            AbstractMappingEngine masterMapper,
            ICollection<Model.Attribution.MetaData.AttributeType> attributeTypeCache = null)
        {
            destination.Ordinal = source.Ordinal;
            destination.Alias = source.Alias;
            destination.AttributeType = MapAttributeTypeDefinition(source.AttributeType, lookupHelper, masterMapper, attributeTypeCache);
            destination.Id = (HiveId)source.Id;
            destination.Name = source.Name;
            destination.Description = source.Description;
            destination.RenderTypeProviderConfigOverride = source.XmlConfiguration;
            destination.AttributeGroup = MapAttributeGroupDefinition(source.AttributeDefinitionGroup, lookupHelper, masterMapper);
        }

        public void MapAttributeDefinition(
            AttributeDefinition source, 
            Model.Attribution.MetaData.AttributeDefinition destination, 
            AbstractLookupHelper lookupHelper, 
            AbstractMappingEngine masterMapper)
        {
            MapAttributeDefinition(source, destination, lookupHelper, masterMapper, null);
        }

        /// <summary>
        /// Used to map attribute definitions when mapping the EntitySchema
        /// </summary>
        /// <param name="attributeDefinition"></param>
        /// <param name="lookupHelper"></param>
        /// <param name="masterMapper"></param>
        /// <param name="attributeTypeCache">A local cache object to resolve already resolved AttributeTypes</param>
        /// <returns></returns>
        private Model.Attribution.MetaData.AttributeDefinition MapAttributeDefinition(
            AttributeDefinition attributeDefinition, 
            AbstractLookupHelper lookupHelper, 
            AbstractMappingEngine masterMapper,
            ICollection<Model.Attribution.MetaData.AttributeType> attributeTypeCache)
        {
            //TODO:
            //- Add ConcurrencyToken to Rdbms model
            //- Remove Status, Dates
            var mapped = new Model.Attribution.MetaData.AttributeDefinition();
            MapAttributeDefinition(attributeDefinition, mapped, lookupHelper, masterMapper, attributeTypeCache);
            return mapped;
        }

        public Model.Attribution.MetaData.AttributeDefinition MapAttributeDefinition(
            AttributeDefinition attributeDefinition,
            AbstractLookupHelper lookupHelper,
            AbstractMappingEngine masterMapper)
        {
            return MapAttributeDefinition(attributeDefinition, lookupHelper, masterMapper, null);
        }

        public void MapAttribute(Attribute source, TypedAttribute destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.AttributeDefinition = MapAttributeDefinition(source.AttributeDefinition, lookupHelper, masterMapper);
            destination.Id = (HiveId)source.Id;

            destination.Values.Clear();

            foreach (var value in source.AttributeStringValues)
            {
                destination.Values.Add(value.ValueKey, value.Value);
            }

            foreach (var value in source.AttributeIntegerValues)
            {
                destination.Values.Add(value.ValueKey, value.Value);
            }

            foreach (var value in source.AttributeDecimalValues)
            {
                destination.Values.Add(value.ValueKey, value.Value);
            }

            foreach (var value in source.AttributeDateValues)
            {
                destination.Values.Add(value.ValueKey, value.Value);
            }

            foreach (var value in source.AttributeLongStringValues)
            {
                destination.Values.Add(value.ValueKey, value.Value);
            }

            //// TODO: Add LanguageInfo to the Values in the persistence (not rdbms) model
            //destination.Values.LazyLoadFactory = () =>
            //{
            //    // Using ConcurrentDictionary as it has a handy AddOrUpdate method meaning a quick
            //    // way of using the last-inserted version, should we hit any ValueKey clashes

            //    var items = new ConcurrentDictionary<string, Lazy<object>>();
            //    foreach (var attributeStringValue in source.AttributeStringValues)
            //    {
            //        var attribValue = attributeStringValue;
            //        var lazyProxyCall = new Lazy<object>(() => attribValue.Value);
            //        items.AddOrUpdate(attribValue.ValueKey, lazyProxyCall, (x,y) => lazyProxyCall);
            //    }

            //    foreach (var attributeIntegerValue in source.AttributeIntegerValues)
            //    {
            //        var attribValue = attributeIntegerValue;
            //        var lazyProxyCall = new Lazy<object>(() => attribValue.Value);
            //        items.AddOrUpdate(attribValue.ValueKey, lazyProxyCall, (x, y) => lazyProxyCall);
            //    }

            //    foreach (var value in source.AttributeDecimalValues)
            //    {
            //        var attribValue = value;
            //        var lazyProxyCall = new Lazy<object>(() => attribValue.Value);
            //        items.AddOrUpdate(attribValue.ValueKey, lazyProxyCall, (x, y) => lazyProxyCall);
            //    }

            //    foreach (var attributeDateValue in source.AttributeDateValues)
            //    {
            //        var attribValue = attributeDateValue;
            //        var lazyProxyCall = new Lazy<object>(() => attribValue.Value);
            //        items.AddOrUpdate(attribValue.ValueKey, lazyProxyCall, (x, y) => lazyProxyCall);
            //    }

            //    foreach (var attributeLongStringValue in source.AttributeLongStringValues)
            //    {
            //        var attribValue = attributeLongStringValue;
            //        var lazyProxyCall = new Lazy<object>(() => attribValue.Value);
            //        items.AddOrUpdate(attribValue.ValueKey, lazyProxyCall, (x, y) => lazyProxyCall);
            //    }

            //    return items;
            //};
        }

        public TypedAttribute MapAttribute(Attribute attribute, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            //TODO:
            //- Remove Dates
            //- Add Ordinal to Rdbms for sorting within a tab
            var mapped = new TypedAttribute();
            MapAttribute(attribute, mapped, lookupHelper, masterMapper);
            return mapped;
        }

        public IEnumerable<TypedAttribute> MapAttributes(IEnumerable<Attribute> attributes, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            return attributes.Select(x => MapAttribute(x, lookupHelper, masterMapper)).ToArray();
        }

        public void MapAttributeGroupDefinition(AttributeDefinitionGroup source, AttributeGroup destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Alias = source.Alias;
            destination.Id = (HiveId)source.Id;
            destination.Name = source.Name;
            destination.UtcCreated = source.DateCreated;
            destination.Ordinal = source.Ordinal;

            //destination.AttributeDefinitions.Clear();

            //foreach (var attributeDefinition in source.AttributeDefinitions)
            //{
            //    destination.AttributeDefinitions.Add(MapAttributeDefinition(attributeDefinition));
            //}
        }

        public AttributeGroup MapAttributeGroupDefinition(AttributeDefinitionGroup attributeDefinitionGroup, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            // TODO:
            // - Attributes and AttributeDefinitions navigator on the stack model will be tricky to track changes
            // - Add Ordinal to Rdbms model
            var mapped = new AttributeGroup();
            MapAttributeGroupDefinition(attributeDefinitionGroup, mapped, lookupHelper, masterMapper);
            return mapped;
        }

        //public void MapAttributeGroup(AttributeGroup destination, AttributeDefinitionGroup source)
        //{
        //    destination.Alias = source.Alias;
        //    destination.Id = source.Id;
        //    destination.Name = source.Name;
        //    destination.UtcCreated = source.DateCreated;
        //}

        //public AttributeGroup MapAttributeGroup(AttributeDefinitionGroup attributeGroup)
        //{
        //    // TODO:
        //    // - Attributes and AttributeDefinitions navigator on the stack model will be tricky to track changes
        //    // - Add Ordinal to Rdbms model
        //    var mapped = new AttributeGroup();
        //    MapAttributeGroup(mapped, attributeGroup);
        //    return mapped;
        //}

        public void MapAttributeSchemaDefinition(AttributeSchemaDefinition source, EntitySchema destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            destination.Alias = source.Alias;
            destination.Id = (HiveId)source.Id;
            destination.Name = source.Name;
            destination.UtcCreated = source.DateCreated;
            destination.SchemaType = source.SchemaType;

            if (!string.IsNullOrEmpty(source.XmlConfiguration))
                destination.XmlConfiguration = XDocument.Parse(source.XmlConfiguration); // else the constructor will initialize it

            destination.AttributeGroups.Clear();
            destination.AttributeDefinitions.Clear();
            //destination.AttributeTypes.Clear();

            foreach (var attributeDefinitionGroup in source.AttributeDefinitionGroups)
            {
                var mappedGroup = MapAttributeGroupDefinition(attributeDefinitionGroup, lookupHelper, masterMapper);
                destination.AttributeGroups.Add(mappedGroup);
            }

            //create a local cache of AttributeTypes to pass to the mappers so that we can 
            //ensure that the same AttributeType based on the alias is the same used across all 
            //AttributeDefinitions.
            var attributeTypeCache = new List<Model.Attribution.MetaData.AttributeType>();

            foreach (var attributeDefinition in source.AttributeDefinitions)
            {
                var localAttribCopy = attributeDefinition;
                var newDef = MapAttributeDefinition(localAttribCopy, lookupHelper, masterMapper, attributeTypeCache);
                newDef.AttributeGroup = destination.AttributeGroups.SingleOrDefault(x => x.Alias == localAttribCopy.AttributeDefinitionGroup.Alias);
                if (newDef.AttributeGroup == null)
                    throw new InvalidOperationException(
                        "Could not find group with alias {0} for mapping".InvariantFormat(
                            localAttribCopy.AttributeDefinitionGroup.Alias));
                destination.AttributeDefinitions.Add(newDef);
            }

            MapRelations(source, destination, lookupHelper, masterMapper);
        }

        public EntitySchema MapAttributeSchemaDefinition(AttributeSchemaDefinition attributeSchemaDefinition, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            //TODO
            //- Map AttributeGroupDefinitions - how to track changes?
            //- Add DateModified to Rdbms model
            var mapped = new EntitySchema();
            MapAttributeSchemaDefinition(attributeSchemaDefinition, mapped, lookupHelper, masterMapper);
            return mapped;
        }

        //public IEnumerable<AttributeGroup> MapAttributeGroups(IEnumerable<AttributeDefinitionGroup> attributeDefinitionGroups)
        //{
        //    // For every item in the input parameter, select the result of the method MapAttributeGroup
        //    return attributeDefinitionGroups.Select(MapAttributeGroup);
        //}

        public void MapTypedEntityForRevision(NodeVersion source, TypedEntity destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            Mandate.ParameterNotNull(source, "source");
            Mandate.ParameterNotNull(destination, "destination");

            Node enclosingNode = source.Node;

            destination.EntitySchema = MapAttributeSchemaDefinition(source.AttributeSchemaDefinition, lookupHelper, masterMapper);

            // When we map the attributes, if any of them belong to schemas with another alias than the schema of this NodeVersion, then
            // those attributes must be "inherited" and we need to advertise that in the output model along with a reference to that schema too
            var allInheritedAttributes = source.Attributes.Where(x => x.AttributeDefinition.AttributeSchemaDefinition.Alias != source.AttributeSchemaDefinition.Alias).ToArray();
            var allRealAttributes = source.Attributes.Except(allInheritedAttributes);

            var mappedRealAttributes = MapAttributes(allRealAttributes, lookupHelper, masterMapper);
            var mappedInheritedAttributes =
                allInheritedAttributes.Select(
                    x => new { Original = x, Mapped = MapAttribute(x, lookupHelper, masterMapper) }).ToArray();

            // For the inherited ones, remap the AttributeDefinition
            foreach (var inherited in mappedInheritedAttributes)
            {
                var otherSchema = MapAttributeSchemaDefinition(inherited.Original.AttributeDefinition.AttributeSchemaDefinition, lookupHelper, masterMapper);
                inherited.Mapped.AttributeDefinition = new InheritedAttributeDefinition(inherited.Mapped.AttributeDefinition, otherSchema);
            }

            var collectionShouldValidateAgainstSchema = !allInheritedAttributes.Any();
            destination.Attributes.Reset(mappedRealAttributes.Concat(mappedInheritedAttributes.Select(x => x.Mapped)), collectionShouldValidateAgainstSchema);

            // Since this manual mapping stuff doesn't have a scope for avoiding remaps of the same object instance, 
            // we need to go through and ensure we're using one group instance, so we'll use the one from the Schema
            foreach (var attribute in destination.Attributes)
            {
                var groupFromSchema = destination.EntitySchema.AttributeGroups.FirstOrDefault(x => x.Id == attribute.AttributeDefinition.AttributeGroup.Id);
                if (groupFromSchema != null) attribute.AttributeDefinition.AttributeGroup = groupFromSchema;
            }

            destination.Id = new HiveId(enclosingNode.Id);
            
            destination.UtcCreated = enclosingNode.DateCreated;
            destination.UtcModified = source.DateCreated;
            destination.UtcStatusChanged = GetLatestStatusDate(source, lookupHelper, masterMapper);

            MapRelations(source.Node, destination, lookupHelper, masterMapper);
        }

        public TypedEntity MapTypedEntityForRevision(NodeVersion node, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var mapped = new TypedEntity();
            MapTypedEntityForRevision(node, mapped, lookupHelper, masterMapper);
            return mapped;
        }

        private void MapRelations<T>(T source, IRelatableEntity destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
            where T: Node
        {
            /* Temp: do nothing */
            return;
        }

        private void OldMapRelations<T>(T source, IRelatableEntity destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
            where T: Node
        {
            //destination.Relations.Clear();
            //destination.Relations.LazyLookAhead = (axis) =>
            //{
            //    switch (axis)
            //    {
            //        case HierarchyScope.Children:
            //        case HierarchyScope.Descendents:
            //        case HierarchyScope.DescendentsOrSelf:
            //            return CheckCountOutgoingRelations(source, destination, lookupHelper, masterMapper);
            //        case HierarchyScope.AllOrNone:
            //            return
            //                CheckCountIncomingRelations(source, destination, lookupHelper, masterMapper) +
            //                CheckCountOutgoingRelations(source, destination, lookupHelper, masterMapper);
            //        case HierarchyScope.Ancestors:
            //        case HierarchyScope.AncestorsOrSelf:
            //            return CheckCountIncomingRelations(source, destination, lookupHelper, masterMapper);
            //    }
            //    return 0;
            //};

            //destination.Relations.LazyLoadFactory = (relatableEntitySource, axis) =>
            //{
            //    switch (axis)
            //    {
            //        case HierarchyScope.Children:
            //        case HierarchyScope.Descendents:
            //        case HierarchyScope.DescendentsOrSelf:
            //            return PopulateOutgoingRelations(source, destination, relatableEntitySource, lookupHelper, masterMapper);
            //        case HierarchyScope.AllOrNone:
            //            return PopulateIncomingRelations(source, destination, relatableEntitySource, lookupHelper, masterMapper)
            //                .Concat(PopulateOutgoingRelations(source, destination, relatableEntitySource, lookupHelper, masterMapper));                    
            //        case HierarchyScope.Ancestors:
            //        case HierarchyScope.AncestorsOrSelf:
            //            return PopulateIncomingRelations(source, destination, relatableEntitySource, lookupHelper, masterMapper);
            //    }
            //    return Enumerable.Empty<Relation>();
            //};
        }

        private NodeVersionStatusHistory GetLatestNodeVersionStatus(NodeVersion source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            return source.NodeVersionStatuses.OrderByDescending(x => x.Date).FirstOrDefault();
        }

        private DateTimeOffset GetLatestStatusDate(NodeVersion source, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            return source.NodeVersionStatuses.OrderByDescending(x => x.Date).Select(x => x.Date).FirstOrDefault();
        }

        private int CheckCountOutgoingRelations(Node source, IRelatableEntity destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var output = 0;
            foreach (var relation in source.OutgoingRelations)
            {
                if (relation.StartNode.Id == relation.EndNode.Id)
                    throw new InvalidOperationException("Cannot relate nodes to themselves");

                output++;
            }
            return output;
        }

        private int CheckCountIncomingRelations(Node source, IRelatableEntity destination, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            var output = 0;
            foreach (var relation in source.IncomingRelations)
            {
                if (relation.StartNode.Id == relation.EndNode.Id)
                    throw new InvalidOperationException("Cannot relate nodes to themselves");

                output++;
            }
            return output;
        }

        private IEnumerable<Relation> PopulateOutgoingRelations(
            Node source, 
            IRelatableEntity destination, 
            IRelatableEntity relatableEntitySource, 
            AbstractLookupHelper lookupHelper, 
            AbstractMappingEngine masterMapper)
        {
            foreach (var relation in source.OutgoingRelations)
            {
                if (relation.StartNode.Id == relation.EndNode.Id)
                    throw new InvalidOperationException("Cannot relate nodes to themselves");

                var endRelatedEntity = relation.EndNode.Id == (Guid)destination.Id.Value
                                           ? destination
                                           : GetEntityFromNodeRelation(relation.EndNode, lookupHelper, masterMapper);

                yield return new Relation(new RelationType(relation.NodeRelationType.Alias),
                    relatableEntitySource,
                    endRelatedEntity,
                    relation.Ordinal,
                    relation.NodeRelationTags.Select(nodeRelationTag => new RelationMetaDatum(nodeRelationTag.Name, nodeRelationTag.Value)).ToArray());
            }
        }

        private IEnumerable<Relation> PopulateIncomingRelations(
            Node source, 
            IRelatableEntity destination, 
            IRelatableEntity relatableEntitySource, 
            AbstractLookupHelper lookupHelper, 
            AbstractMappingEngine masterMapper)
        {
            foreach (var relation in source.IncomingRelations)
            {
                if (relation.StartNode.Id == relation.EndNode.Id)
                    throw new InvalidOperationException("Cannot relate nodes to themselves");

                var startRelatedEntity = relation.StartNode.Id == (Guid)destination.Id.Value
                                           ? destination
                                           : GetEntityFromNodeRelation(relation.StartNode, lookupHelper, masterMapper);                

                yield return new Relation(new RelationType(relation.NodeRelationType.Alias),
                    startRelatedEntity,
                    relatableEntitySource,
                    relation.Ordinal,
                    relation.NodeRelationTags.Select(nodeRelationTag => new RelationMetaDatum(nodeRelationTag.Name, nodeRelationTag.Value)).ToArray());
            }
        }

        /// <summary>
        /// Returns an IRelatableEntity based on a Node, used in the Populate relations methods
        /// </summary>
        /// <param name="relationNode"></param>
        /// <param name="lookupHelper"></param>
        /// <param name="masterMapper"></param>
        /// <returns></returns>
        private IRelatableEntity GetEntityFromNodeRelation(Node relationNode, AbstractLookupHelper lookupHelper, AbstractMappingEngine masterMapper)
        {
            //NOTE: The type check only works when you have NH's Lazy load configuration for relations set to "no-proxy"!
            
            return (relationNode is AttributeSchemaDefinition)
                ? MapAttributeSchemaDefinition((AttributeSchemaDefinition)relationNode, lookupHelper, masterMapper)
                : (IRelatableEntity)MapTypedEntityForRevision(GetMostRecentVersion(relationNode), lookupHelper, masterMapper);            
        }
        
        public NodeVersion GetMostRecentVersion(Node source, IReferenceByName revisionStatus = null)
        {
            // Get the most recent version from the node with the right revisionStatus
            return source
                 .NodeVersions
                 .Select(x =>
                 {
                     var status =
                         x.NodeVersionStatuses.OrderByDescending(y => y.Date).
                             FirstOrDefault();
                     return new { Version = x, Status = status, Type = status.NodeVersionStatusType };
                 })
                 .Where(x => revisionStatus == null || x.Type.Alias == revisionStatus.Alias) // Don't put a constraint on the revision type if the supplied param is null
                 .OrderByDescending(x => x.Status.Date)
                 .Select(x => x.Version)
                 .FirstOrDefault();
        }
    }
}
