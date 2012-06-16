using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    public class MediaImageSchema : SystemSchema
    {
        public const string SchemaAlias = "mediaImage";

        public const string UploadFileAlias = "uploadedFile";

        public MediaImageSchema()
        {
            this.Setup(SchemaAlias, "Image");

            Id = FixedHiveIds.MediaImageSchema;
            SchemaType = FixedSchemaTypes.Content;

            RelationProxies.EnlistParent(FixedSchemas.MediaRootSchema, FixedRelationTypes.DefaultRelationType);

            var nameNameType = new NodeNameAttributeType();
            var fileUploadType = new FileUploadAttributeType();

            AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
                {
                    Id = new HiveId("mi-name".EncodeAsGuid()),
                    AttributeType = nameNameType,
                    AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                    Ordinal = 0
                });

            AttributeDefinitions.Add(new AttributeDefinition(UploadFileAlias, "Uploaded File")
                {
                    Id = new HiveId("mi-upload".EncodeAsGuid()),
                    AttributeType = fileUploadType,
                    AttributeGroup = FixedGroupDefinitions.FileProperties,
                    Ordinal = 0
                });

            SetXmlConfigProperty("thumb", "image1.png");
            SetXmlConfigProperty("icon", "image.png");
            //TODO: Need to change this to be a 'key' lookup for localization
            SetXmlConfigProperty("description", "An image");
            //save the allowed children as a list of HiveId's as string
            SetXmlConfigProperty("allowed-children", new string[] { });
        }
    }
}