using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;
using MvcContrib.TestHelper.Fakes;
using NSubstitute;
using Rhino.Mocks;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Tests.Cms.Stubs;
using System.Globalization;
using Umbraco.Cms.Web;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Cms
{
    /// <summary>
    /// Helper Methods for supporting ModelState testing
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Ensures the controller has the correct context's applied
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="querystringValues">The querystring values.</param>
        /// <param name="formValues">The form values.</param>
        /// <param name="backOfficeRequest"></param>
        /// <param name="ensureModelStateError">Will make sure that there's always an error in Model state so that the controller doesn't validate and thus doesn't Redirect so we can look at the model returned</param>
        /// <param name="noExecution">If true, then the Excecute method will not be run but it is required for any controller that needs the Initialize method run</param>
        /// <param name="userName">The user name to use for the current http context, default is administrator</param>
        /// <param name="userRoles">The roles the user should be in, default is administrator</param>
        [SecuritySafeCritical]
        public static void InjectDependencies<T>(
            this T controller,
            IDictionary<string, string> querystringValues,
            IDictionary<string, string> formValues, 
            IBackOfficeRequestContext backOfficeRequest,
            bool ensureModelStateError = true,
            bool noExecution = false,
            string userName = "administrator",
            string[] userRoles = null) 
            where T : Controller
        {
            var routeData = new RouteData();
            
            //we need to mock the dependency resolver for the IBackOfficeRequest context
            //because of how the UmbracoAuthorizationAttribute works
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            DependencyResolver.SetResolver(dependencyResolver);
            dependencyResolver.GetService(typeof(IBackOfficeRequestContext)).Returns(backOfficeRequest);

            var ctx = new FakeHttpContextFactory("~/MyController/MyAction/MyId");

            controller.Url = new UrlHelper(ctx.RequestContext);
            controller.ValueProvider = formValues.ToValueProvider();            
            var context = new ControllerContext(ctx.RequestContext, controller) {RouteData = routeData};
            controller.ControllerContext = context;
            //set the form values            
            controller.HttpContext.Request.Stub(x => x.QueryString).Return(formValues.ToNameValueCollection());
            controller.HttpContext.Request.Stub(x => x.Form).Return(formValues.ToNameValueCollection());
            controller.HttpContext.Request.Stub(x => x.RequestType).Return(formValues.Count > 0 ? "POST" : "GET");
            if (userRoles == null)
                userRoles = new string[] { "administrator" }; //default to administrator
            var userData = new UserData()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    AllowedApplications = new string[] {},
                    Username = userName,
                    RealName = userName,
                    Roles = userRoles,
                    SessionTimeout = 0,
                    StartContentNode = "-1",
                    StartMediaNode = "-1"
                };
            controller.HttpContext.Stub(x => x.User).Return(new FakePrincipal(new UmbracoBackOfficeIdentity(new FormsAuthenticationTicket(4, userName, DateTime.Now, DateTime.Now.AddDays(1), false, (new JavaScriptSerializer()).Serialize(userData))), userRoles));

            if (!noExecution)
            {
                //if Initialize needs to run on the controller, this needs to occur, so we'll swallow the exception cuz there will always be one.
                try
                {
                    (controller as IController).Execute(ctx.RequestContext);
                }
                catch (System.Exception)
                {
                }    
            }

            if (ensureModelStateError )
            {
                //always ensure invalid model state so we can get the model returned
                controller.ModelState.AddModelError("DummyKey", "error");
            }
        }

        public static void InjectDependencies<T>(this T controller, IBackOfficeRequestContext backOfficeRequest) where T : Controller
        {
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), backOfficeRequest);
        }
    }
}