using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class IntegerAttributeType : AttributeType
    {
        public const string AliasValue = "system-integer-type";

        internal IntegerAttributeType()
            : base(
            AliasValue,
            AliasValue,
            "used internally for built in integer fields for umbraco typed persistence entities",
            new IntegerSerializationType())
        {
            Id = FixedHiveIds.IntegerAttributeType;
        }
    }
}