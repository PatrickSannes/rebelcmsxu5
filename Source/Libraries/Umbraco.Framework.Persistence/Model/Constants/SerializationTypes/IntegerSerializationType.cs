using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Attribution;

namespace Umbraco.Framework.Persistence.Model.Constants.SerializationTypes
{
    using Umbraco.Framework.Data;

    public class IntegerSerializationType : IAttributeSerializationDefinition
    {
        #region Implementation of IReferenceByName

        public string Alias { get; set; }
        public LocalizedString Name { get; set; }

        #endregion

        #region Implementation of IAttributeSerializationDefinition

        public DataSerializationTypes DataSerializationType { get { return DataSerializationTypes.LargeInt; } }
        public dynamic Serialize(TypedAttribute value)
        {
            return (dynamic)value.DynamicValue;
        }

        #endregion
    }
}