using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{

    /// <summary>
    /// Manages the notifications shown in the back-office to administrators
    /// </summary>
    [JsonConverter(typeof(ClientNotificationsJsonConverter))]
    public class ClientNotifications : IEnumerable<NotificationMessage>
    {
        private readonly ControllerContext _controllerContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controllerContext">The temp data provider to put data in</param>
        public ClientNotifications(ControllerContext controllerContext)
        {
            _controllerContext = controllerContext;
        }

        /// <summary>
        /// the internal unique collection of messages
        /// </summary>
        private readonly Dictionary<Guid, NotificationMessage> _messages = new Dictionary<Guid, NotificationMessage>();        

        /// <summary>
        /// Adds a notification message to the stack
        /// </summary>
        /// <param name="notificationMessage"></param>
        public void Add(NotificationMessage notificationMessage)
        {
            //this will throw an exception if the same notification message is added (based on id)
            _messages.Add(notificationMessage.Id, notificationMessage);
        }

        /// <summary>
        /// Removes a notification message from the stack based on it's id
        /// </summary>
        /// <param name="notificationId"></param>
        public void Remove(Guid notificationId)
        {
            _messages.Remove(notificationId);
        }


        #region IEnumerable<NotificationMessage> Members

        public IEnumerator<NotificationMessage> GetEnumerator()
        {
            return _messages.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _messages.Values.GetEnumerator();
        }

        #endregion
    }
}
