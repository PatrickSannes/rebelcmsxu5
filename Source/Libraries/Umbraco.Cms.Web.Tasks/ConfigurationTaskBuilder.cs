using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration.Tasks;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Tasks;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Tasks
{
    /// <summary>
    /// Retrieves all tasks registered in configuration (mostly for packages) using the deep configuration manager
    /// </summary>
    [Task("6B1EF5B2-39C4-49D7-9458-3B366346FF56", TaskTriggers.PostAppStartup, ContinueOnFailure = false)]
    public class ConfigurationTaskBuilder : AbstractWebTask
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTaskBuilder"/> class.
        /// </summary>
        /// <param name="applicationContext">The application context.</param>
        public ConfigurationTaskBuilder(IUmbracoApplicationContext applicationContext)
            : base(applicationContext)
        {
        }

        /// <summary>
        /// This gets all tasks found in configuration blocks and dynamically creates real task definitions from them 
        /// and registers them with the task manager with the trigger name of the package they were found in 
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(TaskExecutionContext context)
        {
            using (DisposableTimer.TraceDuration<ConfigurationTaskBuilder>("Building configuration tasks", "End building configuration tasks"))
            {
                //get all tasks in configuration
                foreach (var t in ApplicationContext.Settings.Tasks)
                {
                    //create a new config task context with the configuration task's parameters
                    var configTaskContext = new ConfigurationTaskContext(ApplicationContext, t.Parameters, t);

                    //create a new instance of the configuration task
                    var task = (ConfigurationTask)Activator.CreateInstance(t.TaskType, new[] { configTaskContext });
                    var meta = new TaskMetadata(null)
                    {
                        ComponentType = task.GetType(),
                        ContinueOnError = true,
                        Id = Guid.NewGuid(),
                    };
                    //now we need to check the trigger name, if it is either post-package-install or pre-package-uninstall 
                    //then we need to check if this task has been registered with a package. If so, then we change the trigger
                    //to be PackageFolderName-post-package-install so that the task only executes for the current packages installation
                    //changes. If its neither of these names, then we just use the normal trigger
                    if ((t.Trigger.InvariantEquals(TaskTriggers.PostPackageInstall) || t.Trigger.InvariantEquals(TaskTriggers.PrePackageUninstall))
                        && !t.PackageFolder.IsNullOrWhiteSpace())
                    {
                        var parts = t.PackageFolder.Split(Path.DirectorySeparatorChar);
                        var packageName = parts[parts.Length - 1];
                        var newTriggername = packageName + "-" + t.Trigger;
                        var t1 = t;
                        LogHelper.TraceIfEnabled<ConfigurationTaskBuilder>("Found {0} config task of type {1} for package, registering it as {2}", () => t1.Trigger, () => t1.TaskType.Name, () => newTriggername);
                        //ok, we've got a task declared on a package for post package install or pre package uninstall, so need to update the trigger accordingly
                        meta.TriggerName = newTriggername;
                    }
                    else
                    {
                        //nope, this is a task declared in config that is not a package... unexpected but i guess it could happen
                        //so we just use the normal trigger that it was declared with.
                        var t1 = t;
                        LogHelper.TraceIfEnabled<ConfigurationTaskBuilder>("Found {0} config task of type {1} outside of a package", () => t1.Trigger, () => t1.TaskType.Name);
                        meta.TriggerName = t.Trigger;
                    }

                    //create a new task definition
                    var taskDefinition = new Lazy<AbstractTask, TaskMetadata>(() => task, meta);
                    //add the definition to the manager
                    ApplicationContext.FrameworkContext.TaskManager.AddTask(taskDefinition);
                }
            }
            
        }
    }
}