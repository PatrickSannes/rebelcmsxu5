using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class FileUploadAttributeType : AttributeType
    {
        public const string AliasValue = "system-file-upload-type";

        internal FileUploadAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents the internal file upload type",
                new StringSerializationType())
        {
            Id = FixedHiveIds.FileUploadAttributeType;
        }
    }
}