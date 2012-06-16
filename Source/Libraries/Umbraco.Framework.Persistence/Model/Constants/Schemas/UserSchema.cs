using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    public class UserSchema : MemberSchema
    {
        public new const string SchemaAlias = "system-user";
        public const string PasswordSaltAlias = "passwordSalt";
        public const string PersistLoginAlias = "persistLogin";
        public const string SessionTimeoutAlias = "persistedLoginDuration";
        public const string StartContentHiveIdAlias = "startContentHiveId";
        public const string StartMediaHiveIdAlias = "startMediaHiveId";
        public const string ApplicationsAlias = "applications";

        public UserSchema()
        {
            this.Setup(SchemaAlias, "User");

            Id = FixedHiveIds.UserSchema;
            SchemaType = FixedSchemaTypes.User;
        }

        private readonly AttributeGroup _detailsGroup = FixedGroupDefinitions.UserDetails;

        protected override AttributeGroup DetailsGroup
        {
            get { return _detailsGroup; }
        }

        protected override void CreatedAttributeDefinitions()
        {
            base.CreatedAttributeDefinitions();

            var inBuiltTextType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue);
            var inBuiltBoolType = AttributeTypeRegistry.Current.GetAttributeType(BoolAttributeType.AliasValue);
            var inBuiltReadOnlyType = AttributeTypeRegistry.Current.GetAttributeType(ReadOnlyAttributeType.AliasValue);
            var inBuiltContentPickerType = AttributeTypeRegistry.Current.GetAttributeType(ContentPickerAttributeType.AliasValue);
            var inBuiltMediaPickerType = AttributeTypeRegistry.Current.GetAttributeType(MediaPickerAttributeType.AliasValue);
            var inBuiltApplicationsListPickerType = AttributeTypeRegistry.Current.GetAttributeType(ApplicationsListPickerAttributeType.AliasValue);
            var inBuildIntegerType = AttributeTypeRegistry.Current.GetAttributeType(IntegerAttributeType.AliasValue);

            this.AttributeDefinitions.Add(new AttributeDefinition(PasswordSaltAlias, "Password salt")
            {
                Id = new HiveId("u-password-salt".EncodeAsGuid()),
                AttributeType = inBuiltReadOnlyType,
                AttributeGroup = DetailsGroup,
                Ordinal = 3
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(SessionTimeoutAlias, "Persisted login duration")
            {
                Id = new HiveId("u-persisted-login-duration".EncodeAsGuid()),
                AttributeType = inBuildIntegerType,
                AttributeGroup = DetailsGroup,
                Ordinal = 9
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(StartContentHiveIdAlias, "Start node in Content")
            {
                Id = new HiveId("u-start-content-hive-id".EncodeAsGuid()),
                AttributeType = inBuiltContentPickerType,
                AttributeGroup = DetailsGroup,
                Ordinal = 13
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(StartMediaHiveIdAlias, "Start node in Media")
            {
                Id = new HiveId("u-start-media-hive-id".EncodeAsGuid()),
                AttributeType = inBuiltMediaPickerType,
                AttributeGroup = DetailsGroup,
                Ordinal = 14
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(ApplicationsAlias, "Sections")
            {
                Id = new HiveId("u-applications".EncodeAsGuid()),
                AttributeType = inBuiltApplicationsListPickerType,
                AttributeGroup = DetailsGroup,
                Ordinal = 15
            });
        }

        

        
    }
}
