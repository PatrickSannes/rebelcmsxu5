using System;
using Newtonsoft.Json;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents a notification sent to the admin in the back-office
    /// </summary>    
    public class NotificationMessage
    {
        /// <summary>
        /// Constructor with message and type
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        /// <param name="type"></param>
        public NotificationMessage(string msg, string title, NotificationType type)
            : this()
        {
            Message = msg;
            Type = type;
            Title = title;
        }

        public NotificationMessage(string msg, NotificationType type)
            : this()
        {
            Message = msg;
            Type = type;
        }

        /// <summary>
        /// Constructor with message, notification type defaults to Info
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        public NotificationMessage(string msg, string title)
            :this()
        {
            Message = msg;
            Title = title;
        }

        public NotificationMessage(string msg)
            : this()
        {
            Message = msg;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationMessage()
        {
            //create id
            Id = Guid.NewGuid();

            //default to Info
            Type = NotificationType.Info;

            Message = string.Empty;
            Title = string.Empty;
        }

        /// <summary>
        /// The type of notification to send to the UI
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// The message to display
        /// </summary>
        [JsonProperty]
        public string Message { get; set; }

        /// <summary>
        /// The title to display for the message
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The Id assigned to the notification when created
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Override equals operator as messages will be the same with the same id
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Id.Equals(obj);
        }

        /// <summary>
        /// Override GetHashCode as messages will be the same with the same id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
