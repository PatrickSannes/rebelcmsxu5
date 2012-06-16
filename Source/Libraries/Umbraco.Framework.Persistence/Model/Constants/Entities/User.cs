using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.ModelFirst.Annotations;

namespace Umbraco.Framework.Persistence.Model.Constants.Entities
{
    /// <summary>
    /// A User entity
    /// </summary>
    public class User : Member
    {
        public User()
        {
            this.SetupFromSchema<UserSchema>();

            //A user is always under the User virtual root
            this.RelationProxies.EnlistParent(FixedEntities.UserVirtualRoot, FixedRelationTypes.DefaultRelationType);
        }

        /// <summary>
        /// Gets or sets the password salt.
        /// </summary>
        /// <value>
        /// The password salt.
        /// </value>
        [AttributeAlias(Alias = UserSchema.PasswordSaltAlias)]
        public string PasswordSalt
        {
            get { return base.BaseAutoGet<string>(UserSchema.PasswordSaltAlias); }
            set { base.BaseAutoSet(UserSchema.PasswordSaltAlias, value); }
        }

        /// <summary>
        /// Gets or sets the session timeout.
        /// </summary>
        /// <value>
        /// The session timeout.
        /// </value>
        [AttributeAlias(Alias = UserSchema.SessionTimeoutAlias)]
        public int SessionTimeout
        {
            get { return base.BaseAutoGet<int>(UserSchema.SessionTimeoutAlias, 60); }
            set { base.BaseAutoSet(UserSchema.SessionTimeoutAlias, value); }
        }

        /// <summary>
        /// Gets or sets the start content hive id.
        /// </summary>
        /// <value>
        /// The start content hive id.
        /// </value>
        [AttributeAlias(Alias = UserSchema.StartContentHiveIdAlias)]
        public HiveId StartContentHiveId
        {
            get { return base.BaseAutoGet<HiveId>(UserSchema.StartContentHiveIdAlias, HiveId.Empty); }
            set { base.BaseAutoSet(UserSchema.StartContentHiveIdAlias, value); }
        }

        /// <summary>
        /// Gets or sets the start media hive id.
        /// </summary>
        /// <value>
        /// The start media hive id.
        /// </value>
        [AttributeAlias(Alias = UserSchema.StartMediaHiveIdAlias)]
        public HiveId StartMediaHiveId
        {
            get { return base.BaseAutoGet<HiveId>(UserSchema.StartMediaHiveIdAlias, HiveId.Empty); }
            set { base.BaseAutoSet(UserSchema.StartMediaHiveIdAlias, value); }
        }

        /// <summary>
        /// Gets or sets the applications.
        /// </summary>
        /// <value>
        /// The applications.
        /// </value>
        public IEnumerable<string> Applications
        {
            get
            {
                return Attributes[UserSchema.ApplicationsAlias].Values.Select(x => x.Value.ToString()).ToList();
            }
            set
            {
                Attributes[UserSchema.ApplicationsAlias].Values.Clear();
                var count = 0;
                foreach (var item in value)
                {
                    Attributes[UserSchema.ApplicationsAlias].Values.Add("val" + count, item);
                    count++;
                }
            }
        }
    }
}