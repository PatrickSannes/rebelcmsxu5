using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Framework.Security
{
    public static class SecurityServiceExtensions
    {
        public static UserGroup GetAnonymousUserGroup(this ISecurityService securityService)
        {
            using (var uow = securityService.Hive.OpenReader<ISecurityStore>())
            {
                return securityService.GetAnonymousUserGroup(uow);
            }
        }

        internal static UserGroup GetAnonymousUserGroup(this ISecurityService securityService, IReadonlyGroupUnit<ISecurityStore> openUnit)
        {
            var group = openUnit.Repositories.Query<UserGroup>().FirstOrDefault(x => x.Name == "Anonymous");
            return group;
        }
    }

    public class SecurityService : ISecurityService
    {
        private readonly IHiveManager _hive;
        private readonly IEnumerable<Lazy<Permission, PermissionMetadata>> _permissions;

        public SecurityService(IHiveManager hive, IEnumerable<Lazy<Permission, PermissionMetadata>> permissions)
        {
            Mandate.That<NullReferenceException>(hive != null);
            Mandate.That<NullReferenceException>(permissions != null);

            _hive = hive;
            _permissions = permissions;
        }

        /// <summary>
        /// Gets the hive.
        /// </summary>
        /// <value>The hive.</value>
        public IHiveManager Hive { get { return _hive; } }

        #region Explicit

        public PermissionResults GetExplicitPermissions(HiveId userId, HiveId entityId = default(HiveId), params Guid[] permissionIds)
        {
            var userGroupIds = GetUserGroupIdsForUser(userId);
            var statuses = GetPermissionStatuses(userGroupIds, entityId, permissionIds);
            return new PermissionResults(statuses, _permissions);
        }

        public PermissionResults GetExplicitPermissions(IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId), params Guid[] permissionIds)
        {
            var statuses = GetPermissionStatuses(userGroupIds, entityId, permissionIds);
            return new PermissionResults(statuses, _permissions);
        }

        /// <summary>
        /// Gets the explicit permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public PermissionResult GetExplicitPermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId))
        {
            using (var readonlyGroupUnit = Hive.OpenReader<ISecurityStore>())
                return GetExplicitPermission(permissionId, userGroupIds, readonlyGroupUnit, entityId);
        }

        public PermissionResult GetExplicitPermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, IReadonlyGroupUnit<ISecurityStore> securityUow, HiveId entityId = default(HiveId))
        {
            Mandate.ParameterNotNull(userGroupIds, "userGroupIds");

            //get/store the result in scoped cache

            var ids = userGroupIds.ToList();
            ids.Sort((id1, id2) => id1.ToString().CompareTo(id2.ToString())); // Sort the list of ids so that the cache key is the same regardless of order of passed in collection

            var key = "explicit-permission-" + (permissionId.ToString("N") + string.Join("", ids.Select(x => x.ToString())) + entityId).ToMd5();

            return Hive.FrameworkContext.ScopedCache.GetOrCreateTyped<PermissionResult>(key, () =>
            {
                // Get the permission reference
                if (!_permissions.Exists(permissionId))
                    throw new InvalidOperationException("Unable to find a Permission with the id '" + permissionId + "'");

                // If value is null, or permission is not an entity permission, just use the system root
                if (entityId.IsNullValueOrEmpty() || _permissions.Single(x => x.Metadata.Id == permissionId).Metadata.Type != FixedPermissionTypes.EntityAction)
                    entityId = FixedHiveIds.SystemRoot;

                HiveId source;
                var status = GetPermissionStatus(permissionId, ids, entityId, out source, securityUow);
                return new PermissionResult(_permissions.Get(permissionId).Value, source, status);
            });
        }

        /// <summary>
        /// Gets the explicit permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public PermissionResult GetExplicitPermission(Guid permissionId, HiveId userId, HiveId entityId = default(HiveId))
        {
            var userGroupIds = GetUserGroupIdsForUser(userId);
            using (var readonlyGroupUnit = Hive.OpenReader<ISecurityStore>())
                return GetExplicitPermission(permissionId, userGroupIds, readonlyGroupUnit, entityId);
        }

        #endregion

        #region Inherited

        public PermissionResults GetInheritedPermissions(HiveId userId, HiveId entityId = default(HiveId), params Guid[] permissionIds)
        {
            var userGroupIds = GetUserGroupIdsForUser(userId);
            return GetInheritedPermissions(userGroupIds, entityId, permissionIds);
        }

        public PermissionResults GetInheritedPermissions(IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId), params Guid[] permissionIds)
        {
            var results = new List<PermissionResult>();

            // Get the permission reference
            if (entityId.IsNullValueOrEmpty())
                entityId = FixedHiveIds.SystemRoot;

            var entityHive = Hive.GetReader<IContentStore>(entityId.ToUri());
            using (var uow = entityHive.CreateReadonly())
            {
                results.AddRange(permissionIds.Select(permissionId => GetInheritedPermission(permissionId, userGroupIds, uow, entityId)));
            }

            return new PermissionResults(results.ToArray());
        }

        /// <summary>
        /// Gets the inherited permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public PermissionResult GetInheritedPermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId))
        {
            // Get the permission reference
            if (entityId.IsNullValueOrEmpty())
                entityId = FixedHiveIds.SystemRoot;

            var entityHive = Hive.GetReader<IContentStore>(entityId.ToUri());
            using (var uow = entityHive.CreateReadonly())
                return GetInheritedPermission(permissionId, userGroupIds, uow, entityId);
        }

        public PermissionResult GetInheritedPermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, IReadonlyGroupUnit<IContentStore> uow, HiveId entityId = default(HiveId))
        {

            Mandate.ParameterNotNull(userGroupIds, "userGroupIds");

            //get/store the result in scoped cache

            var ids = userGroupIds.ToList();
            ids.Sort((id1, id2) => id1.ToString().CompareTo(id2.ToString())); // Sort the list of ids so that the cache key is the same regardless of order of passed in collection

            var key = "inherited-permission-" + (permissionId.ToString("N") + string.Join("", ids.Select(x => x.ToString())) + entityId).ToMd5();

            return Hive.FrameworkContext.ScopedCache.GetOrCreateTyped<PermissionResult>(key, () =>
            {
                // Get the permission reference
                if (!_permissions.Exists(permissionId))
                    throw new InvalidOperationException("Unable to find a Permission with the id '" + permissionId + "'");

                if (entityId.IsNullValueOrEmpty())
                    entityId = FixedHiveIds.SystemRoot;

                // Loop through entities ancestors
                var ancestorsIds = uow.Repositories.GetAncestorRelations(entityId, FixedRelationTypes.DefaultRelationType)
                    .Select(x => x.SourceId).ToArray();

                foreach (var ancestorId in ancestorsIds)
                {
                    HiveId source;
                    var status = GetPermissionStatus(permissionId, ids, ancestorId, out source);

                    // We are not interested in inherit permissions, so if we find one, move on
                    if (status != PermissionStatus.Inherit)
                    {
                        return new PermissionResult(_permissions.Get(permissionId).Value, source, status);
                    }

                    // If the ancester is system root, we've reached then end, in which case return a deny result
                    //BUG: We shouldn't have to compare the system root value but NH is returning content:// for the system root
                    if (ancestorId == FixedHiveIds.SystemRoot || ancestorId.Value == FixedHiveIds.SystemRoot.Value)
                    {
                        return new PermissionResult(_permissions.Get(permissionId).Value, source, PermissionStatus.Deny);
                    }
                }

                // We shouldn't get this far, as the last node in ancestors should always be SystemRoot, but we need to supply a fallback
                return new PermissionResult(_permissions.Get(permissionId).Value, HiveId.Empty, PermissionStatus.Deny);
            });
        }

        /// <summary>
        /// Gets the inherited permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public PermissionResult GetInheritedPermission(Guid permissionId, HiveId userId, HiveId entityId = default(HiveId))
        {
            var userGroupIds = GetUserGroupIdsForUser(userId);

            // Pass through to the user groups overload
            return GetInheritedPermission(permissionId, userGroupIds, entityId);
        }

        #endregion

        #region Effective

        public PermissionResults GetEffectivePermissions(HiveId userId, HiveId entityId = default(HiveId), params Guid[] permissionIds)
        {
            var userGroupIds = GetUserGroupIdsForUser(userId);

            return GetEffectivePermissions(userGroupIds, entityId, permissionIds);
        }

        public PermissionResults GetEffectivePermissions(HiveId userId, IReadonlyGroupUnit<IContentStore> uow, IReadonlyGroupUnit<ISecurityStore> securityUow, HiveId entityId = default(HiveId), params Guid[] permissionIds)
        {
            var userGroupIds = GetUserGroupIdsForUser(userId);

            return GetEffectivePermissions(userGroupIds, uow, securityUow, entityId, permissionIds);
        }

        public PermissionResults GetEffectivePermissions(IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId), params Guid[] permissionIds)
        {
            var entityHive = Hive.GetReader<IContentStore>();
            var securityHive = Hive.GetReader<ISecurityStore>();

            using (var entityUow = entityHive.CreateReadonly())
            using (var securityUow = securityHive.CreateReadonly())
            {
                return GetEffectivePermissions(userGroupIds, entityUow, securityUow, entityId, permissionIds);
            }
        }

        public PermissionResults GetEffectivePermissions(IEnumerable<HiveId> userGroupIds, IReadonlyGroupUnit<IContentStore> uow, IReadonlyGroupUnit<ISecurityStore> securityUow, HiveId entityId = default(HiveId), params Guid[] permissionIds)
        {
            return new PermissionResults(permissionIds.Select(permissionId => GetEffectivePermission(permissionId, userGroupIds, uow, securityUow, entityId)).ToArray());
        }

        /// <summary>
        /// Gets the effective permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public PermissionResult GetEffectivePermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId))
        {
            // Pass through to the user groups overload
            var entityHive = Hive.GetReader<IContentStore>();
            var securityHive = Hive.GetReader<ISecurityStore>();

            using (var entityUow = entityHive.CreateReadonly())
            using (var securityUow = securityHive.CreateReadonly())
                return GetEffectivePermission(permissionId, userGroupIds, entityUow, securityUow, entityId);
        }

        public PermissionResult GetEffectivePermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, IReadonlyGroupUnit<IContentStore> uow, IReadonlyGroupUnit<ISecurityStore> securityUow, HiveId entityId = default(HiveId))
        {
            Mandate.ParameterNotNull(userGroupIds, "userGroupIds");

            var explicitPermission = GetExplicitPermission(permissionId, userGroupIds, securityUow, entityId);

            return explicitPermission.Status != PermissionStatus.Inherit
                       ? explicitPermission
                       : GetInheritedPermission(permissionId, userGroupIds, uow, entityId);
        }

        /// <summary>
        /// Gets the effective permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public PermissionResult GetEffectivePermission(Guid permissionId, HiveId userId, HiveId entityId = default(HiveId))
        {
            var userGroupIds = GetUserGroupIdsForUser(userId);

            // Pass through to the user groups overload
            var entityHive = Hive.GetReader<IContentStore>();
            var securityHive = Hive.GetReader<ISecurityStore>();

            using (var entityUow = entityHive.CreateReadonly())
            using (var securityUow = securityHive.CreateReadonly())
                return GetEffectivePermission(permissionId, userGroupIds, entityUow, securityUow, entityId);
        }

        #endregion

        #region Helper Methods

        public IEnumerable<PermissionStatusResult> GetPermissionStatuses(IEnumerable<HiveId> userGroupIds, HiveId entityId, params Guid[] permissionIds)
        {
            var entityHive = Hive.GetReader<ISecurityStore>(new Uri("security://user-groups"));
            using (var uow = entityHive.CreateReadonly())
            {
                foreach (var permissionId in permissionIds)
                {
                    // Get the permission reference
                    if (!_permissions.Exists(permissionId))
                        throw new InvalidOperationException("Unable to find a Permission with the id '" + permissionId + "'");

                    // If value is null, or permission is not an entity permission, just use the system root
                    if (entityId.IsNullValueOrEmpty() || _permissions.Single(x => x.Metadata.Id == permissionId).Metadata.Type != FixedPermissionTypes.EntityAction)
                        entityId = FixedHiveIds.SystemRoot;

                    var source = HiveId.Empty;
                    var result = GetPermissionStatus(permissionId, userGroupIds, entityId, out source, uow);
                    yield return new PermissionStatusResult(source, result, permissionId);
                }
            }
        }

        /// <summary>
        /// Gets the permission status.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        protected PermissionStatus GetPermissionStatus(Guid permissionId, IEnumerable<HiveId> userGroupIds, HiveId entityId, out HiveId source)
        {
            var entityHive = Hive.GetReader<ISecurityStore>(new Uri("security://user-groups"));
            using (var uow = entityHive.CreateReadonly())
            {
                return GetPermissionStatus(permissionId, userGroupIds, entityId, out source, uow);
            }
        }

        /// <summary>
        /// Gets the permission status using an existing, open unit of work.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="source">The source.</param>
        /// <param name="uow">The unit of work.</param>
        /// <returns></returns>
        protected PermissionStatus GetPermissionStatus(Guid permissionId, IEnumerable<HiveId> userGroupIds, HiveId entityId, out HiveId source, IReadonlyGroupUnit<ISecurityStore> uow)
        {
            // Check for Administrator group membership, and just return true if so
            var adminUserGroup = Hive.FrameworkContext.ScopedCache.GetOrCreateTyped("ss_administrator-group",
                () => uow.Repositories.Query<UserGroup>().FirstOrDefault(x => x.Name == "Administrator"));

            //TODO: Had to change to only compare values, as for some reason, the ProviderGroupRoots are different
            var enumeratedGroupIds = userGroupIds.ToArray();
            if (adminUserGroup != null && enumeratedGroupIds.Any(x => x.Value == adminUserGroup.Id.Value))
            {
                source = HiveId.Empty;
                return PermissionStatus.Allow;
            }

            // Get permission relations for current ancestor
            var parentRelationsKey = "ss_parent-relations-" + entityId + string.Join("|", enumeratedGroupIds.OrderBy(x => x.ToString())).ToMd5();
            var relations = Hive.FrameworkContext.ScopedCache.GetOrCreateTyped(parentRelationsKey,
                () =>
                    {
                        var relationByIds = uow.Repositories.GetParentRelations(entityId, FixedRelationTypes.PermissionRelationType).ToArray();
                        // Comparing by value as schemas are not coming back out of Hive for some reason
                        return relationByIds.Where(x => enumeratedGroupIds.Any(y => y.Value == x.SourceId.Value)).ToArray();
                    });

            // Check for an EXPLICIT Deny permission
            // Deny comes first, as if multiple permission definitions at same level have conflicting status, Deny trumps Allow
            var denyRelation = relations.FirstOrDefault(relation => relation.MetaData.Any(x => x.Key.ToLower() == permissionId.ToString().ToLower() && x.Value == PermissionStatus.Deny.ToString()));
            if (denyRelation != null)
            {
                source = denyRelation.DestinationId;
                return PermissionStatus.Deny;
            }

            // Check for an EXPLICIT Allow permission
            var allowRelation = relations.FirstOrDefault(relation => relation.MetaData.Any(x => x.Key.ToLower() == permissionId.ToString().ToLower() && x.Value == PermissionStatus.Allow.ToString()));
            if (allowRelation != null)
            {
                source = allowRelation.DestinationId;
                return PermissionStatus.Allow;
            }

            // Check for an EXPLICIT Inherit permission
            var inheritRelation = relations.FirstOrDefault(relation => relation.MetaData.Any(x => x.Key.ToLower() == permissionId.ToString().ToLower() && x.Value == PermissionStatus.Inherit.ToString()));
            if (inheritRelation != null)
            {
                source = inheritRelation.DestinationId;
                return PermissionStatus.Inherit;
            }

            source = HiveId.Empty;
            return PermissionStatus.Inherit;
        }

        /// <summary>
        /// Gets a list of UserGroup ids that the specified User belongs to.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        protected IEnumerable<HiveId> GetUserGroupIdsForUser(HiveId userId)
        {
            return Hive.FrameworkContext.ScopedCache.GetOrCreateTyped("ss_GetUserGroupIdsForUser_" + userId,
                () =>
                {
                    var hive = Hive.GetReader<ISecurityStore>(new Uri("security://user-groups"));
                    using (var uow = hive.CreateReadonly())
                    {
                        if (!userId.IsNullValueOrEmpty())
                        {
                            // Valid hive id, so go find any related user groups
                            return uow.Repositories.GetParentRelations(userId, FixedRelationTypes.UserGroupRelationType)
                                .Select(x => x.SourceId)
                                .ToList();
                        }

                        // Empty hive id, so assume anonymous user
                        var id = uow.Repositories.QueryContext.Query<UserGroup>()
                            .Where(x => x.Name == "Anonymous")
                            .ToList() // Currently have to call ToList before select, as QueryContext can't handly the type change right now (2011/10/28)
                            .Select(x => x.Id);

                        return id;
                    }
                });
        }

        #endregion
    }

    public class PermissionStatusResult
    {
        public PermissionStatusResult(HiveId sourceId, PermissionStatus status, Guid permissionId)
        {
            SourceId = sourceId;
            Status = status;
            PermissionId = permissionId;
        }

        public PermissionStatus Status { get; protected set; }
        public HiveId SourceId { get; protected set; }
        public Guid PermissionId { get; protected set; }
    }
}
