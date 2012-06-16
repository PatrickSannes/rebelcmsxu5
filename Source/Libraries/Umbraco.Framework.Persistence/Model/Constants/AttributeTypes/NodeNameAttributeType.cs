using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class NodeNameAttributeType : AttributeType
    {
        public const string AliasValue = "system-node-name-type";

        internal NodeNameAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents the internal NodeName",
                new StringSerializationType())
        {
            Id = FixedHiveIds.NodeNameAttributeTypeId;
        }
    }
}