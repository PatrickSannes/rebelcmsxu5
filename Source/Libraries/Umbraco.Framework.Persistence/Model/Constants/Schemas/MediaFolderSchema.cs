using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    public class MediaFolderSchema : SystemSchema
    {
        public const string SchemaAlias = "mediaFolder";

        public MediaFolderSchema()
        {
            this.Setup(SchemaAlias, "Folder");

            Id = FixedHiveIds.MediaFolderSchema;
            SchemaType = FixedSchemaTypes.Content;

            RelationProxies.EnlistParent(FixedSchemas.MediaRootSchema, FixedRelationTypes.DefaultRelationType);

            var nameNameType = new NodeNameAttributeType();

            AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
                {
                    Id = new HiveId("mf-name".EncodeAsGuid()),
                    AttributeType = nameNameType,
                    AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                    Ordinal = 0
                });

            SetXmlConfigProperty("thumb", "folder_media.png");
            SetXmlConfigProperty("icon", "tree-folder");
            //TODO: Need to change this to be a 'key' lookup for localization
            SetXmlConfigProperty("description", "A folder for media");
            //save the allowed children as a list of HiveId's as string
            SetXmlConfigProperty("allowed-children", new[]
                {
                    FixedHiveIds.MediaFolderSchema.ToString(),
                    FixedHiveIds.MediaImageSchema.ToString()
                });
        }
    }
}