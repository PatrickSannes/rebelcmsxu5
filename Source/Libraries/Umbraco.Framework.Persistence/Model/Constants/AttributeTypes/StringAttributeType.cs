using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class StringAttributeType : AttributeType
    {
        public const string AliasValue = "system-string-type";

        internal StringAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "This type represents internal system text", 
            new StringSerializationType())
        {
            Id = FixedHiveIds.StringAttributeType;
        }
    }
}