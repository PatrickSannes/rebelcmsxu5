using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataAnnotationsExtensions;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class MemberEditorModel : BasicContentEditorModel, IValidatableObject
    {
        public ReadonlyGroupUnitFactory Hive { get; private set; }

        public MemberEditorModel(ReadonlyGroupUnitFactory hive)
        {
            Hive = hive;
        }

        /// <summary>
        /// Override the 'Name' property to lookup/retreive from the Name dynamic property of this object
        /// </summary>
        [Required]
        public override string Name
        {
            get { return GetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Value); }
            set { SetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Value = value); }
        }

        [Required]
        public string Username
        {
            get { return GetPropertyEditorModelValue(MemberSchema.UsernameAlias, x => x.Value); }
            set { SetPropertyEditorModelValue(MemberSchema.UsernameAlias, x => x.Value = value); }
        }

        [DataType(global::System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password
        {
            get { return GetPropertyEditorModelValue(MemberSchema.PasswordAlias, x => x.Value); }
            set { SetPropertyEditorModelValue(MemberSchema.PasswordAlias, x => x.Value = value); }
        }

        [EqualTo("Password")]
        [DataType(global::System.ComponentModel.DataAnnotations.DataType.Password)]
        public string ConfirmPassword { get; set; }

        [DataType(global::System.ComponentModel.DataAnnotations.DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [Email]
        [DataType(global::System.ComponentModel.DataAnnotations.DataType.EmailAddress)]
        public string Email
        {
            get { return GetPropertyEditorModelValue(MemberSchema.EmailAlias, x => x.Value); }
            set { SetPropertyEditorModelValue(MemberSchema.EmailAlias, x => x.Value = value); }
        }

        public bool IsApproved
        {
            get { return GetPropertyEditorModelValue(MemberSchema.IsApprovedAlias, x => x.Value) ?? false; }
            set { SetPropertyEditorModelValue(MemberSchema.IsApprovedAlias, x => x.Value = value); }
        }

        [ReadOnly(true)]
        public DateTime? LastLoginDate
        {
            get
            {
                var val = GetPropertyEditorModelValue(MemberSchema.LastLoginDateAlias, x => x.Value);
                return (val != null && val.ToString() != string.Empty)
                           ? val is DateTime ? val : DateTime.Parse(val)
                           : null;
            }
            set { SetPropertyEditorModelValue(MemberSchema.LastLoginDateAlias, x => x.Value = value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""); }
        }

        [ReadOnly(true)]
        public DateTime? LastActivityDate
        {
            get
            {
                var val = GetPropertyEditorModelValue(MemberSchema.LastActivityDateAlias, x => x.Value);
                return (val != null && val.ToString() != string.Empty)
                           ? val is DateTime ? val : DateTime.Parse(val)
                           : null;
            }
            set { SetPropertyEditorModelValue(MemberSchema.LastActivityDateAlias, x => x.Value = value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""); }
        }

        [ReadOnly(true)]
        public DateTime? LastPasswordChangeDate
        {
            get
            {
                var val = GetPropertyEditorModelValue(MemberSchema.LastPasswordChangeDateAlias, x => x.Value);
                return (val != null && val.ToString() != string.Empty)
                           ? val is DateTime ? val : DateTime.Parse(val)
                           : null;
            }
            set { SetPropertyEditorModelValue(MemberSchema.LastPasswordChangeDateAlias, x => x.Value = value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""); }
        }

        /// <summary>
        /// Validates the input
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //we need to validate if the user already exists
            if ((Id.IsNullValueOrEmpty()))
            {
                using (var uow = Hive.Create())
                {
                    //TODO: Fix the query context to support stuff like Any() without executing and returning all of the data.
                    if (uow.Repositories.QueryContext.Query<Member>().Where(x => x.Username == Username).ToArray().Any())
                    {
                        yield return new ValidationResult("A Member with the specified Username already exists", new[] { "Username" });
                    }
                }
            }

        }

        protected override void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}