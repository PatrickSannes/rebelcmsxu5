using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.Cms.Security
{
    [TestFixture]
    public class SecurityServiceTests
    {
        private IHiveManager _hive;

        private IReadonlyEntityRepositoryGroup<IContentStore> _readonlyContentStoreSession;
        private IReadonlySchemaRepositoryGroup<IContentStore> _readonlyContentStoreSchemaSession;
        private IEntityRepositoryGroup<IContentStore> _contentStoreRepository;
        private ISchemaRepositoryGroup<IContentStore> _contentStoreSchemaRepository;

        private IReadonlyEntityRepositoryGroup<ISecurityStore> _readonlySecurityStoreSession;
        private IReadonlySchemaRepositoryGroup<ISecurityStore> _readonlySecurityStoreSchemaSession;
        private IEntityRepositoryGroup<ISecurityStore> _securityStoreRepository;
        private ISchemaRepositoryGroup<ISecurityStore> _securityStoreSchemaRepository;

        private IEnumerable<Lazy<Permission, PermissionMetadata>> _permissions;
        private TypedEntity _systemRootNode;
        private TypedEntity _childContentNode;
        private HiveId _userId;
        private IEnumerable<UserGroup> _userGroups;

        [Test]
        public void SecurityServiceTests_UserGroup_HasPermission_Success()
        {
            //Arrange
            var permissionsService = new SecurityService(_hive, _permissions);
            var userGroupIds = _userGroups.Select(x => x.Id).ToArray();

            //Assert

            // Both permissions set to allow, no node inheritance
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Copy), userGroupIds).IsAllowed());

            // One permission set to allow, one deny, no node inheritance
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Save), userGroupIds).IsAllowed());
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Publish), userGroupIds).IsAllowed());

            // One permission set to inherit, one set to allow, no node inheritance
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Hostnames), userGroupIds).IsAllowed());

            // Both permissions set to inherit, no node inheritance
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Move), userGroupIds).IsAllowed());

            // Both permissions set to allow, no node inheritance
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Copy), userGroupIds, _childContentNode.Id).IsAllowed());

            // One permission set to allow, one deny, node inheritance
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Save), userGroupIds, _childContentNode.Id).IsAllowed());
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Publish), userGroupIds, _childContentNode.Id).IsAllowed()); // In this instance, becuase the deny permission is inherited, the allow permission takes precedence

            // One permission set to inherit, one set to allow, node inheritance
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Hostnames), userGroupIds, _childContentNode.Id).IsAllowed());

            // Both permissions set to inherit, node inheritance
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Move), userGroupIds, _childContentNode.Id).IsAllowed());
        }

        [Test]
        public void SecurityServiceTests_User_HasPermission_Success()
        {
            //Arrange
            var permissionsService = new SecurityService(_hive, _permissions);

            //Assert

            // Both permissions set to allow, no node inheritance
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Copy), _userId).IsAllowed());

            // One permission set to allow, one deny, no node inheritance
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Save), _userId).IsAllowed());
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Publish), _userId).IsAllowed());

            // One permission set to inherit, one set to allow, no node inheritance
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Hostnames), _userId).IsAllowed());

            // Both permissions set to inherit, no node inheritance
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Move), _userId).IsAllowed());

            // Both permissions set to allow, no node inheritance
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Copy), _userId, _childContentNode.Id).IsAllowed());

            // One permission set to allow, one deny, node inheritance
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Save), _userId, _childContentNode.Id).IsAllowed());
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Publish), _userId, _childContentNode.Id).IsAllowed()); // In this instance, becuase the deny permission is inherited, the allow permission takes precedence

            // One permission set to inherit, one set to allow, node inheritance
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Hostnames), _userId, _childContentNode.Id).IsAllowed());

            // Both permissions set to inherit, node inheritance
            Assert.IsFalse(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.Move), _userId, _childContentNode.Id).IsAllowed());
        }

        [Test]
        public void SecurityServiceTests_Non_Entity_Actions_Dont_Check_Id()
        {
            //Arrange
            var permissionsService = new SecurityService(_hive, _permissions);

            //Assert

            // Child content node has a BackOffice permission defined and set to Deny where system root has one set to Allow
            // because BackOfficeAccess permission is not an entity action, it should go straight to checking the system root
            // so should come back as allow.
            Assert.IsTrue(permissionsService.GetEffectivePermission(new Guid(FixedPermissionIds.BackOfficeAccess), _userId, _childContentNode.Id).IsAllowed());
        }

        [SetUp]
        public void Initialize()
        {
            //Seup permissions
            _permissions = new Permission[] { new SavePermission(), new PublishPermission(), new HostnamesPermission(), 
                new CopyPermission(), new MovePermission(), new BackOfficeAccessPermission() }
                .Select(x => new Lazy<Permission, PermissionMetadata>(() => x, new PermissionMetadata(new Dictionary<string, object>
                    {
                        {"Id", x.Id},
                        {"Name", x.Name},
                        {"Type", x.Type}
                    })));

            //Setup user groups
            _userGroups = new List<UserGroup>
            {
                new UserGroup
                {
                    Id = new HiveId("9DB63B97-4C8F-489C-98C7-017DC6AD869A"),
                    Name = "Administrator"
                },
                new UserGroup
                {
                    Id = new HiveId("F61E4B62-513B-4858-91BF-D873401AD3CA"),
                    Name = "Editor"
                },
                new UserGroup
                {
                    Id = new HiveId("8B65EF91-D49B-43A3-AFD9-6A47D5341B7D"),
                    Name = "Writter"
                }
            };

            // Setup user id
            _userId = new HiveId("857BD0F6-49DC-4378-84C1-6CD8AE14301D");

            //Setup content nodes
            _systemRootNode = new TypedEntity { Id = FixedHiveIds.SystemRoot };
            _childContentNode = new TypedEntity { Id = new HiveId("00E55027-402F-41CF-9052-B8D8F3DBCD76") };

            //Setup relations
            var systemRootPermissionRelations = new List<RelationById>
                {
                    new RelationById(
                        _userGroups.First().Id,
                        _systemRootNode.Id,
                        FixedRelationTypes.PermissionRelationType,
                        0,
                        new RelationMetaDatum(FixedPermissionIds.BackOfficeAccess, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Copy, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Save, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Publish, PermissionStatus.Deny.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Hostnames, PermissionStatus.Inherit.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Move, PermissionStatus.Inherit.ToString())
                        ),
                    new RelationById(
                        _userGroups.Last().Id,
                        _systemRootNode.Id,
                        FixedRelationTypes.PermissionRelationType,
                        0,
                        new RelationMetaDatum(FixedPermissionIds.Copy, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Save, PermissionStatus.Deny.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Publish, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Hostnames, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Move, PermissionStatus.Inherit.ToString())
                        )
                };

            var childContentNodePermissionRelations = new List<RelationById>
                {
                    new RelationById(
                        _userGroups.Skip(1).First().Id,
                        _childContentNode.Id,
                        FixedRelationTypes.PermissionRelationType,
                        0,
                        new RelationMetaDatum(FixedPermissionIds.BackOfficeAccess, PermissionStatus.Deny.ToString()), // Back office access is not an entity action so this should get ignored
                        new RelationMetaDatum(FixedPermissionIds.Copy, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Save, PermissionStatus.Deny.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Publish, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Hostnames, PermissionStatus.Allow.ToString()),
                        new RelationMetaDatum(FixedPermissionIds.Move, PermissionStatus.Inherit.ToString())
                        )
                };

            var childContentNodeAncestorRelations = new List<RelationById>
            {
                new RelationById( 
                    _systemRootNode.Id,
                    _childContentNode.Id,
                    FixedRelationTypes.DefaultRelationType,
                    0)
            };

            var userGroupRelations = new List<RelationById>
            {
                new RelationById( 
                    _userGroups.First().Id,
                    _userId,
                    FixedRelationTypes.UserGroupRelationType,
                    0),
                new RelationById( 
                    _userGroups.Skip(1).First().Id,
                    _userId,
                    FixedRelationTypes.UserGroupRelationType,
                    0),
                new RelationById( 
                    _userGroups.Last().Id,
                    _userId,
                    FixedRelationTypes.UserGroupRelationType,
                    0)
            };


            //Setup hive
            _hive = MockHiveManager.GetManager()
                .MockContentStore(out _readonlyContentStoreSession, out _readonlyContentStoreSchemaSession, out _contentStoreRepository, out _contentStoreSchemaRepository)
                .MockSecurityStore(out _readonlySecurityStoreSession, out _readonlySecurityStoreSchemaSession, out _securityStoreRepository, out _securityStoreSchemaRepository);

            //Setup content store
            _readonlyContentStoreSession.Exists<TypedEntity>(HiveId.Empty).ReturnsForAnyArgs(true);

            _readonlySecurityStoreSession
                .Get<UserGroup>(true, Arg.Any<HiveId[]>())
                .Returns(MockHiveManager.MockReturnForGet<UserGroup>());

            _readonlyContentStoreSession
                .Get<UserGroup>(true, Arg.Any<HiveId[]>())
                .Returns(MockHiveManager.MockReturnForGet<UserGroup>());

            _securityStoreRepository
                .Get<UserGroup>(true, Arg.Any<HiveId[]>())
                .Returns(MockHiveManager.MockReturnForGet<UserGroup>());

            _contentStoreRepository
                .Get<UserGroup>(true, Arg.Any<HiveId[]>())
                .Returns(MockHiveManager.MockReturnForGet<UserGroup>());

            //Setup security store
            _readonlySecurityStoreSession.GetParentRelations(_systemRootNode.Id, FixedRelationTypes.PermissionRelationType).Returns(systemRootPermissionRelations);
            _readonlySecurityStoreSession.GetParentRelations(_childContentNode.Id, FixedRelationTypes.PermissionRelationType).Returns(childContentNodePermissionRelations);
            _readonlySecurityStoreSession.GetAncestorRelations(_childContentNode.Id, FixedRelationTypes.DefaultRelationType).Returns(childContentNodeAncestorRelations);
            _readonlySecurityStoreSession.GetParentRelations(_userId, FixedRelationTypes.UserGroupRelationType).Returns(userGroupRelations);

            _readonlySecurityStoreSession.Query<UserGroup>().Returns(x => Enumerable.Repeat(new UserGroup() {Name = "Administrator"}, 1).AsQueryable());
        }
    }
}
