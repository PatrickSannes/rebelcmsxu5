using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class SelectedTemplateAttributeType : AttributeType
    {
        public const string AliasValue = "system-selected-template-type";

        internal SelectedTemplateAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents the internal SelectedTemplate",
                new StringSerializationType())
        {
            Id = FixedHiveIds.SelectedTemplateAttributeTypeId;
        }
    }
}