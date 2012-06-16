using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Umbraco.Framework.Context;

namespace Umbraco.Framework.Tasks
{
    /// <summary>
    /// Represents a declarative task that runs a provided delegate when told to execute
    /// </summary>
    /// <remarks></remarks>
    public class DelegateTask : AbstractTask
    {
        private readonly Action<TaskExecutionContext> _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateTask"/> class. Note that since this constructor has
        /// no <see cref="IFrameworkContext"/> parameter, the <see cref="Context"/> property of the base class
        /// <see cref="AbstractTask"/> will be null until the <see cref="Execute"/> method is called at which point
        /// the <see cref="TaskExecutionContext.EventArgs.FrameworkContext"/> property will be used to set it.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <remarks></remarks>
        public DelegateTask(Action<TaskExecutionContext> callback)
            : this(null, callback)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateTask"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="callback">The callback.</param>
        /// <remarks></remarks>
        public DelegateTask(IFrameworkContext context, Action<TaskExecutionContext> callback)
            : base(context)
        {
            _callback = callback;
        }

        #region Overrides of AbstractTask

        /// <summary>
        /// Executes this task instance.
        /// </summary>
        /// <remarks></remarks>
        public override void Execute(TaskExecutionContext context)
        {
            // Set the context here if it's null as sometimes we don't accept it in our constructor
            Context = Context ?? context.EventArgs.FrameworkContext;
            _callback.Invoke(context);
        }

        #endregion
    }
}
