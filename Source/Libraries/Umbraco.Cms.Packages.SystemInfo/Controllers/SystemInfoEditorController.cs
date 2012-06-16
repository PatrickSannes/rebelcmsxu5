using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;

namespace Umbraco.Cms.Packages.SystemInfo.Controllers
{
    [Editor("5D85C1EC-ED5C-451E-A53F-78CC95AA53A2")]
    public class SystemInfoEditorController : DashboardEditorController
    {
        public SystemInfoEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }


        public ActionResult PluginInfo()
        {
            return View();
        }

        public ActionResult Backup()
        {
            return View();
        }

        public ActionResult Permissions()
        {
            return View();
        }
    }
}
