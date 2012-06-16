using System.Web.Mvc;
using NuGet;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Framework;
using Umbraco.Framework.Tasks;

namespace Umbraco.Cms.Web.Packaging
{
    internal class PackageInstallUtility
    {
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;

        internal PackageInstallUtility(IBackOfficeRequestContext backOfficeRequestContext)
        {
            _backOfficeRequestContext = backOfficeRequestContext;
        }

        /// <summary>
        /// Runs the actions for PrePackageUninstall and for the packagefoldername specific task
        /// </summary>
        /// <param name="taskExecContext"></param>
        /// <param name="packageFolderName"></param>
        internal void RunPrePackageUninstallActions(TaskExecutionContext taskExecContext, string packageFolderName)
        {
            _backOfficeRequestContext.Application.FrameworkContext.TaskManager.ExecuteInContext(TaskTriggers.PrePackageUninstall, taskExecContext);
            _backOfficeRequestContext.Application.FrameworkContext.TaskManager.ExecuteInContext(packageFolderName + "-" + TaskTriggers.PrePackageUninstall, taskExecContext);
        }

        /// <summary>
        /// Runs package actions for either PostPackageInstall and PostPackageUninstall & the packagefoldername specific task
        /// </summary>
        /// <param name="state"></param>
        /// <param name="taskExecContext"></param>
        /// <param name="packageFolderName"></param>
        internal void RunPostPackageInstallActions(PackageInstallationState state, TaskExecutionContext taskExecContext, string packageFolderName)
        {
            //Run the post pacakge install/uninstall tasks
            var triggerType = state == PackageInstallationState.Installing
                                  ? TaskTriggers.PostPackageInstall
                                  : TaskTriggers.PostPackageUninstall;

            _backOfficeRequestContext.Application.FrameworkContext.TaskManager.ExecuteInContext(triggerType, taskExecContext);

            //Execute all  tasks registered with the package folder name... though that will only fire for packages that have just been installed obviously.
            // ...This will redirect to the appopriate controller/action if a UI task is found for this package
            _backOfficeRequestContext.Application.FrameworkContext.TaskManager.ExecuteInContext(packageFolderName + "-" + triggerType, taskExecContext);
        }

        /// <summary>
        /// Creates the task execution context for use in the Package installation actions
        /// </summary>
        /// <param name="nugetPackage"></param>
        /// <param name="packageFolderName"></param>
        /// <param name="state"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        internal TaskExecutionContext GetTaskExecutionContext(
            IPackage nugetPackage, 
            string packageFolderName, 
            PackageInstallationState state,
            Controller controller)
        {
            var package = new PackageFolder
            {
                IsNugetInstalled = nugetPackage.IsInstalled(_backOfficeRequestContext.PackageContext.LocalPackageManager.LocalRepository),
                Name = packageFolderName
            };

            return new TaskExecutionContext
            {
                EventSource = controller,
                EventArgs = new TaskEventArgs(_backOfficeRequestContext.Application.FrameworkContext,
                    new PackageInstallEventArgs(nugetPackage.Id, package, state))
            };
        }

    }
}
