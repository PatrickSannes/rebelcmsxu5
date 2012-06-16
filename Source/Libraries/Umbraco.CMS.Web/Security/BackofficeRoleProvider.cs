using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Security
{
    public class BackOfficeRoleProvider : RoleProvider
    {
        private readonly Lazy<IHiveManager> _hiveManager;
        private readonly Lazy<GroupUnitFactory<ISecurityStore>> _userHive;
        private readonly Lazy<GroupUnitFactory<ISecurityStore>> _userGroupHive;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeRoleProvider"/> class.
        /// </summary>
        public BackOfficeRoleProvider()
            : this (null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeRoleProvider"/> class.
        /// </summary>
        /// <param name="hiveManager">The hive manager.</param>
        public BackOfficeRoleProvider(Lazy<IHiveManager> hiveManager)
        {
            // Since this service is instantiated by ASP.NET using a parameterless constructor,
            // we need to use the dependencyresolver here. However, to avoid throwing exceptions
            // from the constructor itself in the event of a bad resolution attempt, we'll 
            // resolve it on first access by using Lazy.

            _hiveManager = hiveManager ?? new Lazy<IHiveManager>(
                    () => DependencyResolver.Current.GetService<IUmbracoApplicationContext>().Hive);

            _userHive =
                new Lazy<GroupUnitFactory<ISecurityStore>>(
                    () => Hive.GetWriter<ISecurityStore>(new Uri("security://users")));

            _userGroupHive =
                new Lazy<GroupUnitFactory<ISecurityStore>>(
                    () => Hive.GetWriter<ISecurityStore>(new Uri("security://user-groups")));
        }

        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <returns>The name of the application to store and retrieve role information for.</returns>
        public override string ApplicationName { get; set; }

        /// <summary>
        /// Gets the user hive.
        /// </summary>
        /// <value>The user hive.</value>
        public GroupUnitFactory<ISecurityStore> UserHive
        {
            get { return _userHive.Value; }
        }

        /// <summary>
        /// Gets the user group hive.
        /// </summary>
        /// <value>The user group hive.</value>
        public GroupUnitFactory<ISecurityStore> UserGroupHive
        {
            get { return _userGroupHive.Value; }
        }

        /// <summary>
        /// Gets the app context.
        /// </summary>
        /// <value>The app context.</value>
        public IHiveManager Hive
        {
            get { return _hiveManager.Value; }
        }

        /// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if(usernames.Length == 0)
                return;

            if(roleNames.Length == 0)
                return;

            using (var uow1 = UserHive.Create())
            using (var uow2 = UserGroupHive.Create())
            {
                foreach (var username in usernames)
                {
                    var user = uow1.Repositories
                        .GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                        .SingleOrDefault(x => x.Username == username);

                    var userGroups = GetUserGroupsByName(roleNames, uow2);

                    if (user == null || userGroups.Count() == 0) continue;

                    // Add any new user group
                    foreach (var userGroup in userGroups)
                    {
                        uow2.Repositories.ChangeOrCreateRelationMetadata(userGroup.Id, user.Id, FixedRelationTypes.UserGroupRelationType);
                    }

                    uow2.Repositories.AddOrUpdate(user);
                }

                uow1.Complete();

                AddRolesToCurrentIdentity(roleNames, usernames);
            }
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            if (RoleExists(roleName))
            {
                throw new ProviderException("Role name already exists.");
            }

            using (var uow1 = UserGroupHive.Create())
            {
                var role = new UserGroup()
                {
                    Name = roleName
                };

                uow1.Repositories.AddOrUpdate(role);
                uow1.Complete();
            }
        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">If true, throw an exception if <paramref name="roleName"/> has one or more members and do not delete <paramref name="roleName"/>.</param>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!RoleExists(roleName))
            {
                throw new ProviderException("Role name already exists.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(roleName).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            try
            {
                // Remove users from role
                var users = GetUsersInRole(roleName);
                RemoveUsersFromRoles(users, new[]{ roleName });

                // Delete the role
                using (var uow1 = UserGroupHive.Create())
                {
                    var userGroup = GetUserGroupByName(roleName, uow1);
                    if (userGroup != null)
                    {
                        uow1.Repositories.Delete<UserGroup>(userGroup.Id);
                        uow1.Complete();
                    }
                }

                //remove the role from the current identity
                RemoveRolesFromCurrentIdentity(new[] {roleName});

                return true;
            } 
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches <paramref name="usernameToMatch"/> and the user is a member of the specified role.
        /// </returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var uow1 = UserGroupHive.Create())
            {
                var userGroup = GetUserGroupByName(roleName, uow1);

                var usernames = uow1.Repositories.GetLazyChildRelations(userGroup.Id, FixedRelationTypes.UserGroupRelationType)
                    .Where(x => ((TypedEntity)x.Destination).Attribute<string>(UserSchema.UsernameAlias).StartsWith(usernameToMatch))
                    .Select(x => ((TypedEntity)x.Destination).Attribute<string>(UserSchema.UsernameAlias))
                    .ToArray();

                return usernames;
            }
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {
            using (var uow1 = UserGroupHive.Create())
            {
                return uow1.Repositories
                    .GetEntityByRelationType<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                    .Select(x => x.Name).ToArray();
            }
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            //before we go looking in the db, we need to check the current identity
            if (Thread.CurrentPrincipal.Identity is UmbracoBackOfficeIdentity)
            {
                var identity = (UmbracoBackOfficeIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.Name.Trim() == username.Trim())
                {
                    return identity.Roles;
                }                
            }

            using (var uow1 = UserHive.Create())
            using (var uow2 = UserGroupHive.Create())
            {
                var user = uow1.Repositories.GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                    .SingleOrDefault(x => x.Username == username);

                var userGroups = user != null ? uow2.Repositories.GetLazyParentRelations(user.Id, FixedRelationTypes.UserGroupRelationType)
                    .Select(x => ((TypedEntity)x.Source).Attribute<string>(NodeNameAttributeDefinition.AliasValue))
                    .ToArray() : new string[0];

                return userGroups;
            }
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            using (var uow1 = UserGroupHive.Create())
            {
                var userGroup = GetUserGroupByName(roleName, uow1);

                return uow1.Repositories.GetLazyChildRelations(userGroup.Id, FixedRelationTypes.UserGroupRelationType)
                    .Select(x => ((TypedEntity)x.Destination).Attribute<string>(UserSchema.UsernameAlias))
                    .ToArray();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            //before we go looking in the db, we need to check the current identity
            if (Thread.CurrentPrincipal.Identity is UmbracoBackOfficeIdentity)
            {
                return ((UmbracoBackOfficeIdentity)Thread.CurrentPrincipal.Identity).Roles.Contains(roleName);
            }

            using (var uow1 = UserGroupHive.Create())
            {
                var userGroup = GetUserGroupByName(roleName, uow1);

                return uow1.Repositories.GetLazyChildRelations(userGroup.Id, FixedRelationTypes.UserGroupRelationType)
                    .Any(x => ((TypedEntity)x.Destination).Attribute<string>(UserSchema.UsernameAlias) == username);
            }
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (roleNames.Any(rolename => !RoleExists(rolename)))
            {
                throw new ProviderException("Role name not found.");
            }

            if (usernames.Any(username => roleNames.Any(rolename => !IsUserInRole(username, rolename))))
            {
                throw new ProviderException("User is not in role.");
            }

            using (var uow2 = UserGroupHive.Create())
            {
                var userGroups = GetUserGroupsByName(roleNames, uow2);

                foreach (var userGroup in userGroups)
                {
                    var relations = uow2.Repositories.GetLazyChildRelations(userGroup.Id, FixedRelationTypes.UserGroupRelationType)
                        .Where(x => usernames.Contains(((TypedEntity)x.Destination).Attribute<string>(UserSchema.UsernameAlias), StringComparer.InvariantCultureIgnoreCase));

                    foreach (var relation in relations)
                    {
                        uow2.Repositories.RemoveRelation(relation);
                    }
                }

                uow2.Complete();

                //remove the role from the current identity
                RemoveRolesFromCurrentIdentity(usernames, roleNames);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            using (var uow1 = UserGroupHive.Create())
            {
                return uow1.Repositories.GetEntityByRelationType<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                    .Any(x => x.Name.ToLower() == roleName.ToLower());
            }
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
                name = "BackOfficeRoleProvider";

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Umbraco backoffice role provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                ApplicationName = HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                ApplicationName = config["applicationName"];
            }
        }

        /// <summary>
        /// This checks the current identity is a UmbracoBackOfficeIdentity, if so , it updates its roles and re-sets the cookie
        /// if we are in an HttpContext.
        /// </summary>
        /// <param name="roleNames">The role names.</param>
        /// <param name="userNames">The user names.</param>
        private static void RemoveRolesFromCurrentIdentity(IEnumerable<string> roleNames, IEnumerable<string> userNames = null)
        {
            //remove the current role from the user data
            if (!(Thread.CurrentPrincipal.Identity is UmbracoBackOfficeIdentity)) return;
            var identity = (UmbracoBackOfficeIdentity)Thread.CurrentPrincipal.Identity;
            if (userNames == null || userNames.Contains(identity.Name))
            {
                identity.Roles = identity.Roles.Where(x => roleNames.Contains(x)).ToArray();
                //now we need to reset the cookie
                if (HttpContext.Current != null)
                {
                    var wrapper = new HttpContextWrapper(HttpContext.Current);
                    wrapper.CreateUmbracoAuthTicket(new UserData
                        {
                            AllowedApplications = identity.AllowedApplications,
                            Username = identity.Name,
                            RealName = identity.RealName,
                            Roles = identity.Roles,
                            SessionTimeout = identity.SessionTimeout,
                            StartContentNode = identity.StartContentNode.ToString(),
                            StartMediaNode = identity.StartMediaNode.ToString()
                        });
                }    
            }
        }

        /// <summary>
        /// This checks the current identity is a UmbracoBackOfficeIdentity, if so, it updates its roles and re-sets the cookie
        /// if we are in an HttpContext.
        /// </summary>
        /// <param name="roleNames">The role names.</param>
        /// <param name="userNames">The user names.</param>
        private static void AddRolesToCurrentIdentity(IEnumerable<string> roleNames, IEnumerable<string> userNames = null)
        {
            //remove the current role from the user data
            if (Thread.CurrentPrincipal.Identity is UmbracoBackOfficeIdentity)
            {
                var identity = (UmbracoBackOfficeIdentity)Thread.CurrentPrincipal.Identity;
                if (userNames == null || userNames.Contains(identity.Name))
                {
                    identity.Roles = identity.Roles.Union(roleNames).ToArray();
                    //now we need to reset the cookie))
                    if (HttpContext.Current != null)
                    {
                        var wrapper = new HttpContextWrapper(HttpContext.Current);
                        wrapper.CreateUmbracoAuthTicket(new UserData
                        {
                            AllowedApplications = identity.AllowedApplications,
                            Username = identity.Name,
                            RealName = identity.RealName,
                            Roles = identity.Roles,
                            SessionTimeout = identity.SessionTimeout,
                            StartContentNode = identity.StartContentNode.ToString(),
                            StartMediaNode = identity.StartMediaNode.ToString()
                        });
                    }
                }
            }
        }

        #region Umbraco Extensions

        public UserGroup GetUserGroupByName(string name)
        {
            using (var uow = UserGroupHive.Create())
            {
                return GetUserGroupByName(name, uow);
            }
        }

        private UserGroup GetUserGroupByName(string name, IGroupUnit<ISecurityStore> uow)
        {
            return uow.Repositories.GetEntityByRelationType<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                    .SingleOrDefault(x => x.Name.ToLower() == name.ToLower());
        }

        public IEnumerable<UserGroup> GetUserGroupsByName(string[] names)
        {
            using (var uow = UserGroupHive.Create())
            {
                return GetUserGroupsByName(names, uow);
            }
        }

        private IEnumerable<UserGroup> GetUserGroupsByName(string[] names, IGroupUnit<ISecurityStore> uow)
        {
            return uow.Repositories.GetEntityByRelationType<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                .Where(x => names.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase))
                .ToArray();
        }

        #endregion
    }
}