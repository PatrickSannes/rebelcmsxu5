using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{
    /// <summary>
    /// Required to use client notification engine
    /// </summary>
    public interface INotificationController
    {
        ClientNotifications Notifications { get; }
     
        /// <summary>
        /// The Id of the current request
        /// </summary>
        Guid RequestId { get; }
    }
}