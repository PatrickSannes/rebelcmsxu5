using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class ReadOnlyAttributeType : AttributeType
    {
        public const string AliasValue = "system-read-only-type";

        internal ReadOnlyAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents internal system read only values",
                new StringSerializationType())
        {
            Id = FixedHiveIds.ReadOnlyAttributeType;
        }
    }
}
