using System;
using Umbraco.Framework.Context;

namespace Umbraco.Framework.Tasks
{

    /// <summary>
    /// Abstract task for defining an installation task
    /// </summary>
    public abstract class ProviderInstallTask : AbstractTask
    {
        protected ProviderInstallTask(IFrameworkContext context) : base(context)
        {
        }

        /// <summary>
        /// Returns true if installation is required or false if it is already installed
        /// </summary>
        public abstract bool NeedsInstallOrUpgrade { get; }

        /// <summary>
        /// Gets or sets the ordinal which can be used to infer the running order when this task is in a collection.
        /// </summary>
        /// <value>The ordinal.</value>
        /// <remarks></remarks>
        public virtual int Ordinal { get; set; }

        /// <summary>
        /// Returns the currently installed version
        /// </summary>
        /// <returns></returns>
        public abstract int GetInstalledVersion();
        
        /// <summary>
        /// Performs the install or upgrade
        /// </summary>
        /// <returns></returns>
        public abstract void InstallOrUpgrade();


        #region Overrides of AbstractTask

        /// <summary>
        /// Executes this task instance.
        /// </summary>
        /// <remarks></remarks>
        public override void Execute(TaskExecutionContext context)
        {
            if (NeedsInstallOrUpgrade)
            {
                InstallOrUpgrade();
            }            
        }

        #endregion
    }
}