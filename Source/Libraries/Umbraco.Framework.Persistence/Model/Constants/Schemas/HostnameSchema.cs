using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    /// <summary>
    /// Represents the schema for a hostname entity
    /// </summary>
    public class HostnameSchema : SystemSchema
    {
        public const string SchemaAlias = "system-hostname-schema";
        public const string HostnameAlias = "system-internal-host-name";

        public HostnameSchema()
        {
            this.Setup(SchemaAlias, "Hostname Schema");

            var inBuiltReadOnlyType = AttributeTypeRegistry.Current.GetAttributeType(ReadOnlyAttributeType.AliasValue);

            Id = FixedHiveIds.HostnameSchema;
            SchemaType = FixedSchemaTypes.Hostname;

            AttributeDefinitions.Add(new AttributeDefinition(HostnameAlias, "Hostname")
            {
                Id = new HiveId("hn-name".EncodeAsGuid()),
                AttributeType = inBuiltReadOnlyType,
                AttributeGroup = FixedGroupDefinitions.GeneralGroup,
                Ordinal = 0
            });
        }
    }
}