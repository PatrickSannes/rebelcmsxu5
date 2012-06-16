namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    public class ContentRootSchema: SystemSchema
    {
        public const string SchemaAlias = "system-content-schema-root";

        public ContentRootSchema()
        {
            this.Setup(SchemaAlias, "Content Schema Root");
            Id = FixedHiveIds.ContentRootSchema;
            SchemaType = FixedSchemaTypes.Content;
        }
    }
}