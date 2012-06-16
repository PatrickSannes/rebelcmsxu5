using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Umbraco.Framework.Diagnostics;

namespace Umbraco.Framework.Tasks
{
    using Umbraco.Framework.Context;

    /// <summary>
    /// A service for executing tasks when they match given trigger names
    /// </summary>
    /// <remarks></remarks>
    public class ApplicationTaskManager
    {
        private readonly List<Lazy<AbstractTask, TaskMetadata>> _tasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTaskManager"/> class.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <remarks></remarks>
        public ApplicationTaskManager(IEnumerable<Lazy<AbstractTask, TaskMetadata>> tasks)
        {
            _tasks = tasks.ToList();
        }

        /// <summary>
        /// Registers a task handler with the manager.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        public ApplicationTaskManager AddTask(Lazy<AbstractTask, TaskMetadata> task)
        {
            _tasks.Add(task);
            return this;
        }

        /// <summary>
        /// Registers a task handler with the manager.
        /// </summary>
        /// <param name="triggerName">Name of the trigger.</param>
        /// <param name="valueFactory">The factory which will create the handler.</param>
        /// <param name="continueOnError">if set to <c>true</c> [continue on error].</param>
        /// <returns></returns>
        public ApplicationTaskManager AddTask(string triggerName, Func<AbstractTask> valueFactory, bool continueOnError)
        {
            return AddTask(valueFactory, new TaskMetadata(triggerName, continueOnError));
        }

        /// <summary>
        /// Registers a task handler with the manager.
        /// </summary>
        /// <param name="valueFactory">The factory which will create the handler.</param>
        /// <param name="metaData">The meta data for the task.</param>
        /// <returns></returns>
        public ApplicationTaskManager AddTask(Func<AbstractTask> valueFactory, TaskMetadata metaData)
        {
            return AddTask(new Lazy<AbstractTask, TaskMetadata>(valueFactory, metaData));
        }

        /// <summary>
        /// Adds a new delegate task to the task manager with the specified trigger
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ApplicationTaskManager AddDelegateTask(string triggerName, Action<TaskExecutionContext> callback)
        {
            var metaData = new TaskMetadata(triggerName, true);
            AddTask(new Lazy<AbstractTask, TaskMetadata>(() => new DelegateTask(null, callback), metaData));
            return this;
        }

        /// <summary>
        /// Executes the task in the current context synchronously
        /// </summary>
        /// <param name="triggerName">Name of the trigger.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="TaskEventArgs"/> instance containing the event data.</param>
        /// <returns>The <see cref="TaskExecutionContext"/> in which the tasks run.</returns>
        public TaskExecutionContext ExecuteInContext(string triggerName, object sender, TaskEventArgs eventArgs)
        {
            var taskExecutionContext = new TaskExecutionContext(sender, eventArgs);
            ExecuteInContext(triggerName, taskExecutionContext);
            return taskExecutionContext;
        }

        /// <summary>
        /// Executes the task in the current context synchronously. Executes the tasks matching the specified trigger name.
        /// </summary>
        /// <param name="triggerName">Name of the trigger.</param>
        /// <param name="context">The context.</param>
        public ApplicationTaskManager ExecuteInContext(string triggerName, TaskExecutionContext context)
        {
            var canceled = false;
            return ExecuteInContext(triggerName, context, out canceled);
        }

        /// <summary>
        /// Executes the task in the current context synchronously. Executes the tasks matching the specified trigger name.
        /// </summary>
        /// <param name="triggerName">Name of the trigger.</param>
        /// <param name="context">The context.</param>
        /// <param name="canceled">if set to <c>true</c> taks was canceled.</param>
        /// <returns></returns>
        public ApplicationTaskManager ExecuteInContext(string triggerName, TaskExecutionContext context, out bool canceled)
        {
            canceled = false;

            using (DisposableTimer.TraceDuration<ApplicationTaskManager>("Raising trigger: " + triggerName, "End trigger " + triggerName))
            {
                try
                {
                    var tasks = _tasks
                        .Where(x => String.Compare(x.Metadata.TriggerName, triggerName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        .ToArray();

                    foreach (var task in tasks)
                    {
                        var continueOnFailure = task.Metadata.ContinueOnError;
                        var abstractTask = task.Value;
                        try
                        {
                            LogHelper.TraceIfEnabled(abstractTask.GetType(), () => "Executing " + abstractTask.ToString());

                            abstractTask.Execute(context);

                            if (context.Cancel && !continueOnFailure)
                                break;
                        }
                        catch (Exception ex)
                        {
                            
                            var name = abstractTask != null ? abstractTask.GetType().Name : "<unknown; task was null>";
                            context.Exceptions.Add(ex);
                            LogHelper.Error<ApplicationTaskManager>(
                                String.Format("Error running a task trigger {0} for task {1}", triggerName, name), ex);
                            if (!continueOnFailure) break;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error<ApplicationTaskManager>("Error running a task trigger " + triggerName, e);
                }
            }

            return this;
        }

        public void ExecuteInCancellableTask<T>(object eventSource, T taskItem, string preActionTaskTrigger, string postActionTaskTrigger,
                                                        Action execution, Func<T, EventArgs> preActionEventArgs, Func<T, EventArgs> postActionEventArgs, IFrameworkContext frameworkContext)
        {
            var callerEventArgs = preActionEventArgs.Invoke(taskItem);
            var eventArgs = new TaskEventArgs(frameworkContext, callerEventArgs);

            var context = ExecuteInContext(preActionTaskTrigger, eventSource, eventArgs);

            try
            {
                if (!context.Cancel) execution.Invoke();
            }
            finally
            {
                if (!context.Cancel)
                {
                    var hiveEntityPostActionEventArgs = postActionEventArgs.Invoke(taskItem);
                    var taskEventArgs = new TaskEventArgs(frameworkContext, hiveEntityPostActionEventArgs);
                    ExecuteInContext(postActionTaskTrigger, eventSource, taskEventArgs);
                }
            }
        }
    }
}
