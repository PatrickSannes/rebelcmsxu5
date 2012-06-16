using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Tasks;

namespace Umbraco.Cms.Web.Tasks
{
    /// <summary>
    /// This task is not invoked directly, instead its normally used as a proxy task
    /// for configuration tasks.
    /// </summary>
    [Task("CFDC6803-13B1-4F19-9FA4-F622BC5A4D05", "display-ui", ContinueOnFailure = false)]
    public class DisplayUITask : ConfigurationTask
    {
        public DisplayUITask(ConfigurationTaskContext configurationTaskContext)
            : base(configurationTaskContext)
        {
        }

        public override void Execute(TaskExecutionContext context)
        {
            var controller = context.EventSource as Controller;
            if (controller != null)
            {
                Mandate.That(ConfigurationTaskContext.Parameters.ContainsKey("controllerType"), x => new ArgumentNullException("The 'controllerType' configuration parameter is required for DisplayUITask"));
                Mandate.That(ConfigurationTaskContext.Parameters.ContainsKey("action"), x => new ArgumentNullException("The 'action' configuration parameter is required for DisplayUITask"));

                var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
                var controllerType = Type.GetType(ConfigurationTaskContext.Parameters["controllerType"]);
                if (controllerType != null)
                {
                    var url = urlHelper.GetEditorUrl(ConfigurationTaskContext.Parameters["action"], controllerType, null);
                    controller.HttpContext.Response.Redirect(url);
                }
            }
        }
    }
}