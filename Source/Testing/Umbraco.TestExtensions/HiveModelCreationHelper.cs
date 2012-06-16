using System;
using System.Linq;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.NodeName;
using Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.SelectedTemplate;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive;

namespace Umbraco.Tests.Extensions
{
    using Umbraco.Cms.Web.Model;

    public static class HiveModelCreationHelper
    {
        public static readonly string TypeAlias1 = "type-alias1";
        public static readonly string TypeAlias2 = "type-alias2";
        public static readonly string DefAlias1WithType1 = "def-alias-1-with-type1";
        public static readonly string DefAlias2WithType1 = "def-alias-2-with-type1";

        public static Content SetupTestContentData(Guid newGuid, Guid newGuidRedHerring, ProviderSetup providerSetup)
        {
            var baseEntity = HiveModelCreationHelper.MockTypedEntity();
            var entity = new Content(baseEntity); 
            entity.Id = new HiveId(newGuid);
            entity.EntitySchema.Alias = "schema-alias1";

            var existingDef = entity.EntitySchema.AttributeDefinitions[0];
            var newDef = HiveModelCreationHelper.CreateAttributeDefinition("aliasForQuerying", "", "", existingDef.AttributeType, existingDef.AttributeGroup, true);
            entity.EntitySchema.AttributeDefinitions.Add(newDef);
            entity.Attributes.Add(new TypedAttribute(newDef, "my-new-value"));

            entity.Attributes[1].DynamicValue = "not-on-red-herring";
            entity.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "my-test-route";

            var redHerringEntity = HiveModelCreationHelper.MockTypedEntity();
            redHerringEntity.Id = new HiveId(newGuidRedHerring);
            redHerringEntity.EntitySchema.Alias = "redherring-schema";

            using (var uow = providerSetup.UnitFactory.Create())
            {
                var publishedRevision = new Revision<TypedEntity>(entity)
                    { MetaData = { StatusType = FixedStatusTypes.Published } };

                uow.EntityRepository.Revisions.AddOrUpdate(publishedRevision);
                // Only add extra entity if caller wants it
                if (newGuidRedHerring != Guid.Empty) uow.EntityRepository.AddOrUpdate(redHerringEntity);
                uow.Complete();
            }

            return entity;
        }

        public static TypedEntity SetupTestData(Guid newGuid, Guid newGuidRedHerring, ProviderSetup providerSetup)
        {
            var entity = HiveModelCreationHelper.MockTypedEntity();
            entity.Id = new HiveId(newGuid);
            entity.EntitySchema.Alias = "schema-alias1";

            var existingDef = entity.EntitySchema.AttributeDefinitions[0];
            var newDef = HiveModelCreationHelper.CreateAttributeDefinition("aliasForQuerying", "", "", existingDef.AttributeType, existingDef.AttributeGroup, true);
            entity.EntitySchema.AttributeDefinitions.Add(newDef);
            entity.Attributes.Add(new TypedAttribute(newDef, "my-new-value"));

            entity.Attributes[1].DynamicValue = "not-on-red-herring";
            entity.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "my-test-route";

            var redHerringEntity = HiveModelCreationHelper.MockTypedEntity();
            redHerringEntity.Id = new HiveId(newGuidRedHerring);
            redHerringEntity.EntitySchema.Alias = "redherring-schema";

            using (var uow = providerSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(entity);
                // Only add extra entity if caller wants it
                if (newGuidRedHerring != Guid.Empty) uow.EntityRepository.AddOrUpdate(redHerringEntity);
                uow.Complete();
            }

            return entity;
        }


        public static Revision<TypedEntity> CreateVersionedTypedEntity(EntitySchema schema, TypedAttribute[] attribs)
        {
            var entity = new Revision<TypedEntity>
                {
                    Item = new TypedEntity
                        {
                            EntitySchema = schema
                        }
                };

            entity.Item.Attributes.Reset(attribs);

            entity.MetaData = new RevisionData();

            return entity;
        }

        public static TypedEntity CreateTypedEntity(EntitySchema schema, TypedAttribute[] attribs, bool assignId = false)
        {
            var entity = new TypedEntity
                {
                    EntitySchema = schema
                };
            entity.Attributes.Reset(attribs);
            if (assignId)
                entity.Id = new HiveId(Guid.NewGuid());
            return entity;
        }

        //public static VersionedPersistenceEntity CreateVersionedPersistenceEntity()
        //{
        //    var entity = new VersionedPersistenceEntity();
        //    //entity.Setup();
        //    entity.ConcurrencyToken = new BasicConcurrencyToken();
        //    entity.Revision = new RevisionData();
        //    entity.Revision.Changeset = new Changeset();
        //    entity.Revision.Changeset.Branch = new Branch();
        //    entity.UtcCreated = entity.UtcModified = entity.UtcStatusChanged = DateTime.UtcNow;
        //    entity.Status = new RevisionStatusType();
        //    return entity;
        //}

        public static AttributeType CreateAttributeType(string alias, string name, string description, bool assignId = false)
        {
            var atd = new AttributeType();
            atd.Setup(alias, name, description);
            atd.SerializationType = new StringSerializationType();
            if (assignId)
                atd.Id = new HiveId(Guid.NewGuid());
            return atd;
        }

        public static AttributeDefinition CreateAttributeDefinition(string alias, string name, string description, AttributeType typeDef, AttributeGroup groupDef, bool assignId = false)
        {
            var definition = new AttributeDefinition();
            definition.Setup(alias, name);
            definition.AttributeType = typeDef;
            definition.Description = description;
            definition.AttributeGroup = groupDef;
            if (assignId)
                definition.Id = new HiveId(Guid.NewGuid());
            return definition;
        }

        public static AttributeGroup CreateAttributeGroup(string alias, string name, int ordinal, bool assignId = false)
        {
            var definition = new AttributeGroup();
            definition.Setup(alias, name);
            definition.Ordinal = ordinal;
            if (assignId)
                definition.Id = new HiveId(Guid.NewGuid());
            return definition;
        }

        public static EntitySchema CreateEntitySchema(string alias, string name, params AttributeDefinition[] attributeDefinitions)
        {
            var def = new EntitySchema();
            def.Setup(alias, name);
            def.AttributeDefinitions.AddRange(attributeDefinitions);
            return def;
        }

        public static EntitySchema CreateEntitySchema(string alias, string name, bool assignId, params AttributeDefinition[] attributeDefinitions)
        {
            var def = new EntitySchema();
            def.Setup(alias, name);
            def.AttributeDefinitions.AddRange(attributeDefinitions);
            if (assignId)
                def.Id = new HiveId(Guid.NewGuid());
            return def;
        }

        public static TypedAttribute CreateAttribute(AttributeDefinition def, object value)
        {
            return new TypedAttribute(def, value);
        }

        public static TypedEntity MockTypedEntity(HiveId id)
        {
            TypedEntity created = null;
            CreateMockTypedEntityProperties(false,
                                                       (schema, attr) =>
                                                       {
                                                           created = CreateTypedEntity(schema, attr);
                                                           created.Id = id;
                                                       });
            return created;
        }

        public static TypedEntity MockTypedEntity(bool assignIds)
        {
            TypedEntity created = null;

            CreateMockTypedEntityProperties(assignIds,
                                                       (schema, attr) =>
                                                       {
                                                           created = CreateTypedEntity(schema, attr);
                                                           created.Id = assignIds ? new HiveId(Guid.NewGuid()) : HiveId.Empty;
                                                       });

            return created;
        }

        public static TypedEntity MockTypedEntity()
        {
            return MockTypedEntity(false);
        }

        /// <summary>
        /// Create a typed persistence entity
        /// </summary>
        /// <param name="assignIds">whether or not to generate Ids for each entity</param>
        /// <returns></returns>
        public static Revision<TypedEntity> MockVersionedTypedEntity(bool assignIds)
        {
            Revision<TypedEntity> created = null;

            CreateMockTypedEntityProperties(assignIds,
                                                       (schema, attr) =>
                                                       {
                                                           created = CreateVersionedTypedEntity(schema, attr);
                                                           if (assignIds) created.Item.Id = new HiveId(Guid.NewGuid());

                                                           created.MetaData = new RevisionData(FixedStatusTypes.Draft);
                                                           if (assignIds) created.MetaData.Id = new HiveId(Guid.NewGuid());
                                                       });

            return created;
        }

        /// <summary>
        /// Create a typed persistence entity
        /// </summary>
        /// <returns></returns>
        public static Revision<TypedEntity> MockVersionedTypedEntity()
        {
            return MockVersionedTypedEntity(false);
        }

        public static EntitySchema MockEntitySchema(string testDoctypeAlias, string testDoctypeName)
        {
            AttributeGroup groupDefinition = CreateAttributeGroup("tab-1", "Tab 1", 0);
            var generalGroup = FixedGroupDefinitions.GeneralGroup;

            AttributeType typeDefinition = CreateAttributeType(TypeAlias1,
                                                               "test-type-name",
                                                               "test-type-description");
            AttributeType typeDefinition2 = CreateAttributeType(TypeAlias2,
                                                                "test-type-name2",
                                                                "test-type-description2");

            AttributeDefinition attribDef1 = CreateAttributeDefinition("def-alias-1-with-type-1", "name-1", "test-description", typeDefinition, generalGroup);
            AttributeDefinition attribDef2 = CreateAttributeDefinition("def-alias-2-with-type-1", "name-2", "test-description", typeDefinition, groupDefinition);
            AttributeDefinition attribDef3 = CreateAttributeDefinition("def-alias-3-with-type-2", "name-3", "test-description", typeDefinition2, groupDefinition);
            AttributeDefinition attribDef4 = CreateAttributeDefinition("def-alias-4-with-type-2", "name-4", "test-description", typeDefinition2, groupDefinition);


            var schema = CreateEntitySchema(testDoctypeAlias, testDoctypeName,
                                      new[] { attribDef1, attribDef2, attribDef3, attribDef4 });

            schema.TryAddAttributeDefinition(
                CreateAttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Name", "",
                                                                  new AttributeType("nodeName", "Name", "", new StringSerializationType()) {RenderTypeProvider = CorePluginConstants.NodeNamePropertyEditorId}, null));
            schema.TryAddAttributeDefinition(
                CreateAttributeDefinition(SelectedTemplateAttributeDefinition.AliasValue, "Name", "",
                                                                  new AttributeType("selectedTemplate", "Selected Template", "", new StringSerializationType()) { RenderTypeProvider = CorePluginConstants.SelectedTemplatePropertyEditorId }, null));

            //re-order the attribute defs
            foreach(var g in schema.AttributeGroups)
            {
                var attDefs = schema.AttributeDefinitions.Where(x => x.AttributeGroup.Id == g.Id).ToArray();
                for(var i = 0;i<attDefs.Count();i++)
                {
                    attDefs[i].Ordinal = i;
                }
            }

            schema.SetXmlConfigProperty("thumb", "thumb");
            schema.SetXmlConfigProperty("icon", "thumb");

            return schema;
        }

        /// <summary>
        /// Create some properties for a TypedEntity
        /// </summary>
        /// <param name="assignIds"></param>
        /// <param name="callback"></param>
        private static void CreateMockTypedEntityProperties(
            bool assignIds,
            Action<EntitySchema, TypedAttribute[]> callback)
        {

            //create a tab

            var groupDefinition = CreateAttributeGroup("tab-1", "Tab 1", 0);
            if (assignIds) groupDefinition.Id = new HiveId(Guid.NewGuid());
            var generalGroup = FixedGroupDefinitions.GeneralGroup;

            //create some data types

            var typeDefinition = CreateAttributeType(TypeAlias1,
                                                                          "test-type-name",
                                                                          "test-type-description");
            if (assignIds) typeDefinition.Id = new HiveId(Guid.NewGuid());

            var typeDefinition2 = CreateAttributeType(TypeAlias2,
                                                                           "test-type-name2",
                                                                           "test-type-description2");
            if (assignIds) typeDefinition2.Id = new HiveId(Guid.NewGuid());

            //create some documenttype properties

            var attribDef1 = CreateAttributeDefinition(DefAlias1WithType1, "name-1", "test-description", typeDefinition, groupDefinition);
            if (assignIds) attribDef1.Id = new HiveId(Guid.NewGuid());

            var attribDef2 = CreateAttributeDefinition(DefAlias2WithType1, "name-2", "test-description", typeDefinition, groupDefinition);
            if (assignIds) attribDef2.Id = new HiveId(Guid.NewGuid());

            var attribDef3 = CreateAttributeDefinition("def-alias-3-with-type2", "name-3", "test-description", typeDefinition2, groupDefinition);
            if (assignIds) attribDef3.Id = new HiveId(Guid.NewGuid());

            var attribDef4 = CreateAttributeDefinition("def-alias-4-with-type2", "name-4", "test-description", typeDefinition2, groupDefinition);
            if (assignIds) attribDef4.Id = new HiveId(Guid.NewGuid());

            var nodeNameAttributeDefinition = new NodeNameAttributeDefinition(generalGroup);
            if (assignIds) nodeNameAttributeDefinition.Id = new HiveId(Guid.NewGuid());

            var selectedTemplateDefinition = new SelectedTemplateAttributeDefinition(generalGroup);
            if (assignIds) selectedTemplateDefinition.Id = new HiveId(Guid.NewGuid());

            //create the document type

            var schema = CreateEntitySchema("test-doctype-alias", "test-doctype-name",
                                                                    new[] { attribDef1, attribDef2, attribDef3, attribDef4, nodeNameAttributeDefinition, selectedTemplateDefinition });

            if (assignIds) schema.Id = new HiveId(Guid.NewGuid());

            //create some content properties

            var attribute1 = CreateAttribute(attribDef1, "my-test-value1");
            if (assignIds) attribute1.Id = new HiveId(Guid.NewGuid());

            var attribute2 = CreateAttribute(attribDef2, "my-test-value2");
            if (assignIds) attribute2.Id = new HiveId(Guid.NewGuid());

            var attribute3 = CreateAttribute(attribDef3, "my-test-value3");
            if (assignIds) attribute3.Id = new HiveId(Guid.NewGuid());

            var attribute4 = CreateAttribute(attribDef4, "5");
            if (assignIds) attribute4.Id = new HiveId(Guid.NewGuid());

            var nodeNameAttribute = new NodeNameAttribute("my-test-name", generalGroup) { AttributeDefinition = nodeNameAttributeDefinition };
            if (assignIds) nodeNameAttribute.Id = new HiveId(Guid.NewGuid());

            var selectedTemplateAttribute = new SelectedTemplateAttribute(new HiveId(Guid.NewGuid()), generalGroup) { AttributeDefinition = selectedTemplateDefinition };
            if (assignIds) selectedTemplateAttribute.Id = new HiveId(Guid.NewGuid());

            callback(schema, new[] { attribute1, attribute2, attribute3, attribute4, nodeNameAttribute, selectedTemplateAttribute });

        }
    }
}
