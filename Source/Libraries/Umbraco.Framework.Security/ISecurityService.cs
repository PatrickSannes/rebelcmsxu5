using System;
using System.Collections.Generic;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Framework.Security
{
    using Umbraco.Hive;

    public interface ISecurityService
    {
        /// <summary>
        /// Gets the explicit permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        PermissionResult GetExplicitPermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId));

        /// <summary>
        /// Gets the explicit permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        PermissionResult GetExplicitPermission(Guid permissionId, HiveId userId, HiveId entityId = default(HiveId));

        /// <summary>
        /// Gets the inherited permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        PermissionResult GetInheritedPermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId));

        /// <summary>
        /// Gets the inherited permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        PermissionResult GetInheritedPermission(Guid permissionId, HiveId userId, HiveId entityId = default(HiveId));

        /// <summary>
        /// Gets the effective permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        PermissionResult GetEffectivePermission(Guid permissionId, IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId));

        /// <summary>
        /// Gets the effective permission.
        /// </summary>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        PermissionResult GetEffectivePermission(Guid permissionId, HiveId userId, HiveId entityId = default(HiveId));

        PermissionResults GetEffectivePermissions(HiveId userId, HiveId entityId = default(HiveId), params Guid[] permissionIds);
        PermissionResults GetEffectivePermissions(IEnumerable<HiveId> userGroupIds, HiveId entityId = default(HiveId), params Guid[] permissionIds);
        PermissionResults GetEffectivePermissions(IEnumerable<HiveId> userGroupIds, IReadonlyGroupUnit<IContentStore> uow, IReadonlyGroupUnit<ISecurityStore> securityUow, HiveId entityId = default(HiveId), params Guid[] permissionIds);
        PermissionResults GetEffectivePermissions(HiveId userId, IReadonlyGroupUnit<IContentStore> uow, IReadonlyGroupUnit<ISecurityStore> securityUow, HiveId entityId = default(HiveId), params Guid[] permissionIds);

        /// <summary>
        /// Gets the hive.
        /// </summary>
        /// <value>The hive.</value>
        IHiveManager Hive { get; }
    }
}