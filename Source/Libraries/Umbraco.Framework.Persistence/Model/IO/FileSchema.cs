using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.IO
{
    public class FileSchema : EntitySchema
    {
        public FileSchema()
        {
            SchemaType = FixedSchemaTypes.File;
            AttributeDefinitions.Add(new AttributeDefinition
            {
                Alias = "name",
                Name = "Name",
                AttributeType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue),
                AttributeGroup = FixedGroupDefinitions.GeneralGroup
            });

            AttributeDefinitions.Add(new AttributeDefinition
            {
                Alias = "rootedPath",
                Name = "RootedPath",
                AttributeType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue),
                AttributeGroup = FixedGroupDefinitions.GeneralGroup
            });

            AttributeDefinitions.Add(new AttributeDefinition
            {
                Alias = "rootRelativePath",
                Name = "Root Relative Path",
                AttributeType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue),
                AttributeGroup = FixedGroupDefinitions.GeneralGroup
            });

            AttributeDefinitions.Add(new AttributeDefinition
            {
                Alias = "publicUrl",
                Name = "Public URL",
                AttributeType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue),
                AttributeGroup = FixedGroupDefinitions.GeneralGroup
            });

            AttributeDefinitions.Add(new AttributeDefinition
            {
                Alias = "isContainer",
                Name = "Is Container",
                AttributeType = AttributeTypeRegistry.Current.GetAttributeType(IntegerAttributeType.AliasValue),
                AttributeGroup = FixedGroupDefinitions.GeneralGroup
            });

            AttributeDefinitions.Add(new AttributeDefinition
            {
                Alias = "contentBytes",
                Name = "Content Bytes",
                AttributeType = new BytesAttributeType(),
                AttributeGroup = FixedGroupDefinitions.GeneralGroup
            });
        }
    }
}
