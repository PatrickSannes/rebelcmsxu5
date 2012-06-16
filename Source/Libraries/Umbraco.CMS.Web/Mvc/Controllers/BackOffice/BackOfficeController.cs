using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ActionInvokers;

using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Localization;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{
    /// <summary>
    /// Has the basic properties for back-office controllers
    /// </summary>
    [HandleError(View = "Exception")]
    [InstalledFilter(Order = 0)]
    [HandleAuthorizationErrors(true)]
    public abstract class BackOfficeController : Controller, INotificationController, IRequiresBackOfficeRequestContext
    {

        protected BackOfficeController(IBackOfficeRequestContext requestContext)
        {
            BackOfficeRequestContext = requestContext;
            Notifications = new ClientNotifications(ControllerContext);
            ActionInvoker = new UmbracoBackOfficeActionInvoker(requestContext);
        }

        public IBackOfficeRequestContext BackOfficeRequestContext { get; set; }
        
        public ClientNotifications Notifications { get; private set; }

        /// <summary>
        /// Returns the request Id from the BackOfficeRequestContext
        /// </summary>
        public Guid RequestId
        {
            get { return BackOfficeRequestContext.RequestId; }
        }

        /// <summary>
        /// Helper method to add the notification to the UI regarding validation errors found in model state
        /// </summary>
        protected void AddValidationErrorsNotification()
        {                       
            var errorCount = ModelState.Values.Where(v => v.Errors.Any()).Count();
            Notifications.Add(new NotificationMessage(
                "Save.ErrorsOccurred.Message".Localize(this, new { ErrorCount = errorCount }),
                "Save.ErrorsOccurred.Title".Localize(this),
                NotificationType.Warning));                
        }
    }
}
