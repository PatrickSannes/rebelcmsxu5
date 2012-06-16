using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Cms.Web;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class ClientNotificationsTests
    {

        [Test]
        public void ClientNotification_Serialization()
        {

            //Arrange

            var controller = new NullController();
            controller.InjectDependencies(new FakeBackOfficeRequestContext(new FakeUmbracoApplicationContext(false)));
            var notifications = new ClientNotifications(controller.ControllerContext);

            var msg1 = new NotificationMessage("hello", "world");
            var msg2 = new NotificationMessage("world", NotificationType.Error);
            var msg3 = new NotificationMessage("good", NotificationType.Warning);
            var msg4 = new NotificationMessage("bye", NotificationType.Success);

            //Act

            notifications.Add(msg1);
            notifications.Add(msg2);
            notifications.Add(msg3);
            notifications.Add(msg4);

            //Assert

            var serializeed = notifications.ToJsonString();
            Assert.AreEqual(@"[{""id"":""" + msg1.Id.ToString("N") + @""",""message"":""hello"",""title"":""world"",""type"":""info""},{""id"":""" + msg2.Id.ToString("N") + @""",""message"":""world"",""title"":"""",""type"":""error""},{""id"":""" + msg3.Id.ToString("N") + @""",""message"":""good"",""title"":"""",""type"":""warning""},{""id"":""" + msg4.Id.ToString("N") + @""",""message"":""bye"",""title"":"""",""type"":""success""}]", serializeed);
        }

        [Test]
        public void ClientNotification_Message_Equals()
        {

            // a msg is only as good as it's Id

            //Arrange

            var msg = new NotificationMessage("hello");
            var guid = msg.Id;
      
            //Assert

            Assert.IsTrue(msg.Equals(guid));
        }

        [Test]
        public void ClientNotification_Message_Added()
        {

            //Arrange
            var controller = new NullController();
            controller.InjectDependencies(new FakeBackOfficeRequestContext(new FakeUmbracoApplicationContext(false)));
            var notifications = new ClientNotifications(controller.ControllerContext);

            var msg = new NotificationMessage("hello");
            
            //Act

            notifications.Add(msg);
            
            //Assert

            Assert.AreEqual(1, notifications.Count());
        }

        [Test]
        public void ClientNotification_Message_Removed()
        {

            //Arrange
            var controller = new NullController();
            controller.InjectDependencies(new FakeBackOfficeRequestContext(new FakeUmbracoApplicationContext(false)));
            var notifications = new ClientNotifications(controller.ControllerContext);

            var msg = new NotificationMessage("hello");

            //Act

            notifications.Add(msg);
            notifications.Remove(msg.Id);

            //Assert

            Assert.AreEqual(0, notifications.Count());
        }

        [Test]        
        [ExpectedException(typeof(ArgumentException))]
        public void ClientNotification_Unique_Messages_Only()
        {

            //Arrange
            var controller = new NullController();
            controller.InjectDependencies(new FakeBackOfficeRequestContext(new FakeUmbracoApplicationContext(false)));
            var notifications = new ClientNotifications(controller.ControllerContext);

            var msg = new NotificationMessage("hello");

            //Act

            notifications.Add(msg);
            notifications.Add(msg);

          
        }
    }
}
