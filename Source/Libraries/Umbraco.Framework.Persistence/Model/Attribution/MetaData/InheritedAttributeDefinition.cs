using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Persistence.Model.Attribution.MetaData
{
    public class InheritedAttributeDefinition : AttributeDefinition
    {
        public EntitySchema Schema { get; set; }

        //public new InheritedAttributeGroup AttributeGroup { get; set; }

        public InheritedAttributeDefinition(AttributeDefinition attributeDefinition, EntitySchema schema)
            : base(attributeDefinition.Alias, attributeDefinition.Name)
        {
            Id = attributeDefinition.Id;
            Description = attributeDefinition.Description;
            AttributeType = attributeDefinition.AttributeType;
            Ordinal = attributeDefinition.Ordinal;
            RenderTypeProviderConfigOverride = attributeDefinition.RenderTypeProviderConfigOverride;
            AttributeGroup = new InheritedAttributeGroup(attributeDefinition.AttributeGroup, schema);
            UtcCreated = attributeDefinition.UtcCreated;
            UtcModified = attributeDefinition.UtcModified;
            UtcStatusChanged = attributeDefinition.UtcStatusChanged;

            Schema = schema;
        }
    }
}
