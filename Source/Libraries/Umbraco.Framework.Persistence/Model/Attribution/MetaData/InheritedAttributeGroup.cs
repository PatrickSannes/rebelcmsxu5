using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Persistence.Model.Attribution.MetaData
{
    public class InheritedAttributeGroup : AttributeGroup
    {
        public EntitySchema Schema { get; set; }

        public InheritedAttributeGroup(AttributeGroup attributeGroup, EntitySchema schema)
            : base(attributeGroup.Alias, attributeGroup.Name, attributeGroup.Ordinal)
        {
            Id = attributeGroup.Id;
            UtcCreated = attributeGroup.UtcCreated;
            UtcModified = attributeGroup.UtcModified;
            UtcStatusChanged = attributeGroup.UtcStatusChanged;

            Schema = schema;
        }
    }
}
