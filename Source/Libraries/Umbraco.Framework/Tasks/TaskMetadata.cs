using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Umbraco.Framework.Tasks
{
    /// <summary>
    /// Outlines metadata for the registration of Tasks with MEF
    /// </summary>
    /// <remarks></remarks>
    public class TaskMetadata : PluginMetadataComposition 
    {
        /// <summary>
        /// constructor, sets all properties of this object based on the dictionary values
        /// </summary>
        /// <param name="obj"></param>
        public TaskMetadata(IDictionary<string, object> obj) : base(obj)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskMetadata"/> class.
        /// </summary>
        /// <param name="triggerName">Name of the trigger.</param>
        /// <param name="continueOnError">if set to <c>true</c> [continue on error].</param>
        /// <remarks></remarks>
        public TaskMetadata(string triggerName, bool continueOnError) : base(null)
        {
            TriggerName = triggerName;
            ContinueOnError = continueOnError;
        }      

        /// <summary>
        /// Gets or sets the name of the trigger to which the task should subscribe.
        /// </summary>
        /// <value>The name of the trigger.</value>
        /// <remarks></remarks>
        public string TriggerName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to continue if an error is encountered running the associated task
        /// </summary>
        public bool ContinueOnError { get; set; }
    }
}
