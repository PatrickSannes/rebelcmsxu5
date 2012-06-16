using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Cms.Security
{
    //[TestFixture]
    //public class BackOfficeRoleProviderTests : StandardWebTest
    //{
    //    private BackOfficeRoleProvider _roleProvider;

    //    [Test]
    //    public void BackOfficeRoleProviderTests_AddUsersToRoles_Success()
    //    {
    //        //Arrange
    //        var users = new[] {"Administrator"};
    //        var roles = new[] {"Administrator", "Editor"};

    //        //Act
    //        _roleProvider.AddUsersToRoles(users, roles);

    //        //Assert

    //    }

    //    [SetUp]
    //    public void Initialize()
    //    {
    //        base.Init();

    //        var hiveManager = CreateHiveManager();

    //        _roleProvider = new BackOfficeRoleProvider(new Lazy<IHiveManager>(() => hiveManager));
    //    }
    //}
}
