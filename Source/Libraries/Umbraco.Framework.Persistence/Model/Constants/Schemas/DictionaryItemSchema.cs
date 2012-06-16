using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    public class DictionaryItemSchema : SystemSchema
    {
        public const string SchemaAlias = "system-dictionary-item-schema";

        public const string TranslationsAlias = "translations";

        public DictionaryItemSchema()
        {
            this.Setup(SchemaAlias, "Dictionary Item Schema");

            Id = FixedHiveIds.DictionaryItemSchema;
            SchemaType = FixedSchemaTypes.Content;

            RelationProxies.EnlistParent(FixedSchemas.DictionaryRootSchema, FixedRelationTypes.DefaultRelationType);

            var nodeNameType = new NodeNameAttributeType();
            var translationsType = new DictionaryItemTranslationsAttributeType();

            AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
            {
                Id = new HiveId("di-name".EncodeAsGuid()),
                AttributeType = nodeNameType,
                AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                Ordinal = 0
            });

            AttributeDefinitions.Add(new AttributeDefinition(TranslationsAlias, "Translations")
            {
                Id = new HiveId("di-translations".EncodeAsGuid()),
                AttributeType = translationsType,
                AttributeGroup = FixedGroupDefinitions.Translations,
                Ordinal = 0
            });

            SetXmlConfigProperty("thumb", "developer.png");
            SetXmlConfigProperty("icon", "dictionary.gif");
            SetXmlConfigProperty("description", "A dictionary item");

            //save the allowed children as a list of HiveId's as string
            SetXmlConfigProperty("allowed-children", new string[] { FixedHiveIds.DictionaryItemSchema.ToString() });
        }
    }

}