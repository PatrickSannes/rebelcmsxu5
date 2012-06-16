using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class ApplicationsListPickerAttributeType : AttributeType
    {
        public const string AliasValue = "system-applications-list-picker-type";

        internal ApplicationsListPickerAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "This type represents internal system applications list picker", 
            new StringSerializationType())
        {
            Id = FixedHiveIds.ApplicationsListPickerAttributeType;
        }
    }
}
