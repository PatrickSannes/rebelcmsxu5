using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Umbraco.Framework.Context;

namespace Umbraco.Framework.Tasks
{
    
    /// <summary>
    /// Represents a task that can be executed
    /// </summary>
    public abstract class AbstractTask
    {
        protected AbstractTask(IFrameworkContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Gets a reference to the <see cref="IFrameworkContext"/>.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext Context { get; protected set; }

        /// <summary>
        /// Executes this task instance.
        /// </summary>
        /// <remarks></remarks>
        public abstract void Execute(TaskExecutionContext context);
    }
}
