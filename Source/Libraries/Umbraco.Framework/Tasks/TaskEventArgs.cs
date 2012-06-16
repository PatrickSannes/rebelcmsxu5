using System;
using Umbraco.Framework.Context;

namespace Umbraco.Framework.Tasks
{
    /// <summary>
    /// Contains task event data and an <see cref="IFrameworkContext"/>.
    /// </summary>
    /// <remarks></remarks>
    public class TaskEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.EventArgs"/> class.
        /// </summary>
        public TaskEventArgs(IFrameworkContext frameworkContext, EventArgs callerEventArgs)
        {
            FrameworkContext = frameworkContext;
            CallerEventArgs = callerEventArgs;
        }

        /// <summary>
        /// Gets or sets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; protected set; }

        /// <summary>
        /// Gets or sets the caller event args.
        /// </summary>
        /// <value>The caller event args.</value>
        /// <remarks></remarks>
        public EventArgs CallerEventArgs { get; protected set; }
    }
}