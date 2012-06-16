using System;

namespace Umbraco.Framework.Tasks
{
    /// <summary>
    /// When a class is decorated with this attribute it denotes that it subscribes to the specified task trigger
    /// </summary>
    /// <remarks></remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TaskAttribute : PluginAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAttribute"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the task</param>
        /// <param name="trigger">The trigger.</param>
        /// <remarks></remarks>
        public TaskAttribute(string id, string trigger)
            : base(id)
        {
            Trigger = trigger;
        }

        /// <summary>
        /// Gets the name of the trigger.
        /// </summary>
        /// <remarks></remarks>
        public string Trigger { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether a task queue execution should be continued if this task fails
        /// </summary>
        public bool ContinueOnFailure { get; set; }
    }
}