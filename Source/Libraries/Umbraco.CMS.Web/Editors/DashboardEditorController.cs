using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration.Dashboards;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionInvokers;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Editors
{
    [Editor("CEA20A03-13F6-4D8E-8350-A9CF3FC58039")]
    [UmbracoEditor]
    public class DashboardEditorController : AbstractEditorController
    {
        public DashboardEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            this.ActionInvoker = new DashboardActionInvoker(requestContext);
        }

        /// <summary>
        /// The Action to render the dashboard
        /// </summary>
        /// <returns></returns>
        /// <param name="appAlias">The current application name, optional</param>
        [HttpGet]
        public virtual ActionResult Dashboard(string appAlias)
        {
            if (appAlias.IsNullOrWhiteSpace())
                return HttpNotFound();

            Func<IEnumerable<IDashboardApplication>, bool> isMatch = x =>
                {
                    var appAliases = x.Select(a => a.ApplicationAlias.ToUpper()).ToArray();
                    if (appAliases.Contains("*")) return true;
                    if (appAliases.Contains(appAlias.ToUpper())) return true;
                    return false;
                };

            //get groups for the current app
            var groups = BackOfficeRequestContext.Application.Settings.DashboardGroups
                .Where(x => isMatch(x.Applications))
                .ToArray();

            //map groups to model
            var model = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers
                .Map<IEnumerable<IDashboardGroup>, DashboardApplicationModel>(groups);

            return View(model);
        }


        [ChildActionOnly]
        public ActionResult RenderDashboard(DashboardItemModel model)
        {
            switch (model.DashboardType)
            {
                case DashboardType.PartialView:
                    return PartialView(model.ViewName);
                case DashboardType.ChildAction:
                    MethodInfo childAction;
                    var editorController = model.GetEditorDashboardAction(BackOfficeRequestContext.RegisteredComponents, out childAction);

                    if (!TypeFinder.IsTypeAssignableFrom<PartialViewResult>(childAction.ReturnType))
                    {
                        throw new InvalidOperationException("ChildAction dashboards should have a return type of " + typeof(PartialViewResult).Name);
                    }

                    using (var controller = editorController.Value)
                    {
                        if (controller == null)
                            throw new TypeLoadException("Could not create controller: " + UmbracoController.GetControllerName(editorController.Metadata.ComponentType));
                        
                        //proxy the request to the controller
                        return this.ProxyRequestToController(controller, childAction);
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }


        }
    }
}
