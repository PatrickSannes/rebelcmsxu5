using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.Schemas
{
    public class MemberSchema : SystemSchema
    {
        public const string SchemaAlias = "system-member";
        public const string IsLockedOutAlias = "isLockedOut";
        public const string IsOnlineAlias = "isOnline";
        public const string LastLockoutDateAlias = "lastLockedOutDate";
        public const string UsernameAlias = "username";
        public const string PasswordAlias = "password";
        public const string PasswordQuestionAlias = "passwordQuestion";
        public const string PasswordAnswerAlias = "passwordAnswer";
        public const string CommentsAlias = "comments";
        public const string EmailAlias = "email";
        public const string IsApprovedAlias = "isApproved";
        public const string LastLoginDateAlias = "lastLoginDate";
        public const string LastActivityDateAlias = "lastActivityDate";
        public const string LastPasswordChangeDateAlias = "lastPasswordChangeDate";

        public MemberSchema()
        {
            this.Setup(SchemaAlias, "Member");

            Id = FixedHiveIds.MemberSchema;
            SchemaType = FixedSchemaTypes.Member;
            SetupAttributeDefinitions();
        }

        private void SetupAttributeDefinitions()
        {
            CreatedAttributeDefinitions();
        }

        private readonly AttributeGroup _detailsGroup = FixedGroupDefinitions.MemberDetails;

        protected virtual AttributeGroup DetailsGroup
        {
            get { return _detailsGroup; }
        }

        protected virtual void CreatedAttributeDefinitions()
        {
            var inBuiltTextType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue);
            var inBuiltBoolType = AttributeTypeRegistry.Current.GetAttributeType(BoolAttributeType.AliasValue);
            var inBuiltReadOnlyType = AttributeTypeRegistry.Current.GetAttributeType(ReadOnlyAttributeType.AliasValue);

            this.AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
                {
                    Id = new HiveId("u-name".EncodeAsGuid()),
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 0
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(IsOnlineAlias, "Is Online")
                {
                    Id = new HiveId("u-isonline".EncodeAsGuid()),
                    AttributeType = inBuiltReadOnlyType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 1
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(IsLockedOutAlias, "Is Locked Out")
                {
                    Id = new HiveId("u-lockedout".EncodeAsGuid()),
                    AttributeType = inBuiltBoolType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 1
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(LastLockoutDateAlias, "Last Lockout Date")
                {
                    Id = new HiveId("u-lastlockedout".EncodeAsGuid()),
                    AttributeType = inBuiltReadOnlyType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 1
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(UsernameAlias, "Username")
                {
                    Id = new HiveId("u-username".EncodeAsGuid()),
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 1
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(PasswordAlias, "Password")
                {
                    Id = new HiveId("u-password".EncodeAsGuid()),
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 2
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(PasswordQuestionAlias, "Password question")
                {
                    Id = new HiveId("u-password-question".EncodeAsGuid()),
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 4
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(PasswordAnswerAlias, "Password answer")
                {
                    Id = new HiveId("u-password-answer".EncodeAsGuid()),
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 5
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(EmailAlias, "Email address")
                {
                    Id = new HiveId("u-email".EncodeAsGuid()),
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 6
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(CommentsAlias, "Comments / Notes")
                {
                    Id = new HiveId("u-comments".EncodeAsGuid()),
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 7
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(IsApprovedAlias, "Is user approved?")
                {
                    Id = new HiveId("u-is-approved".EncodeAsGuid()),
                    AttributeType = inBuiltBoolType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 8
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(LastLoginDateAlias, "Last login date")
                {
                    Id = new HiveId("u-last-login-date".EncodeAsGuid()),
                    AttributeType = inBuiltReadOnlyType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 10
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(LastActivityDateAlias, "Last activity date")
                {
                    Id = new HiveId("u-last-activity-date".EncodeAsGuid()),
                    AttributeType = inBuiltReadOnlyType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 11
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(LastPasswordChangeDateAlias, "Last password change date")
                {
                    Id = new HiveId("u-last-password-change-date".EncodeAsGuid()),
                    AttributeType = inBuiltReadOnlyType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 12
                });
        }

    }
}