using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    public class SupportClientNotificationsAttribute : AbstractTempDataCookieFilter
    {

        public override string CookieNamePrefix
        {
            get { return "notification_"; }
        }

        public override string QueryStringName
        {
            get { return "NotificationId"; }
        }

        /// <summary>
        /// Returns the request id as the temp data key
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        protected override string GetTempDataKey(ControllerContext filterContext)
        {
            var backOfficeController = GetController(filterContext.Controller);
            return backOfficeController.RequestId.ToString("N");     
        }
      
        /// <summary>
        /// Ensures that the controller is of the required type, if so lets execution continue on the base class
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var backOfficeController = GetController(filterContext.Controller);
            if (!backOfficeController.Notifications.Any())
                return;
            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// Ensures that the controller is of the required type, if so lets execution continue on the base class
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var backOfficeController = GetController(filterContext.Controller);
            if (!backOfficeController.Notifications.Any())
                return;
            base.OnResultExecuted(filterContext);
        }

        /// <summary>
        /// Returns the notifications to be stored in the temp data
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        protected override object GetTempDataValue(ControllerContext filterContext)
        {
            var backOfficeController = GetController(filterContext.Controller);
            if (!backOfficeController.Notifications.Any())
                return null;
            return backOfficeController.Notifications;
        }

        /// <summary>
        /// Gets the BackOfficeController type from the filter context's controller instance
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        private INotificationController GetController(IController controller)
        {
            var backOfficeController = controller as INotificationController;
            if (backOfficeController != null)
            {
                return backOfficeController;
            }
            else
            {
                throw new NotSupportedException("The " + GetType().Name + " can only be used on controllers of type " + typeof(INotificationController).Name);
            }
        }

    }
}
