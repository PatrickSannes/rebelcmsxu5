using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Configuration.Tasks
{
    public class TaskElement : ConfigurationElement, ITask
    {
        /// <summary>
        /// The string name of the task Type
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string TaskTypeName
        {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        /// <summary>
        /// The object Type of task
        /// </summary>
        public Type TaskType
        {
            get { return Type.GetType(TaskTypeName); }
        }

        /// <summary>
        /// The name of the trigger that is fired to execute this task.
        /// </summary>
        /// <remarks>
        /// It is important to know that there are 2 special triggers regarding task configuration:
        /// * post-package-intall
        /// * pre-package-uninstall
        /// 
        /// If the task element is found inside of a package folder and a configuration task has a trigger defined in 
        /// configuration of either of the 2 names above, then the task will ONLY execute for that particular packages
        /// install/uninstall actions. This is done by modifying the configuration triggers at runtime when we 
        /// detect that the configuration task is inside a package and the trigger names are changed to:
        /// 
        /// PackageFolderName-post-package-uninstall
        /// PackageFolderName-pre-package-uninstall
        /// 
        /// where PackageFolderName is the folder name of where the package resides.
        /// </remarks>
        [ConfigurationProperty("trigger", IsRequired = true)]
        public string Trigger
        {
            get
            {
                return (string)this["trigger"];
            }
            set
            {
                this["trigger"] = value;
            }
        }

        /// <summary>
        /// Set at runtime when a task is found in a package, otherwise is empty
        /// </summary>
        public string PackageFolder { get; set; }
    
        [ConfigurationCollection(typeof(TaskParameterConfigurationCollection), AddItemName = "parameter")]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = false)]
        public TaskParameterConfigurationCollection Parameters
        {
            get
            {
                return (TaskParameterConfigurationCollection)base[""];
            }
        }

        IEnumerable<ITaskParameter> ITask.Parameters
        {
            get { return Parameters.OnlyLocalConfig<ITaskParameter>(); }
        }
    }
}