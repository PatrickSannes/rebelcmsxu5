#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework;

using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

#endregion

namespace Umbraco.Cms.Web.Security
{
    public class BackOfficeMembershipProvider : MembershipProvider
    {
        private IUmbracoApplicationContext _appContext;
        private GroupUnitFactory<ISecurityStore> _hive;

        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private int _maxInvalidPasswordAttempts;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private int _passwordAttemptWindow;
        private MembershipPasswordFormat _passwordFormat;
        private string _passwordStrengthRegularExpression;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "T:System.Web.Security.MembershipProvider" /> class.
        /// </summary>
        public BackOfficeMembershipProvider()
        {
            _appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            _hive = _appContext.Hive.GetWriter<ISecurityStore>(new Uri("security://users"));
        }

        #region MembershipProvider Implementation

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName { get; set; }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>
        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <returns>The minimum length required for a password. </returns>
        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"/> values indicating the format for storing passwords in the data store.</returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <returns>A regular expression used to evaluate a password.</returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">The current password for the specified user.</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            using (var uow = _hive.Create())
            {
                var user = GetUmbracoUser(_appContext, uow, username, false);

                if (user == null) return false;

                if (ValidateUserInternal(user, oldPassword))
                {
                    var args = new ValidatePasswordEventArgs(username, newPassword, false);

                    base.OnValidatingPassword(args);

                    if (args.Cancel)
                    {
                        if (args.FailureInformation != null)
                            throw args.FailureInformation;

                        throw new MembershipPasswordException("Change password canceled due to new password validation failure.");
                    }

                    if (!ValidatePassword(newPassword))
                        throw new ArgumentException("Password doesn't meet password strength requirements!");

                    var salt = "";
                    user.Password = TransformPassword(newPassword, ref salt);
                    user.PasswordSalt = salt;
                    user.LastPasswordChangeDate = DateTime.UtcNow;

                    uow.Repositories.AddOrUpdate(user);
                    uow.Complete();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <param name="username">The user to change the password question and answer for.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <param name="newPasswordQuestion">The new password question for the specified user.</param>
        /// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password,
                                                             string newPasswordQuestion, string newPasswordAnswer)
        {
            using (var uow = _hive.Create())
            {
                var user = GetUmbracoUser(_appContext, uow, username, false);

                if (user == null) return false;

                if (ValidateUserInternal(user, password))
                {
                    user.PasswordQuestion = newPasswordQuestion;
                    user.PasswordAnswer = newPasswordAnswer;

                    uow.Repositories.AddOrUpdate(user);
                    uow.Complete();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"/> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
        /// </returns>
        public override MembershipUser CreateUser(string username, string password, string email,
            string passwordQuestion, string passwordAnswer, bool isApproved,
            object providerUserKey, out MembershipCreateStatus status)
        {
            try
            {
                // Validate the username
                if (!ValidateUserName(username, HiveId.Empty))
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                    return null;
                }

                // Validate the email address
                if (RequiresUniqueEmail && !ValidateEmail(email, HiveId.Empty))
                {
                    status = MembershipCreateStatus.DuplicateEmail;
                    return null;
                }

                // Validate the password
                var e = new ValidatePasswordEventArgs(username, password, true);

                base.OnValidatingPassword(e);

                if (e.Cancel || !ValidatePassword(password))
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }

                using (var uow = _hive.Create())
                {
                    var salt = "";
                    var transformedPassword = TransformPassword(password, ref salt);

                    var user = new User
                    {
                        Name = username,
                        Username = username,
                        Password = transformedPassword,
                        PasswordSalt = salt,
                        Email = email,
                        PasswordQuestion = passwordQuestion,
                        PasswordAnswer = passwordAnswer,
                        IsApproved = isApproved,
                        LastActivityDate = DateTime.UtcNow,
                        LastPasswordChangeDate = DateTime.UtcNow,
                        LastLoginDate = DateTime.UtcNow
                    };

                    uow.Repositories.AddOrUpdate(user);
                    uow.Complete();

                    status = MembershipCreateStatus.Success;

                    return ConvertUserToMembershipUser(user);
                }
            }
            catch (Exception e)
            {
                status = MembershipCreateStatus.ProviderError;
            }

            return null;
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (var uow = _hive.Create())
            {
                var user = GetUmbracoUser(_appContext, uow, username, false);

                if (user == null) return false;

                uow.Repositories.Delete<User>(user.Id);
                uow.Complete();
            }

            return false;
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                                  out int totalRecords)
        {
            //change the where clause depending on if there are wildcard chars specified
            Func<User, bool> whereClause = u =>
            {
                if (emailToMatch.StartsWith("*"))
                {
                    return u.Email.EndsWith(emailToMatch.TrimStart('*'), StringComparison.OrdinalIgnoreCase);
                }
                if (emailToMatch.EndsWith("*"))
                {
                    return u.Email.StartsWith(emailToMatch.TrimEnd('*'), StringComparison.OrdinalIgnoreCase);
                }
                if (emailToMatch.EndsWith("*") && emailToMatch.StartsWith("*"))
                {
                    return u.Email.IndexOf(emailToMatch.TrimEnd('*').TrimStart('*'), StringComparison.InvariantCultureIgnoreCase) > -1;
                }
                return false;
            };

            using (var uow = _hive.Create())
            {
                //TODO: Change this to use the query context when it supports querying by parent (or similar)

                var users = uow.Repositories
                    .GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                    .Where(whereClause)
                    .Skip(pageIndex * pageSize).Take(pageSize).ToList();

                totalRecords = users.Count;
                return ConvertUserListToMembershipCollection(users);
            }
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
                                                                 out int totalRecords)
        {
            //change the where clause depending on if there are wildcard chars specified
            Func<User, bool> whereClause = u =>
                {
                    if (usernameToMatch.StartsWith("*"))
                    {
                        return u.Username.EndsWith(usernameToMatch.TrimStart('*'), StringComparison.OrdinalIgnoreCase);
                    }
                    if (usernameToMatch.EndsWith("*"))
                    {
                        return u.Username.StartsWith(usernameToMatch.TrimEnd('*'), StringComparison.OrdinalIgnoreCase);
                    }
                    if (usernameToMatch.EndsWith("*") && usernameToMatch.StartsWith("*"))
                    {
                        return u.Username.IndexOf(usernameToMatch.TrimEnd('*').TrimStart('*'), StringComparison.InvariantCultureIgnoreCase) > -1;
                    }
                    return false;
                };

            using (var uow = _hive.Create())
            {
                //TODO: Change this to use the query context when it supports querying by parent (or similar)

                var users = uow.Repositories
                    .GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                    .Where(whereClause)
                    .Skip(pageIndex * pageSize).Take(pageSize).ToList();

                totalRecords = users.Count;
                return ConvertUserListToMembershipCollection(users);
            }
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            using (var uow = _hive.Create())
            {
                var users = uow.Repositories
                    .GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                    .Skip(pageIndex * pageSize).Take(pageSize).ToList();

                totalRecords = users.Count;
                return ConvertUserListToMembershipCollection(users);
            }
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            using (var uow = _hive.Create())
            {
                var users = uow.Repositories
                    .GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                    .Where(x => x.LastActivityDate.AddMinutes(Membership.UserIsOnlineTimeWindow) >= DateTime.UtcNow).ToList();

                return users.Count;
            }
        }

        /// <summary>
        ///   Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name = "username">The user to retrieve the password for.</param>
        /// <param name = "answer">The password answer for the user.</param>
        /// <returns>The password for the specified user name.</returns>
        /// <remarks>
        /// </remarks>
        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
                throw new InvalidOperationException("Password retrieval is not enabled!");

            using (var uow = _hive.Create())
            {
                var user = GetUmbracoUser(_appContext, uow, username, false);

                if (user == null) return null;

                if ((RequiresQuestionAndAnswer && answer.Equals(user.PasswordAnswer, StringComparison.OrdinalIgnoreCase)) || !RequiresQuestionAndAnswer)
                {
                    return UnEncodePassword(user.Password);
                }

                throw new MembershipPasswordException();
            }
        }

        /// <summary>
        ///   Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name = "username">The name of the user to get information for.</param>
        /// <param name = "userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>A <see cref = "T:System.Web.Security.MembershipUser" /> object populated with the specified user's information from the data source.</returns>
        /// <remarks>
        /// </remarks>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var hiveUser = GetUmbracoUser(username, userIsOnline);
            return hiveUser != null ? ConvertUserToMembershipUser(hiveUser) : null;
        }

        /// <summary>
        /// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var hiveId = providerUserKey.TryConvertTo<HiveId>();
            if (!hiveId.Success)
            {
                throw new FormatException("The providerUserKey could not be converted to a HiveId");
            }
            var hiveUser = GetUmbracoUser(hiveId.Result, userIsOnline);
            return hiveUser != null ? ConvertUserToMembershipUser(hiveUser) : null;
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        public override string GetUserNameByEmail(string email)
        {
            using (var uow = _hive.Create())
            {
                var user = uow.Repositories
                    .GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                    .Where(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                return user == null ? null : user.Username;
            }
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user.</param>
        /// <returns>
        /// The new password for the specified user.
        /// </returns>
        public override string ResetPassword(string username, string answer)
        {
            using (var uow = _hive.Create())
            {
                var user = GetUmbracoUser(_appContext, uow, username, false);

                if (user == null) return null;

                if (Membership.RequiresQuestionAndAnswer && string.IsNullOrWhiteSpace(answer))
                    throw new InvalidOperationException("Invalid answer entered!");

                if (Membership.RequiresQuestionAndAnswer && !string.IsNullOrWhiteSpace(answer) && !user.PasswordAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Invalid answer entered!");

                // Generate random password
                var newPassword = new byte[16];
                var rng = RandomNumberGenerator.Create();
                rng.GetBytes(newPassword);

                var newPasswordString = Convert.ToBase64String(newPassword);
                var salt = string.Empty;
                user.Password = TransformPassword(newPasswordString, ref salt);
                user.PasswordSalt = salt;

                uow.Repositories.Delete<User>(user.Id);
                uow.Complete();

                return newPasswordString;
            }
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="userName">The membership user whose lock status you want to clear.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        public override bool UnlockUser(string userName)
        {
            // This provider doesn't support locking
            return true;
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="membershipUser">A <see cref="T:System.Web.Security.MembershipUser"/> object that represents the user to update and the updated information for the user.</param>
        public override void UpdateUser(MembershipUser membershipUser)
        {
            using (var uow = _hive.Create())
            {
                var user = GetUmbracoUser(uow, (HiveId)membershipUser.ProviderUserKey, false);

                if (user == null)
                    throw new ProviderException("User does not exist!");

                if (!ValidateUserName(user.Username, user.Id))
                    throw new ArgumentException("UserName is not unique!");

                if (RequiresUniqueEmail && !ValidateEmail(user.Email, user.Id))
                    throw new ArgumentException("Email is not unique!");

                user.Email = membershipUser.Email;
                user.Comments = membershipUser.Comment;
                user.LastActivityDate = membershipUser.LastActivityDate;
                user.LastLoginDate = membershipUser.LastLoginDate;
                user.IsApproved = membershipUser.IsApproved;

                uow.Repositories.AddOrUpdate(user);
                uow.Complete();
            }
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        public override bool ValidateUser(string username, string password)
        {
            using (var uow = _hive.Create())
            {
                var user = GetUmbracoUser(_appContext, uow, username, false);

                if (user == null) return false;

                if (ValidateUserInternal(user, password))
                {
                    user.LastLoginDate = DateTime.UtcNow;
                    user.LastActivityDate = DateTime.UtcNow;

                    uow.Repositories.AddOrUpdate(user);
                    uow.Complete();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(name))
                name = "BackOfficeMembershipProvider";

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Umbraco backoffice membership provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            ApplicationName = GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

            _maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            _minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            _minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            _passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            _enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], bool.TrueString));
            _enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], bool.TrueString));
            _requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], bool.FalseString));
            _requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], bool.TrueString));

            var passwordFormat = config["passwordFormat"] ?? "Hashed";

            switch (passwordFormat)
            {
                case "Hashed":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    _passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }
        }

        #endregion

        #region Umbraco Extensions

        public User GetUmbracoUser(string username, bool userIsOnline)
        {
            using (var uow = _hive.Create())
            {
                return GetUmbracoUser(_appContext, uow, username, userIsOnline);
            }
        }

        internal static User GetUmbracoUser(IUmbracoApplicationContext appContext, IGroupUnit<ISecurityStore> uow, string username, bool userIsOnline)
        {
            // TODO: Enable type of extension method GetEntityByRelationType to be passed all the way to the provider
            // so that it can use the typemappers collection to map back to a User

            // APN: I changed SingleOrDefault to FirstOrDefault to guard against YSODs if somehow a duplicate user gets into the store [31/Jan]
            var userEntity = uow.Repositories
                .GetEntityByRelationType<TypedEntity>(
                    FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                .FirstOrDefault(x => x.Attribute<string>(UserSchema.UsernameAlias) == username);


            if (userEntity == null) return null;

            var user = new User();
            user.SetupFromEntity(userEntity);

            if (userIsOnline)
            {
                user.LastActivityDate = DateTime.UtcNow;

                uow.Repositories.AddOrUpdate(user);
                uow.Complete();
            }

            return user;
        }

        public User GetUmbracoUser(HiveId id, bool userIsOnline)
        {
            using (var uow = _hive.Create())
            {
                return GetUmbracoUser(uow, id, userIsOnline);
            }
        }

        internal static User GetUmbracoUser(IGroupUnit<ISecurityStore> uow, HiveId id, bool userIsOnline)
        {
            var user = uow.Repositories
                .GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                .SingleOrDefault(x => x.Id.Value == id.Value);
            //.SingleOrDefault(x => x.Id.ToString(HiveIdFormatStyle.AsUri) == id.ToString(HiveIdFormatStyle.AsUri));
            //Had to change this comparison to use values because the Uri portion wont work with the membership wrapper provider because the 
            //underlying membership providers dont know anything about a URI, therefore teh wrapper only passes in the value portion of the HiveId. SD. 28/11/2011
            

            if (user == null) return null;

            if (userIsOnline)
            {
                user.LastActivityDate = DateTime.UtcNow;

                uow.Repositories.AddOrUpdate(user);
                uow.Complete();
            }

            return user;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper method to get the config value or default to a set value if null or empty.
        /// </summary>
        /// <param name="configValue">The config value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        private static string GetConfigValue(string configValue, string defaultValue)
        {
            return string.IsNullOrEmpty(configValue) ? defaultValue : configValue;
        }

        /// <summary>
        /// Transforms the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        private string TransformPassword(string password, ref string salt)
        {
            var ret = string.Empty;
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    ret = password;
                    break;
                case MembershipPasswordFormat.Hashed:
                    // Generate the salt if not passed in
                    if (string.IsNullOrEmpty(salt))
                    {
                        var saltBytes = new byte[16];
                        var rng = RandomNumberGenerator.Create();
                        rng.GetBytes(saltBytes);
                        salt = Convert.ToBase64String(saltBytes);
                    }
                    ret = FormsAuthentication.HashPasswordForStoringInConfigFile((salt + password), "SHA1");
                    break;
                case MembershipPasswordFormat.Encrypted:
                    var clearText = Encoding.UTF8.GetBytes(password);
                    var encryptedText = base.EncryptPassword(clearText);
                    ret = Convert.ToBase64String(encryptedText);
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Validates the email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="excludeId">The exclude id.</param>
        /// <returns></returns>
        private bool ValidateEmail(string email, HiveId excludeId)
        {
            if (RequiresUniqueEmail)
            {
                using (var uow = _hive.Create())
                {
                    return !uow.Repositories.GetAll<User>().Any(
                                x => x.Attribute<string>(UserSchema.EmailAlias).InvariantEquals(email) &&
                                (excludeId.IsNullValueOrEmpty() || excludeId != x.Id));
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the username.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="excludeId">The exclude id.</param>
        /// <returns></returns>
        private bool ValidateUserName(string userName, HiveId excludeId)
        {
            if (userName.IsNullOrWhiteSpace()) return false;

            using (var uow = _hive.Create())
            {
                return !uow.Repositories.GetAll<User>().Any(
                    x => x.Attribute<string>(UserSchema.UsernameAlias).InvariantEquals(userName) 
                         && (excludeId.IsNullValueOrEmpty() || excludeId != x.Id));
            }
        }

        /// <summary>
        /// Validates the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private bool ValidatePassword(string password)
        {
            var isValid = true;

            // Validate simple properties
            isValid = (password.Length >= MinRequiredPasswordLength);
            if (!isValid) return false;

            // Validate non-alphanumeric characters
            var regex = new Regex(@"\W");
            isValid = (regex.Matches(password).Count >= MinRequiredNonAlphanumericCharacters);
            if (!isValid) return false;

            // Validate regular expression
            if (!string.IsNullOrEmpty(PasswordStrengthRegularExpression))
            {
                regex = new Regex(PasswordStrengthRegularExpression);
                isValid = (regex.Matches(password).Count > 0);
            }

            return isValid;
        }

        /// <summary>
        /// Uns the encode password.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <returns></returns>
        private string UnEncodePassword(string encodedPassword)
        {
            var password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password = Encoding.UTF8.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        /// <summary>
        /// Validates the user internal.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private bool ValidateUserInternal(User user, string password)
        {
            if (user != null && user.IsApproved)
            {
                var salt = user.PasswordSalt;
                var transformedPassword = TransformPassword(password, ref salt);
                if (string.Compare(transformedPassword, user.Password) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts the user to membership user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="providerUserKey">The provider user key.</param>
        /// <returns></returns>
        private MembershipUser ConvertUserToMembershipUser(User user)
        {
            return new MembershipUser(Name, user.Username, user.Id, user.Email, user.PasswordQuestion,
                                      user.Id.ToString(), user.IsApproved, false, user.UtcCreated.DateTime, user.LastLoginDate.DateTime,
                                      user.LastActivityDate.DateTime, user.LastPasswordChangeDate.DateTime, DateTime.MinValue);
        }

        /// <summary>
        /// Converts the user list to membership collection.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        private MembershipUserCollection ConvertUserListToMembershipCollection(IEnumerable<User> users)
        {
            var returnCollection = new MembershipUserCollection();

            foreach (User user in users)
            {
                returnCollection.Add(ConvertUserToMembershipUser(user));
            }

            return returnCollection;
        }

        #endregion
    }
}