using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Security;
using Artem.Web.Security;
using Artem.Web.Security.Store;
using Umbraco.Framework.Testing;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Membership
{
    /// <summary>
    /// Used to have a test membership provider for unit tests
    /// </summary>
    /// <remarks>
    /// This will NOT work in medium trust as we are using reflection to set an private field on the base class so it works in unit tests.
    /// </remarks>
    public class CustomXmlMembershipProvider : XmlMembershipProvider
    {

        public void Reset( )
        {
            this.Store.Users.Clear();
            this.Store.Save();
        }

        /// <summary>
        /// Sets the private _file field on the base class so taht it works in unit tests.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(name))
                name = "XmlMembershipProvider";
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "XML Membership Provider");
            }
            base.Initialize(name, config);

            //need to use reflection to set the _file private property :( but don't want to write an in memory membership provider and can't find one
            var current = new DirectoryInfo(Common.CurrentAssemblyDirectory);
            while (current.Parent.GetDirectories("App_Data").SingleOrDefault() == null)
            {
                current = current.Parent;
            }
            var appData = current.Parent.GetDirectories("App_Data").Single();
            var fileField = typeof(XmlMembershipProvider).GetField("_file", BindingFlags.NonPublic | BindingFlags.Instance);
            fileField.SetValue(this, Path.Combine(appData.FullName, "Users.xml"));
        }

        /// <summary>
        /// Need to override because the underlying class doesn't allow us to specify an id for member, always generates one which doesn't work for our unit tests.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="isApproved"></param>
        /// <param name="providerUserKey"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out System.Web.Security.MembershipCreateStatus status)
        {
            if (username == null)
                throw new ArgumentNullException("username");
            try
            {
                if (!this.VerifyUserIsValid(providerUserKey, username, password, email, passwordQuestion, passwordAnswer, out status))
                    return null;
                var dateTime = this.UseUniversalTime ? DateTime.UtcNow : DateTime.Now;
                var salt = string.Empty;
                var str = this.EncodePassword(password, ref salt);
                var user = new XmlUser()
                {
                    //NOTE: this is the fix, have lodged a bug here: http://tinyproviders.codeplex.com/workitem/3
                    UserKey = providerUserKey == null ? Guid.NewGuid() : (Guid)providerUserKey,

                    UserName = username,
                    PasswordSalt = salt,
                    Password = str,
                    Email = email,
                    PasswordQuestion = passwordQuestion,
                    PasswordAnswer = passwordAnswer,
                    IsApproved = isApproved,
                    CreationDate = dateTime,
                    LastActivityDate = dateTime,
                    LastPasswordChangeDate = dateTime
                };
                lock (SyncRoot)
                {
                    this.Store.Users.Add(user);
                    this.Store.Save();
                }
                return this.CreateMembershipFromInternalUser(user);
            }
            catch
            {
                throw;
            }
        }

        public override System.Web.Security.MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return base.GetUser(providerUserKey, userIsOnline);
        }

        private MembershipUser CreateMembershipFromInternalUser(XmlUser user)
        {
            if (user == null)
                return (MembershipUser)null;
            else
                return new MembershipUser(this.Name, user.UserName, (object)user.UserKey, user.Email, user.PasswordQuestion, user.Comment, user.IsApproved, user.IsLockedOut, user.CreationDate, user.LastLoginDate, user.LastActivityDate, user.LastPasswordChangeDate, user.LastLockoutDate);
        }

    }
}