using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class MediaPickerAttributeType : AttributeType
    {
        public const string AliasValue = "system-media-picker-type";

        internal MediaPickerAttributeType()
            : base(
            AliasValue,
            AliasValue,
            "This type represents internal system media picker",
            new StringSerializationType())
        {
            Id = FixedHiveIds.MediaPickerAttributeType;
        }
    }
}
