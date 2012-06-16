using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class TextAttributeType : AttributeType
    {
        public const string AliasValue = "system-long-string-type";

        internal TextAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "used internally for built in long string fields for umbraco typed persistence entities", 
            new LongStringSerializationType())
        {
            Id = FixedHiveIds.TextAttributeType;
        }
    }
}