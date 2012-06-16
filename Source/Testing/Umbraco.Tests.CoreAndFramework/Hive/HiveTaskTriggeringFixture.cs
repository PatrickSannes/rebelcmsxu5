using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.Exceptions;
using NUnit.Framework;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Cms.Web.System;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security;
using Umbraco.Framework.Tasks;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.Tasks;
using Umbraco.Tests.Extensions;
using Umbraco.Hive;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    using Umbraco.Framework.Persistence.Model.Associations;
    using Umbraco.Framework.Persistence.Model.Associations._Revised;

    [TestFixture]
    public class HiveTaskTriggeringFixture
    {
        private FakeFrameworkContext _frameworkContext;
        private GroupUnitFactory _groupUnitFactory;
        private AbstractScopedCache _unitScopedCache;

        [SetUp]
        public void Setup()
        {
            _frameworkContext = new FakeFrameworkContext();
            _unitScopedCache = new DictionaryScopedCache();
            var providerGroup = GroupedProviderMockHelper.GenerateProviderGroup(1, 0, 1, _frameworkContext);
            var idRoot = new Uri("oh-yeah://this-is-my-root/");
            _groupUnitFactory = new GroupUnitFactory(providerGroup.Writers, idRoot, FakeHiveCmsManager.CreateFakeRepositoryContext(_frameworkContext), _frameworkContext, () => _unitScopedCache);
        }

        [Test]
        public void WhenEntityIsAdded_WhenCacheWatcherTaskIsRegistered_EntityIsPutInScopedCache()
        {
            // Arrange
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostAddOrUpdateOnUnitComplete, () => new CacheWatcherTask(_frameworkContext), true);

            // Act
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);
                uow.Complete();

                // Assert the task has been fired
                Assert.That(_unitScopedCache.GetOrCreate(anything.Id.ToString(), () => null), Is.Not.Null);
            }
        }

        [Test]
        public void WhenEntityIsRetrievedViaGet_WhenCacheWatcherTaskIsRegistered_EntityIsPutInScopedCache()
        {
            // Arrange
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostReadEntity, () => new CacheWatcherTask(_frameworkContext), true);

            // Act
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                uow.Repositories.AddOrUpdate(anything);
                uow.Complete();
            }

            using (var uow = _groupUnitFactory.Create())
            {
                // Cause the task to be fired
                var getItem = uow.Repositories.Get(HiveId.Empty); // Store is mocked
                Assert.NotNull(getItem);

                // Assert the task has been fired
                Assert.That(_unitScopedCache.GetOrCreate(getItem.Id.ToString(), () => null), Is.Not.Null);
            }
        }

        [Test]
        public void WhenRelationIsAdded_ViaAddRelationMethod_PreAndPostRelationAddedBeforeUnitComplete_TasksAreTriggered()
        {
            // Arrange
            var preEventMock = Substitute.For<AbstractTask>(_frameworkContext);
            var postEventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PreRelationAdded, () => preEventMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PostRelationAdded, () => postEventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var parentAnything = HiveModelCreationHelper.MockTypedEntity();
                var childAnything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Add items
                uow.Repositories.AddOrUpdate(parentAnything);
                uow.Repositories.AddOrUpdate(childAnything);

                // Add relation and cause the task to be fired
                uow.Repositories.AddRelation(parentAnything, childAnything, FixedRelationTypes.DefaultRelationType, 0);

                // Assert the task has been fired once for the group and once for the repository (handler can check source if they only want to act once)
                DoesNotThrow(() => preEventMock.Received(2).Execute(Arg.Any<TaskExecutionContext>()), "Pre-Call was not received 2 times");
                DoesNotThrow(() => postEventMock.Received(2).Execute(Arg.Any<TaskExecutionContext>()), "Post-Call was not received 2 times");
            }
        }

        private void DoesNotThrow(NUnit.Framework.TestDelegate action, string message = null)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Assert.Fail(message.IfNullOrWhiteSpace(string.Empty) + "\n" + ex.Message + "\n" + ex.InnerException.IfNotNull(x => x.ToString()));
            }
        }

        [Test]
        public void WhenRelationIsRemoved_ViaRemoveRelationMethod_PreAndPostRelationRemovedBeforeUnitComplete_TasksAreTriggered()
        {
            // Arrange
            var preEventMock = Substitute.For<AbstractTask>(_frameworkContext);
            var postEventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PreRelationRemoved, () => preEventMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PostRelationRemoved, () => postEventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var parentAnything = HiveModelCreationHelper.MockTypedEntity();
                var childAnything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Add items
                uow.Repositories.AddOrUpdate(parentAnything);
                uow.Repositories.AddOrUpdate(childAnything);

                // Add relation
                uow.Repositories.AddRelation(parentAnything, childAnything, FixedRelationTypes.DefaultRelationType, 0);

                // Remove relation and cause task to be fired
                uow.Repositories.RemoveRelation(new Relation(FixedRelationTypes.DefaultRelationType, parentAnything, childAnything));

                // Assert the task has been fired
                DoesNotThrow(() => preEventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Pre-Call was not received 1 times");
                DoesNotThrow(() => postEventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Post-Call was not received 1 times");
            }
        }

        [Test]
        public void WhenRelationIsAdded_ViaRelationProxies_PreAndPostRelationAddedBeforeUnitComplete_TasksAreTriggered()
        {
            // Arrange
            var preEventMock = Substitute.For<AbstractTask>(_frameworkContext);
            var postEventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PreRelationAdded, () => preEventMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.Relations.PostRelationAdded, () => postEventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var parentAnything = HiveModelCreationHelper.MockTypedEntity();
                var childAnything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postEventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Add items
                parentAnything.RelationProxies.EnlistChild(childAnything, FixedRelationTypes.DefaultRelationType);
                uow.Repositories.AddOrUpdate(parentAnything);
                uow.Repositories.AddOrUpdate(childAnything);

                // Complete the uow which should raise the task as relation proxies aren't added until uow completion
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => preEventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Pre-Call was not received 1 times");
                DoesNotThrow(() => postEventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Post-Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityIsAdded_PreAddOrUpdateBeforeUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdate, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityIsAdded_PreAddOrUpdateAfterUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdateOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }

        [Test]
        public void WhenEntityIsAdded_AndUnitCompletionTaskCausesCancellation_UnitIsRolledBack()
        {
            // Arrange
            var preAddMock = Substitute.For<AbstractTask>(_frameworkContext);
            preAddMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>())).Do(x => x.Arg<TaskExecutionContext>().Cancel = true);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdateOnUnitComplete, () => preAddMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                Assert.That(uow.WasAbandoned, Is.False);
                uow.Complete();

                // Assert
                // Check the task has been fired
                DoesNotThrow(() => preAddMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Task was not executed");
                // Check the unit was then rolled back
                Assert.That(uow.WasAbandoned, Is.True);
            }
        }

        [Test]
        public void WhenEntityIsAdded_PostAddOrUpdateAfterUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostAddOrUpdateOnUnitComplete, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Check the task has still not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Complete the unit
                uow.Complete();

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
            }
        }

        [Test]
        public void WhenEntityIsAdded_AndTaskCausesCancellation_EntityIsNotAdded()
        {
            // Arrange
            var preAddMock = Substitute.For<AbstractTask>(_frameworkContext);
            preAddMock.When(x => x.Execute(Arg.Any<TaskExecutionContext>())).Do(x => x.Arg<TaskExecutionContext>().Cancel = true);
            var postAddMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PreAddOrUpdate, () => preAddMock, true);
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostAddOrUpdate, () => postAddMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the pre-add and post-add tasks have not yet been fired
                DoesNotThrow(() => preAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
                DoesNotThrow(() => postAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));

                // Cause the pre-add task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Assert the pre-add task has been fired, and the post-add task is not fired
                DoesNotThrow(() => preAddMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received 1 times");
                DoesNotThrow(() => postAddMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()));
            }
        }

        [Test]
        public void WhenEntityIsAdded_PostAddOrUpdateBeforeUnitComplete_TaskIsTriggered()
        {
            // Arrange
            var eventMock = Substitute.For<AbstractTask>(_frameworkContext);

            // Act
            _frameworkContext.TaskManager.AddTask(TaskTriggers.Hive.PostAddOrUpdate, () => eventMock, true);
            using (var uow = _groupUnitFactory.Create())
            {
                var anything = HiveModelCreationHelper.MockTypedEntity();

                // Check the task has not yet been fired
                DoesNotThrow(() => eventMock.Received(0).Execute(Arg.Any<TaskExecutionContext>()), "Call was received early");

                // Cause the task to be fired
                uow.Repositories.AddOrUpdate(anything);

                // Assert the task has been fired
                DoesNotThrow(() => eventMock.Received(1).Execute(Arg.Any<TaskExecutionContext>()), "Call was not received");
            }
        }
    }
}