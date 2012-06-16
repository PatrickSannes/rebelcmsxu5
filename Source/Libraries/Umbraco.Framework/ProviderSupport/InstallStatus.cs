using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Framework.Tasks;

namespace Umbraco.Framework.ProviderSupport
{
    /// <summary>
    /// Describes the install status of a provider
    /// </summary>
    public class InstallStatus
    {
        private readonly HashSet<ProviderInstallTask> _tasksToRun = new HashSet<ProviderInstallTask>();
        
        /// <summary>
        /// By default RequiresConfiguration is the first stage of insall
        /// </summary>
        private InstallStatusType _statusType = InstallStatusType.RequiresConfiguration;
        
        private readonly HashSet<Exception> _errors = new HashSet<Exception>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InstallStatus(InstallStatusType statusType, IEnumerable<ProviderInstallTask> taskToRun, Exception error = null) : this(statusType, error)
        {
            _statusType = statusType;
            taskToRun.ForEach(x => _tasksToRun.Add(x));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InstallStatus(InstallStatusType statusType, Exception error = null)
        {
            _statusType = statusType;
            if (error != null)
                _errors.Add(error);
        }

        /// <summary>
        /// Gets or sets the install status.
        /// </summary>
        /// <value>The type of the status.</value>
        /// <remarks></remarks>
        public InstallStatusType StatusType
        {
            get { return _statusType; }
            set { _statusType = value; }
        }

        /// <summary>
        /// Gets or sets the tasks to run.
        /// </summary>
        /// <value>The tasks to run.</value>
        /// <remarks></remarks>
        public IOrderedEnumerable<ProviderInstallTask> TasksToRun { get { return _tasksToRun.OrderBy(x => x.Ordinal); } }

        public IEnumerable<Exception> Errors
        {
            get { return _errors; }
            protected set
            {
                _errors.Clear();
                value.ForEach(x => _errors.Add(x));
            }
        }
    }
}
