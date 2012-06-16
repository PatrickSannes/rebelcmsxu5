namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    public class DictionaryRootSchema : SystemSchema
    {
        public const string SchemaAlias = "system-dictionary-schema-root";

        public DictionaryRootSchema()
        {
            this.Setup(SchemaAlias, "Dictionary Schema Root");
            Id = FixedHiveIds.DictionaryRootSchema;
            //the schema type is the same! no need for a different type as its all the same we just have schemas starting at a different path
            SchemaType = FixedSchemaTypes.Content;

            SetXmlConfigProperty("allowed-children", new string[] { FixedHiveIds.DictionaryItemSchema.ToString() });
        }
    }

}