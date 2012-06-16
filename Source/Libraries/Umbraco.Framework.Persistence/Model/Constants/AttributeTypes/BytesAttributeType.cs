using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class BytesAttributeType : AttributeType
    {
        public const string AliasValue = "system-bytearray-type";

        internal BytesAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents an internal system byte array",
                new ByteArraySerializationType())
        {
            Id = FixedHiveIds.ByteArrayAttributeType;
        }
    }
}