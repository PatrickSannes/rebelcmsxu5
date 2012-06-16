using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Cms.Web;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class ControllerExtensionsTests
    {
        private DummyController _controller;

        [Test]
        public void ControllerExtensionsTests_ExecuteSecuredMethod_UnSecuredMethod_Success()
        {
            //Act
            var result = _controller.TryExecuteSecuredMethod(x => x.TestUnSecuredMethod(HiveId.Empty));

            //Assert
            Assert.IsTrue(result.Authorized);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void ControllerExtensionsTests_ExecuteSecuredMethod_SecuredMethod_Success()
        {
            //TODO: Not sure how to test, as ExecuteSecuredMethod internally calls DependencyResovler

            //Act
            //var result = _controller.TryExecuteSecuredMethod(x => x.TestSecuredMethodSuccess(HiveId.Empty));

            //Assert
            //Assert.IsTrue(result.Authorized);
            //Assert.IsTrue(result.Success);
        }

        [SetUp]
        public void Initialize()
        {
            _controller = new DummyController();

            var userData = @"{
                'Id' : '6C50D4C1-B608-4EFA-B488-C95DBC6F9BA6',
                'Roles' : [ 'Editor' ],
                'SessionTimeout' : 60,
                'RealName' : 'Dummy User',
                'StartContentNode' : '',
                'StartMediaNode' : '',
                'AllowedApplications' : [ 'Content', 'Media' ]
            }";

            var ticket = new FormsAuthenticationTicket(1, "DummyUser", DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1), false, userData);
            var identity = new UmbracoBackOfficeIdentity(ticket);

            var user = Substitute.For<IPrincipal>();
            user.Identity.Returns(identity);

            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User.Returns(user);

            var routeData = new RouteData();
            var context = new RequestContext(httpContext, routeData);
            var controllerContext = new ControllerContext(context, _controller);

            _controller.ControllerContext = controllerContext;
        }

        public class DummyController : Controller
        {
            public void TestUnSecuredMethod(HiveId id)
            { }

            [UmbracoAuthorize(Permissions = new[]{ FixedPermissionIds.Publish })]
            public void TestSecuredMethodSuccess(HiveId id)
            { }
        }
    }
}
