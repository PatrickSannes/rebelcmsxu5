using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.Schemas  
{
    public class UserGroupSchema : SystemSchema
    {
        public const string SchemaAlias = "system-user-group";
        public const string UsersAlias = "users";

        public UserGroupSchema()
        {
            this.Setup(SchemaAlias, "User Group");
            
            Id = FixedHiveIds.UserGroupSchema;
            SchemaType = FixedSchemaTypes.UserGroup;
            var userGroupDetails = FixedGroupDefinitions.UserGroupDetails;

            RelationProxies.EnlistParentById(FixedHiveIds.SystemRoot, FixedRelationTypes.PermissionRelationType);

            var inBuiltTextType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue);
            var inBuiltUserGroupUsersType = AttributeTypeRegistry.Current.GetAttributeType(UserGroupUsersAttributeType.AliasValue);

            this.AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Name")
            {
                Id = new HiveId("ug-name".EncodeAsGuid()),
                AttributeType = inBuiltTextType,
                AttributeGroup = userGroupDetails,
                Ordinal = 0,
                Description = "user group name"
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(UsersAlias, "Users")
            {
                Id = new HiveId("ug-users".EncodeAsGuid()),
                AttributeType = inBuiltUserGroupUsersType,
                AttributeGroup = userGroupDetails,
                Ordinal = 1,
                Description = "user group members"
            });

        }
    }
}
