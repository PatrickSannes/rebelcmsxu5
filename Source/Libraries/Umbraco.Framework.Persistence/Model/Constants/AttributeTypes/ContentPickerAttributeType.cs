using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class ContentPickerAttributeType : AttributeType
    {
        public const string AliasValue = "system-content-picker-type";

        internal ContentPickerAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "This type represents internal system content picker", 
            new StringSerializationType())
        {
            Id = FixedHiveIds.ContentPickerAttributeType;
        }
    }
}
