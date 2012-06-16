using System;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.ModelFirst;
using Umbraco.Framework.Persistence.ModelFirst.Annotations;

namespace Umbraco.Framework.Persistence.Model.Constants.Entities
{
    /// <summary>
    /// Represents a member
    /// </summary>
    public class Member : CustomTypedEntity
    {
        public Member()
        {
            this.SetupFromSchema<MemberSchema>();
        }

        /// <summary>
        /// The unique identifier for the user for use with Membership services
        /// </summary>
        public object ProviderUserKey
        {
            get { return Id.Value.Value; }
        }

        [AttributeAlias(Alias = MemberSchema.IsOnlineAlias)]
        public bool IsOnline
        {
            get { return base.BaseAutoGet<bool>(MemberSchema.IsOnlineAlias); }
            set { base.BaseAutoSet(MemberSchema.IsOnlineAlias, value); }
        }

        [AttributeAlias(Alias = MemberSchema.IsLockedOutAlias)]
        public bool IsLockedOut
        {
            get { return base.BaseAutoGet<bool>(MemberSchema.IsLockedOutAlias); }
            set { base.BaseAutoSet(MemberSchema.IsLockedOutAlias, value); }
        }

        [AttributeAlias(Alias = MemberSchema.LastLockoutDateAlias)]
        public DateTimeOffset LastLockoutDate
        {
            get { return base.BaseAutoGet<DateTimeOffset>(MemberSchema.LastLockoutDateAlias); }
            set { base.BaseAutoSet(MemberSchema.LastLockoutDateAlias, value); }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [AttributeAlias(Alias = NodeNameAttributeDefinition.AliasValue)]
        public string Name
        {
            get { return base.BaseAutoGet<string>(NodeNameAttributeDefinition.AliasValue); }
            set { base.BaseAutoSet(NodeNameAttributeDefinition.AliasValue, value); }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.UsernameAlias)]
        public string Username
        {
            get { return base.BaseAutoGet<string>(MemberSchema.UsernameAlias); }
            set { base.BaseAutoSet(MemberSchema.UsernameAlias, value); }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.PasswordAlias)]
        public string Password
        {
            get { return base.BaseAutoGet<string>(MemberSchema.PasswordAlias); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    base.BaseAutoSet(MemberSchema.PasswordAlias, value);
            }
        }

        /// <summary>
        /// Gets or sets the password question.
        /// </summary>
        /// <value>
        /// The password question.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.PasswordQuestionAlias)]
        public string PasswordQuestion
        {
            get { return base.BaseAutoGet<string>(MemberSchema.PasswordQuestionAlias); }
            set { base.BaseAutoSet(MemberSchema.PasswordQuestionAlias, value); }
        }

        /// <summary>
        /// Gets or sets the password answer.
        /// </summary>
        /// <value>
        /// The password answer.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.PasswordAnswerAlias)]
        public string PasswordAnswer
        {
            get { return base.BaseAutoGet<string>(MemberSchema.PasswordAnswerAlias); }
            set { base.BaseAutoSet(MemberSchema.PasswordAnswerAlias, value); }
        }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.EmailAlias)]
        public string Email
        {
            get { return base.BaseAutoGet<string>(MemberSchema.EmailAlias); }
            set { base.BaseAutoSet(MemberSchema.EmailAlias, value); }
        }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.CommentsAlias)]
        public string Comments
        {
            get { return base.BaseAutoGet<string>(MemberSchema.CommentsAlias); }
            set { base.BaseAutoSet(MemberSchema.CommentsAlias, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is approved.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is approved; otherwise, <c>false</c>.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.IsApprovedAlias)]
        public bool IsApproved
        {
            get { return base.BaseAutoGet<bool>(MemberSchema.IsApprovedAlias); }
            set { base.BaseAutoSet(MemberSchema.IsApprovedAlias, value); }
        }

        /// <summary>
        /// Gets or sets the last login date.
        /// </summary>
        /// <value>
        /// The last login date.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.LastLoginDateAlias)]
        public DateTimeOffset LastLoginDate
        {
            get { return base.BaseAutoGet<DateTimeOffset>(MemberSchema.LastLoginDateAlias, DateTimeOffset.MinValue); }
            set { base.BaseAutoSet(MemberSchema.LastLoginDateAlias, value); }
        }

        /// <summary>
        /// Gets or sets the last activity date.
        /// </summary>
        /// <value>
        /// The last activity date.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.LastActivityDateAlias)]
        public DateTimeOffset LastActivityDate
        {
            get { return base.BaseAutoGet<DateTimeOffset>(MemberSchema.LastActivityDateAlias, DateTimeOffset.MinValue); }
            set { base.BaseAutoSet(MemberSchema.LastActivityDateAlias, value); }
        }

        /// <summary>
        /// Gets or sets the last password change date.
        /// </summary>
        /// <value>
        /// The last password change date.
        /// </value>
        [AttributeAlias(Alias = MemberSchema.LastPasswordChangeDateAlias)]
        public DateTimeOffset LastPasswordChangeDate
        {
            get { return base.BaseAutoGet(MemberSchema.LastPasswordChangeDateAlias, DateTimeOffset.MinValue); }
            set { base.BaseAutoSet(MemberSchema.LastPasswordChangeDateAlias, value); }
        }
    }
}