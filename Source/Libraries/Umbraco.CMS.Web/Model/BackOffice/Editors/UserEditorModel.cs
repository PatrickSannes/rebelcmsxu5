using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework;
using System.ComponentModel.DataAnnotations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    [Bind(Exclude = "LastLoginDate,LastActivityDate,LastPasswordChangeDate")]
    public class UserEditorModel : MemberEditorModel
    {
        public UserEditorModel(ReadonlyGroupUnitFactory hive)
            : base(hive)
        {
            ConfirmPassword = Password;
            UserGroups = new List<HiveId>();
        }

        /// <summary>
        /// Parent is always the UserVirtualRoot
        /// </summary>
        public override HiveId ParentId
        {
            get { return FixedHiveIds.UserVirtualRoot; }
            set { return; }
        }


        [Range(5, int.MaxValue)]
        [Required]
        public int SessionTimeout
        {
            get { return GetPropertyEditorModelValue(UserSchema.SessionTimeoutAlias, x => x.ValueAsInteger) ?? 60; }
            set { SetPropertyEditorModelValue(UserSchema.SessionTimeoutAlias, x => x.ValueAsInteger = value); }
        }

        public HiveId StartContentHiveId
        {
            get { return GetPropertyEditorModelValue(UserSchema.StartContentHiveIdAlias, x => x.Value) ?? HiveId.Empty; }
            set { SetPropertyEditorModelValue(UserSchema.StartContentHiveIdAlias, x => x.Value = value); }
        }

        public HiveId StartMediaHiveId
        {
            get { return GetPropertyEditorModelValue(UserSchema.StartMediaHiveIdAlias, x => x.Value) ?? HiveId.Empty; }
            set { SetPropertyEditorModelValue(UserSchema.StartMediaHiveIdAlias, x => x.Value = value); }
        }

        public IEnumerable<string> Applications
        {
            get { return GetPropertyEditorModelValue(UserSchema.ApplicationsAlias, x => x.Value); }
            set { SetPropertyEditorModelValue(UserSchema.ApplicationsAlias, x => x.Value = value); }
        }

        [Required]
        public IEnumerable<HiveId> UserGroups { get; set; }

        /// <summary>
        /// Validates the input
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //we need to validate if the user already exists
            if ((Id.IsNullValueOrEmpty()))
            {
                using (var uow = Hive.Create())
                {
                    //TODO: Fix the query context to support stuff like Any() without executing and returning all of the data.
                    if (uow.Repositories.QueryContext.Query<User>().Where(x => x.Username == Username).ToArray().Any())
                    {
                        yield return new ValidationResult("A User with the specified Username already exists", new[] { "Username" });
                    }
                }
            }
            
            if (UserGroups == null || !UserGroups.Any())
            {
                yield return new ValidationResult("A user must belong to at least one user group", new[] { "UserGroups" });
            }
        }
    }
}
