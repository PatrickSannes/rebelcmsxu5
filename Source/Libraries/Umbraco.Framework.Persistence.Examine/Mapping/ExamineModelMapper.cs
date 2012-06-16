using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    /// <summary>
    /// Mapping class to map Hive entities to Examine entities and vice versa
    /// </summary>
    public sealed class ExamineModelMapper : AbstractFluentMappingEngine
    {

        private readonly ExamineHelper _helper;

        public ExamineModelMapper(ExamineHelper helper, IFrameworkContext frameworkContext)
            : base(frameworkContext)
        {
            _helper = helper;
        }

        public override void ConfigureMappings()
        {

            #region LinearHiveIndexOperation -> IndexOperation

            this.CreateMap<LinearHiveIndexOperation, IndexOperation>(true)
                .CreateUsing(x => new IndexOperation())
                .MapMemberFrom(x => x.Item, x => new IndexItem
                    {
                        Fields = x.Fields,
                        Id = x.Id.Value,
                        ItemCategory = x.ItemCategory
                    })
                .MapMemberFrom(x => x.Operation, x => x.OperationType);

            #endregion


            #region SearchResult -> Relation

            Func<string, int> getOrdinal = (s) =>
                {
                    //need to safe parse the ordinal
                    int ordinal;
                    return int.TryParse(s, out ordinal) ? ordinal : 0;
                };
            this.CreateMap<SearchResult, IRelationById>()
                .CreateUsing(x => new RelationById(
                                      new HiveId(x.Fields[FixedRelationIndexFields.SourceId]),
                                      new HiveId(x.Fields[FixedRelationIndexFields.DestinationId]),
                                      new RelationType(x.Fields[FixedRelationIndexFields.RelationType]),
                                      getOrdinal(x.Fields[FixedIndexedFields.Ordinal])))
                .AfterMap((s, t) =>
                    {

                       

                        //need to setup the metadata
                        foreach(var m in s.Fields.Where(x => x.Key.StartsWith(FixedRelationIndexFields.MetadatumPrefix)))
                        {
                            t.MetaData.Add(new RelationMetaDatum(m.Key.Split('.')[1], m.Value));
                        }
                    });

            #endregion


            #region SearchResult -> Revision<TypedEntity>

            this.CreateMap<SearchResult, Revision<TypedEntity>>(true)
                .CreateUsing(x => new Revision<TypedEntity>())
                .ForMember(x => x.MetaData, x => x.MapUsing(new SearchResultToRevisionData(_helper)))
                .MapMemberFrom(x => x.Item, Map<SearchResult, TypedEntity>)
                .AfterMap((s, t) =>
                {

                });

            #endregion

            #region SearchResult -> TypedEntity

            this.CreateMap(new SearchResultToTypedEntity(_helper, this), true)
                .CreateUsing(x => new TypedEntity())
                .ForMember(x => x.Id, opt => opt.MapFrom(y => HiveId.Parse(y.Fields[FixedIndexedFields.EntityId])))
                .AfterMap((s, t) =>
                    {
                        ExamineHelper.SetEntityDatesFromSearchResult(t, s);
                    });

            #endregion

            #region SearchResult -> EntitySchema

            this.CreateMap<SearchResult, EntitySchema>()
                .CreateUsing(x => new EntitySchema())
                .ForMember(x => x.Id, opt => opt.MapFrom(y => HiveId.Parse(y.Id)))
                .ForMember(x => x.Alias, opt => opt.MapFrom(y => y.Fields.GetValueAsString("Alias")))
                .ForMember(x => x.Name, opt => opt.MapFrom(y => new LocalizedString(y.Fields.GetValueAsString("Name"))))
                .ForMember(x => x.SchemaType, opt => opt.MapFrom(y => y.Fields.GetValueAsString("SchemaType")))
                .MapMemberFrom(x => x.XmlConfiguration,
                               y => y.Fields.GetValueAsString("XmlConfiguration").IsNullOrWhiteSpace()
                                        ? new XDocument()
                                        : XDocument.Parse(y.Fields.GetValueAsString("XmlConfiguration")))
                .AfterMap((s, t) =>
                    {
                        var groups = _helper.GetMappedGroupsForSchema(t.Id);
                        foreach (var g in groups)
                        {
                            t.AttributeGroups.Add(g.Item1);
                        }

                        //find all attribute definitions with this schema id
                        var attDefs = _helper.GetAttributeDefinitionsForSchema(t.Id);
                        //declare a local cache for found attribute types, see notes below.
                        var attributeTypeCache = new List<AttributeType>();

                        foreach (var a in attDefs)
                        {
                            //ok, we've already looked up the groups and its very IMPORTANT that the same group object is applied
                            //to the AttributeDefinition otherwise problems will occur. 
                            //So instead of re-coding the mapping operation for SearchResult -> AttributeDefinition, we'll re-use our 
                            //current Map, but we'll ensure that it does not go re-lookup the AttributeGroup. We can do this by removing
                            //the FixedIndexFieldsGroupId item from the fields so it won't think it has a gruop, then we will add the group back.
                            //NOTE: this procedure can be avoided when the ScopedCache is turned on in the ExamineHelper, however
                            // i don't feel that relying on that mechanism is robust. Once we implement an ExamineDataContext to do 
                            // the lookup caching instead, then this could be removed.
                            var groupId = a.Fields.ContainsKey(FixedIndexedFields.GroupId) ? a.Fields[FixedIndexedFields.GroupId] : null;
                            a.Fields.Remove(FixedIndexedFields.GroupId);

                            //similar to the above, it is very IMPORTANT that the same AttributeType object is applied to each
                            //of the AttributeDefinitions if they reference the same alias/id. In order to acheive this we will 
                            //remove the FixedIndexedFields.AttributeTypeId from the fields so the mapping operation thinks that it
                            //doesn't have one assigned and won't go look it up. We will store the AttributeTypeId locally and look
                            //it up manually and create a local cache to re-assign to the AttributeDefinitions.
                            //NOTE: this procedure can be avoided when the ScopedCache is turned on in the ExamineHelper, however
                            // i don't feel that relying on that mechanism is robust. Once we implement an ExamineDataContext to do 
                            // the lookup caching instead, then this could be removed.
                            var attributeTypeId = a.Fields.ContainsKey(FixedIndexedFields.AttributeTypeId) ? a.Fields[FixedIndexedFields.AttributeTypeId] : null;
                            a.Fields.Remove(FixedIndexedFields.AttributeTypeId);

                            //now do the mapping
                            var mappedAttributeDefinition = Map<SearchResult, AttributeDefinition>(a);
                            
                            //now see if we can find the already found group by id
                            if (groupId != null)
                            {
                                var group = t.AttributeGroups.SingleOrDefault(x => x.Id.Value.ToString() == groupId);
                                mappedAttributeDefinition.AttributeGroup = group;
                            }

                            //now see if we can find an attribute type from our cache or from the helper
                            if (attributeTypeId != null)
                            {
                                var attType = attributeTypeCache.SingleOrDefault(x => x.Id.Value.ToString() == attributeTypeId);
                                if (attType == null)
                                {
                                    //its not in our cache so look it up and add to cache
                                    attType = _helper.PerformGet<AttributeType>(true, LuceneIndexer.IndexNodeIdFieldName, new HiveId(attributeTypeId))
                                        .SingleOrDefault();
                                    if (attType != null)
                                    {
                                        attributeTypeCache.Add(attType);
                                    }
                                }                                
                                mappedAttributeDefinition.AttributeType = attType;
                            }

                            //add the attribute definition
                            t.AttributeDefinitions.Add(mappedAttributeDefinition);
                        }

                        ExamineHelper.SetEntityDatesFromSearchResult(t, s);
                    });

            #endregion

            #region SearchResult -> AttributeDefinition

            this.CreateMap<SearchResult, AttributeDefinition>()
                .CreateUsing(x => new AttributeDefinition())
                .ForMember(x => x.Id, opt => opt.MapFrom(y => HiveId.Parse(y.Id)))
                .ForMember(x => x.Alias, opt => opt.MapFrom(y => y.Fields.GetValueAsString("Alias")))
                .ForMember(x => x.Name, opt => opt.MapFrom(y => new LocalizedString(y.Fields.GetValueAsString("Name"))))
                .ForMember(x => x.Description, opt => opt.MapFrom(y => new LocalizedString(y.Fields.GetValueAsString("Description"))))
                .ForMember(x => x.RenderTypeProviderConfigOverride, opt => opt.MapFrom(y => y.Fields.GetValueAsString("RenderTypeProviderConfigOverride")))
                .AfterMap((s, t) =>
                {
                    //need to do Ordinal safely
                    int ordinal;
                    if (int.TryParse(s.Fields.GetValueAsString(FixedIndexedFields.Ordinal), out ordinal))
                    {
                        t.Ordinal = ordinal;
                    }

                    //lookup the attribute def & group from hive for the attribute def
                    if (s.Fields.ContainsKey(FixedIndexedFields.GroupId))
                    {
                        var group = _helper.PerformGet<AttributeGroup>(true, LuceneIndexer.IndexNodeIdFieldName, new HiveId(s.Fields[FixedIndexedFields.GroupId]));
                        t.AttributeGroup = group.SingleOrDefault();                        
                    }
                    if (s.Fields.ContainsKey(FixedIndexedFields.AttributeTypeId))
                    {
                        var attType = _helper.PerformGet<AttributeType>(true, LuceneIndexer.IndexNodeIdFieldName, new HiveId(s.Fields[FixedIndexedFields.AttributeTypeId]));
                        t.AttributeType = attType.SingleOrDefault();                        
                    }

                    ExamineHelper.SetEntityDatesFromSearchResult(t, s);
                });

            #endregion

            #region SearchResult -> AttributeGroup

            this.CreateMap<SearchResult, AttributeGroup>()
                .CreateUsing(x => new AttributeGroup())
                .ForMember(x => x.Id, opt => opt.MapFrom(y => HiveId.Parse(y.Id)))
                .ForMember(x => x.Alias, opt => opt.MapFrom(y => y.Fields.GetValueAsString("Alias")))
                .ForMember(x => x.Name, opt => opt.MapFrom(y => new LocalizedString(y.Fields.GetValueAsString("Name"))))
                .AfterMap((s, t) =>
                {
                    //need to do Ordinal safely
                    int ordinal;
                    if (int.TryParse(s.Fields.GetValueAsString(FixedIndexedFields.Ordinal), out ordinal))
                    {
                        t.Ordinal = ordinal;
                    }

                    ExamineHelper.SetEntityDatesFromSearchResult(t, s);
                });

            #endregion

            #region SearchResult -> AttributeType

            this.CreateMap<SearchResult, AttributeType>()
                .CreateUsing(x => new AttributeType())
                .ForMember(x => x.Id, opt => opt.MapFrom(y => HiveId.Parse(y.Id)))
                .ForMember(x => x.Alias, opt => opt.MapFrom(y => y.Fields.GetValueAsString("Alias")))
                .ForMember(x => x.Name, opt => opt.MapFrom(y => new LocalizedString(y.Fields.GetValueAsString("Name"))))
                .ForMember(x => x.Description, opt => opt.MapFrom(y => new LocalizedString(y.Fields.GetValueAsString("Description"))))
                .ForMember(x => x.RenderTypeProvider, opt => opt.MapFrom(y => y.Fields.GetValueAsString("RenderTypeProvider")))
                .ForMember(x => x.RenderTypeProviderConfig, opt => opt.MapFrom(y => y.Fields.GetValueAsString("RenderTypeProviderConfig")))
                .AfterMap((s, t) =>
                {
                    //need to do Ordinal safely
                    int ordinal;
                    if (int.TryParse(s.Fields.GetValueAsString(FixedIndexedFields.Ordinal), out ordinal))
                    {
                        t.Ordinal = ordinal;
                    }

                    //create the serialization type based on the FQN stored in the index
                    var serializationType = Type.GetType(s.Fields[FixedIndexedFields.SerializationType]);
                    if (serializationType == null)
                    {
                        //this shouldn't happen but in case something has changed in the index, then we'll default to string
                        t.SerializationType = new StringSerializationType();
                    }
                    else
                    {
                        t.SerializationType = (IAttributeSerializationDefinition)Activator.CreateInstance(serializationType);
                    }

                    ExamineHelper.SetEntityDatesFromSearchResult(t, s);
                });

            #endregion

            #region EntitySchema -> NestedHiveIndexOperation

            //create a map that supports inheritance as we don't want to create a map for all EntitySchemas
            this.CreateMap<EntitySchema, NestedHiveIndexOperation>(true)
                .CreateUsing(x => new NestedHiveIndexOperation())
                .ForMember(x => x.Entity, opt => opt.MapFrom(y => y))
                .ForMember(x => x.OperationType, opt => opt.MapFrom(y => IndexOperationType.Add))
                .ForMember(x => x.Id, opt => opt.MapFrom(y => new Lazy<string>(() => y.Id.Value.ToString()))) //need to lazy load as it might not be set
                .ForMember(x => x.ItemCategory, opt => opt.MapFrom(y => typeof(EntitySchema).Name))
                .ForMember(x => x.Fields, opt => opt.MapUsing<EntitySchemaToIndexFields>())
                .AfterMap((s, t) =>
                    {
                        //Create sub operations for each of its children (both attribute definitions and groups)
                        foreach (var op in s.AttributeDefinitions.Select(Map<AttributeDefinition, NestedHiveIndexOperation>)
                            .Concat(s.AttributeGroups.Select(Map<AttributeGroup, NestedHiveIndexOperation>)))
                        {
                            //NOTE: we need to add this schema id to the fields otherwise we would just add this in the mapping operation for AttributeDefinition if it exposed the schema it belonged to
                            op.Fields.Add(FixedIndexedFields.SchemaId, new Lazy<ItemField>(() => new ItemField(s.Id.Value.ToString()))); //need to add it as lazy since the id might not exist yet
                            t.SubIndexOperations.Add(op);
                        }

                        //get the relations
                        s.MapRelations(t, this);
                    });
            #endregion

            #region TypedEntity -> NestedHiveIndexOperation

            this.CreateMap(new TypedEntityToIndexOperation(this), true);
                
            #endregion

            #region Relation -> NestedHiveIndexOperation

            this.CreateMap<IReadonlyRelation<IRelatableEntity, IRelatableEntity>, NestedHiveIndexOperation>(true)
                .CreateUsing(x => new NestedHiveIndexOperation())
                .ForMember(x => x.Entity, opt => opt.MapFrom(y => y))
                .ForMember(x => x.OperationType, opt => opt.MapFrom(y => IndexOperationType.Add))
                //need to lazy load as ids might not be set yet, this is also a 'composite' id of Source,Dest,Type
                .ForMember(x => x.Id, opt => opt.MapFrom(y => new Lazy<string>(y.GetCompositeId)))
                .ForMember(x => x.ItemCategory, opt => opt.MapFrom(y => "Relation"))
                .ForMember(x => x.Fields, opt => opt.MapUsing<RelationToIndexFields>());

            #endregion

            #region Revision<TypedEntity> -> NestedHiveIndexOperation

            this.CreateMap<Revision<TypedEntity>, NestedHiveIndexOperation>(true)
                .CreateUsing(x => new NestedHiveIndexOperation())
                .AfterMap((s, t) =>
                    {

                        //first, map the underlying TypedEntity
                        var op = Map<TypedEntity, NestedHiveIndexOperation>(s.Item);
                        
                        //map all of the properties... we don't have to explicitly declare a map for this, the engine will automatically just create one
                        Map(op, t);

                        //ensure the rest of the revision data
                        _helper.EnsureRevisionDataForIndexOperation(s, t);

                    });

            #endregion

            #region AttributeType -> NestedHiveIndexOperation

            //create a map that supports inheritance as we don't want to create a map for all EntitySchemas... this would also be impossible
            this.CreateMap<AttributeType, NestedHiveIndexOperation>(true)
                .CreateUsing(x => new NestedHiveIndexOperation())
                .ForMember(x => x.Entity, opt => opt.MapFrom(y => y))
                .ForMember(x => x.OperationType, opt => opt.MapFrom(y => IndexOperationType.Add))
                .ForMember(x => x.Id, opt => opt.MapFrom(y => new Lazy<string>(() => y.Id.Value.ToString()))) //need to lazy load as it might not be set
                .ForMember(x => x.ItemCategory, opt => opt.MapFrom(y => typeof(AttributeType).Name))
                .ForMember(x => x.Fields, opt => opt.MapUsing(new AttributeTypeToIndexFields(_helper)));

            #endregion

            #region AttributeGroup -> NestedHiveIndexOperation

            //create a map that supports inheritance as we don't want to create a map for all EntitySchemas... this would also be impossible
            this.CreateMap<AttributeGroup, NestedHiveIndexOperation>(true)
                .CreateUsing(x => new NestedHiveIndexOperation())
                .ForMember(x => x.Entity, opt => opt.MapFrom(y => y))
                .ForMember(x => x.OperationType, opt => opt.MapFrom(y => IndexOperationType.Add))
                .ForMember(x => x.Id, opt => opt.MapFrom(y => new Lazy<string>(() => y.Id.Value.ToString()))) //need to lazy load as it might not be set
                .ForMember(x => x.ItemCategory, opt => opt.MapFrom(y => typeof(AttributeGroup).Name))
                .ForMember(x => x.Fields, opt => opt.MapUsing<AttributeGroupToIndexFields>());

            #endregion

            #region AttributeDefinition -> NestedHiveIndexOperation

            //create a map that supports inheritance as we don't want to create a map for all EntitySchemas... this would also be impossible
            this.CreateMap<AttributeDefinition, NestedHiveIndexOperation>(true)
                .CreateUsing(x => new NestedHiveIndexOperation())
                .ForMember(x => x.Entity, opt => opt.MapFrom(y => y))
                .ForMember(x => x.OperationType, opt => opt.MapFrom(y => IndexOperationType.Add))
                .ForMember(x => x.Id, opt => opt.MapFrom(y => new Lazy<string>(() => y.Id.Value.ToString()))) //need to lazy load as it might not be set
                .ForMember(x => x.ItemCategory, opt => opt.MapFrom(y => typeof(AttributeDefinition).Name))
                .ForMember(x => x.Fields, opt => opt.MapUsing<AttributeDefinitionToIndexFields>())
                .AfterMap((s, t) =>
                    {
                        //Add sub operation for it's AttributeType
                        t.SubIndexOperations.Add(Map<AttributeType, NestedHiveIndexOperation>(s.AttributeType));
                    });

            #endregion

        }

    }
}
