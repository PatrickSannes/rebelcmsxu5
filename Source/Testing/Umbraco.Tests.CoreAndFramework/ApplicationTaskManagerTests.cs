using System;
using NUnit.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Tasks;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.CoreAndFramework.PersistenceModel;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework
{
    /// <summary>
    /// Summary description for ApplicationTaskManagerTests
    /// </summary>
    [TestFixture]
    public class ApplicationTaskManagerTests : AbstractPartialTrustFixture<ApplicationTaskManagerTests>
    {
        private static IFrameworkContext _frameworkContext;
        private static int _testCounter = 0;

        private void UpdateCounter(int newValue)
        {
            _testCounter = newValue;
        }

        public ApplicationTaskManagerTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [Test]
        public void TaskManager_FiresAllTasksInOrder()
        {
            // Arrange
            // - Add two delegate tasks one after the other which should update a local variable
            _frameworkContext.TaskManager.AddDelegateTask("mytrigger", x => UpdateCounter(9));
            _frameworkContext.TaskManager.AddDelegateTask("mytrigger", x => UpdateCounter(8));
            _frameworkContext.TaskManager.AddDelegateTask("mytrigger", x => UpdateCounter(7));

            // Act
            // - Execute the trigger
            _frameworkContext.TaskManager.ExecuteInContext("mytrigger", this, new TaskEventArgs(_frameworkContext, new EventArgs()));

            // Assert
            // - Assert that the local variable equals the last task which should have been run
            Assert.AreEqual(7, _testCounter);
        }

        public override void TestSetup()
        {
            _frameworkContext = new FakeFrameworkContext();
        }

        public override void TestTearDown()
        {
            return;
        }
    }
}
