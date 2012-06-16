using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class BoolAttributeType : AttributeType
    {
        public const string AliasValue = "system-bool-type";

        internal BoolAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents internal system booleans",
                new BoolSerializationType())
        {
            Id = FixedHiveIds.BoolAttributeType;
        }
    }
}