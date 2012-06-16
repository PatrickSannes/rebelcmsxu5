using System.Collections.Generic;
using System.Collections.Specialized;

namespace Umbraco.Framework.Tasks
{
    /// <summary>
    /// A context for passing state along a task execution queue
    /// </summary>
    /// <remarks></remarks>
    public class TaskExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public TaskExecutionContext()
        {
            TaskQueueData = new Dictionary<string, object>();
            Exceptions = new List<System.Exception>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public TaskExecutionContext(object eventSource, TaskEventArgs eventArgs)
        {
            EventSource = eventSource;
            EventArgs = eventArgs;
            Exceptions = new List<System.Exception>();
        }

        /// <summary>
        /// Gets or sets the event source.
        /// </summary>
        public object EventSource { get; set; }

        /// <summary>
        /// Gets or sets the event args.
        /// </summary>
        public TaskEventArgs EventArgs { get; set; }

        /// <summary>
        /// Gets or sets the task queue data for passing information along to other tasks in the queue.
        /// </summary>
        public IDictionary<string, object> TaskQueueData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the queue should cancel execution
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        public IList<System.Exception> Exceptions { get; set; }
    }
}