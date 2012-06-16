using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Framework.Security
{
    using Umbraco.Framework.Persistence.Model.Constants.Entities;

    public static class PermissionExtensions
    {
        /// <summary>
        /// Filters the node set to nodes that are allowed for the user/permission combination
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities">The entities.</param>
        /// <param name="permissionId">The permission id.</param>
        /// <param name="securityService">The security service.</param>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public static IEnumerable<T> FilterWithPermissions<T>(this IEnumerable<T> entities, Guid permissionId, ISecurityService securityService, HiveId userId)
            where T: IRelatableEntity
        {
            return entities.Where(x => securityService.GetEffectivePermission(permissionId, userId, x.Id).IsAllowed());
        }

        /// <summary>
        /// Filters a sequence of <see cref="HiveId"/> where user <paramref name="userId"/> has permission <paramref name="permissionIds"/> by checking with <paramref name="securityService"/>.
        /// </summary>
        /// <param name="entityIds">The entity ids.</param>
        /// <param name="securityService">The security service.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="entityUow">The entity uow.</param>
        /// <param name="securityUow">The security uow.</param>
        /// <param name="permissionIds">The permission ids.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> FilterWithPermissions(this IEnumerable<HiveId> entityIds, 
            ISecurityService securityService, 
            HiveId userId, 
            IReadonlyGroupUnit<IContentStore> entityUow,
            IReadonlyGroupUnit<ISecurityStore> securityUow,
            params Guid[] permissionIds)
        {
            return entityIds.Where(id => securityService.GetEffectivePermissions(userId, entityUow, securityUow, id, permissionIds).AreAllAllowed());
        }

        /// <summary>
        /// Filters a sequence of <see cref="HiveId"/> where the Anonymous <see cref="UserGroup"/> has permissions <paramref name="permissionIds"/> by checking with <paramref name="securityService"/>.
        /// </summary>
        /// <param name="entityIds">The entity ids.</param>
        /// <param name="securityService">The security service.</param>
        /// <param name="entityUow">The entity uow.</param>
        /// <param name="securityUow">The security uow.</param>
        /// <param name="permissionIds">The permission ids.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> FilterAnonymousWithPermissions(this IEnumerable<HiveId> entityIds,
            ISecurityService securityService,
            IReadonlyGroupUnit<IContentStore> entityUow,
            IReadonlyGroupUnit<ISecurityStore> securityUow,
            Guid firstPermission,
            params Guid[] permissionIds)
        {
            var anonymousUserGroup = securityService.GetAnonymousUserGroup(securityUow);
            if (anonymousUserGroup == null || firstPermission == Guid.Empty) return Enumerable.Empty<HiveId>();
            return entityIds.Where(id => securityService.GetEffectivePermissions(anonymousUserGroup.Id.AsEnumerableOfOne(), entityUow, securityUow, id, firstPermission.AsEnumerableOfOne().Concat(permissionIds).ToArray()).AreAllAllowed());
        } 

        /// <summary>
        /// Checks whether specified permission exists.
        /// </summary>
        /// <param name="permissions">The permissions.</param>
        /// <param name="permissionId">The permission id.</param>
        /// <returns></returns>
        public static bool Exists(this IEnumerable<Lazy<Permission, PermissionMetadata>> permissions, Guid permissionId)
        {
            return permissions.Any(x => x.Metadata.Id == permissionId);
        }

        /// <summary>
        /// Gets the specified permissions.
        /// </summary>
        /// <param name="permissions">The permissions.</param>
        /// <param name="permissionId">The permission id.</param>
        /// <returns></returns>
        public static Lazy<Permission, PermissionMetadata> Get(this IEnumerable<Lazy<Permission, PermissionMetadata>> permissions, Guid permissionId)
        {
            return permissions.SingleOrDefault(x => x.Metadata.Id == permissionId);
        }
    }
}
