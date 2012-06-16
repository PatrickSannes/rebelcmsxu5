using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Security;
using ClientDependency.Core;
using ClientDependency.Core.Mvc;
using Umbraco.Cms.Web.Configuration.ApplicationSettings;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using System.Linq;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Security;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;
using BackOfficeDefaultModel = Umbraco.Cms.Web.Model.BackOffice.BackOfficeDefaultModel;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{
    

    /// <summary>
    /// The controller used to render the back office application
    /// </summary>
    /// <remarks>
    /// We need to ensure that each action has the explicit UmbracoAuthorize attribute applied (except for Login) because this does
    /// not inherit from SecuredBackOfficeController
    /// </remarks>
    public class DefaultController : BackOfficeController
    {
        private readonly IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> _editorControllers;
        private readonly IEnumerable<Lazy<MenuItem, MenuItemMetadata>> _menuItems;
        private readonly IEnumerable<Lazy<TreeController, TreeMetadata>> _trees;

        /// <summary>
        /// 
        /// </summary>
        public DefaultController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {

            _editorControllers = requestContext.RegisteredComponents.EditorControllers;
            _menuItems = requestContext.RegisteredComponents.MenuItems;
            _trees = requestContext.RegisteredComponents.TreeControllers;
        }

        /// <summary>
        /// Renders the login view
        /// </summary>
        /// <returns></returns>
        [InstalledFilter]
        public ActionResult Login(LoginDisplayType displayType = LoginDisplayType.StandardPage)
        {
            return View(new LoginModel() { DisplayType = displayType});
        }

        /// <summary>
        /// Renders the generic Search list template
        /// </summary>
        /// <returns></returns>
        [InstalledFilter(Order = 0)]
        [UmbracoAuthorize(Order = 1)]
        public ActionResult Search(string searchTerm, Guid treeId)
        {

            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        [InstalledFilter]
        public ActionResult LoginForm(LoginModel model)
        {
            model.Username = model.Username.Trim();
            model.Password = model.Password.Trim();
            var provider = Membership.Providers.GetBackOfficeMembershipProvider();
            if (provider.ValidateUser(model.Username, model.Password))
            {
                if (provider.IsDefaultBackOfficeMembershipProvider())
                {
                    var umpProvider = (BackOfficeMembershipProvider)provider;
                    var umbracoUser = umpProvider.GetUmbracoUser(model.Username, false);
                    HttpContext.CreateUmbracoAuthTicket(umbracoUser);

                    //check if the ReturnUrl is specified in the route
                    var redirectUrl = HttpContext.Request["ReturnUrl"];
                    if (!string.IsNullOrEmpty(redirectUrl) &&
                        !(redirectUrl.StartsWith("http") && !redirectUrl.StartsWith(HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority)))
                    {
                        return Redirect(HttpContext.Request["ReturnUrl"]);
                    }

                    //check if it is an overlay login (auth timeout)
                    if (model.DisplayType == LoginDisplayType.DisplayingOverlay)
                    {
                        return View("LoginOverlaySuccess");
                    }

                    return RedirectToAction("App");
                }
            }

            ModelState.AddModelError("Login.Invalid", "Login.Invalid".Localize(this));

            return View(model);
        }

        [UmbracoAuthorize]
        public ActionResult Logout()
        {
            HttpContext.UmbracoLogout();

            return RedirectToAction("Login");
        }

        /// <summary>
        /// Renders the default back-office View
        /// </summary>
        /// <param name="appAlias">The application to render</param>
        /// <returns></returns>
        [InstalledFilter(Order = 0)]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.BackOfficeAccess }, Order = 1)]
        [HandleAuthorizationErrors(false)]
        public virtual ActionResult App(string appAlias)
        {
            //we need to ensure the sprite files for the tree/app icons are added to CDF
            foreach (var path in BackOfficeRequestContext.DocumentTypeIconResolver.Sprites
                .Select(sprites =>
                    BackOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeIconFolder + "/" + sprites.Value.Name))
            {
                HttpContext.GetLoader().RegisterDependency(path, ClientDependencyType.Css);
            }
            foreach (var path in BackOfficeRequestContext.ApplicationIconResolver.Sprites
                .Select(sprites =>
                    BackOfficeRequestContext.Application.Settings.UmbracoFolders.ApplicationIconFolder + "/" + sprites.Value.Name))
            {
                HttpContext.GetLoader().RegisterDependency(path, ClientDependencyType.Css);
            }

            //create all menu item instances one time for rendering

            //Register all client dependencies for each menu item and tree that is found
            var dependencies = new List<IClientDependencyFile>();

            foreach (var attr in _menuItems
                    .Select(m => m.Metadata.ComponentType.GetCustomAttributes(typeof(ClientDependencyAttribute), true)
                    .OfType<ClientDependencyAttribute>()
                    .Where(x => !string.IsNullOrEmpty(x.FilePath))))
            {
                dependencies.AddRange(attr);
            }
            foreach (var attr in _trees
                .Select(t => t.Metadata.ComponentType.GetCustomAttributes(typeof(ClientDependencyAttribute), true)
                .OfType<ClientDependencyAttribute>()
                .Where(x => !string.IsNullOrEmpty(x.FilePath))))
            {
                dependencies.AddRange(attr);
            }


            ControllerContext.GetLoader().RegisterClientDependencies(dependencies, new List<IClientDependencyPath>());

            //create the model to return
            var model = new BackOfficeDefaultModel
                            {
                                //Applications = apps,
                                TreeModel = new TreeRenderModel(Url.GetTreeUrl(appAlias)) { ManuallyInitialize = true },
                                //DefaultDashboardEditor = editor,
                                CurrentApplicationAlias = appAlias,
                                MenuItems = _menuItems.Select(x => x.Value)
                            };

            return View("Index", model);
        }

        public PartialViewResult ApplicationTray()
        {
            var currentUser = (UmbracoBackOfficeIdentity)HttpContext.User.Identity;

            //get the apps that the user is allowed to view
            var apps = BackOfficeRequestContext.Application.FrameworkContext
                .TypeMappers.Map<IEnumerable<IApplication>, IEnumerable<ApplicationTrayModel>>(
                    BackOfficeRequestContext.Application.Settings.Applications)
                .Where(x => currentUser.AllowedApplications.Any(y => y == x.Alias));

            //set the tree Render Model and dashboard url for each app
            foreach (var a in apps)
            {
                a.TreeModel = new TreeRenderModel(Url.GetTreeUrl(a.Alias));
                a.DashboardUrl = Url.GetDashboardUrl(a.Alias);
            }
            return PartialView("TrayPartial", apps);
        }

        /// <summary>
        /// Insufficient the permissions.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public virtual ActionResult InsufficientPermissions(string url)
        {
            ViewBag.Url = url;

            return View();
        }
    }
}
