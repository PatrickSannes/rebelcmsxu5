namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    public class MediaRootSchema : SystemSchema
    {
        public const string SchemaAlias = "system-media-schema-root";

        public MediaRootSchema()
        {
            this.Setup(SchemaAlias, "Media Schema Root");
            Id = FixedHiveIds.MediaRootSchema;
            //the schema type is the same! no need for a different type as its all the same we just have schemas starting at a different path
            SchemaType = FixedSchemaTypes.Content;
        }
    }

}