namespace Umbraco.Framework
{
    /// <summary>
    /// A helper class listing common constant trigger aliases
    /// </summary>
    /// <remarks></remarks>
    public static class TaskTriggers
    {
        public static class Hive
        {
            public const string PreAddOrUpdateOnUnitComplete = "hive-pre-add-or-update-after-unit-complete";
            public const string PostAddOrUpdateOnUnitComplete = "hive-post-add-or-update-after-unit-complete";
            //public const string PreDelete = "hive-pre-delete";
            //public const string PostDelete = "hive-post-delete";
            public const string PostReadEntity = "hive-item-available";
            public const string PostQueryResultsAvailable = "hive-post-query-results-available";
            public const string PostInstall = "post-hive-install";
            public const string InstallStatusChanged = "hive-install-status-changed";
            //public const string PreAddOrUpdateRevision = "hive-pre-add-or-update-revision";
            //public const string PostAddOrUpdateRevision = "hive-post-add-or-update-revision";
            public const string PreAddOrUpdate = "hive-pre-add-or-update-before-unit-complete";
            public const string PostAddOrUpdate = "hive-post-add-or-update-before-unit-complete";

            public static class Relations
            {
                public const string PreRelationAdded = "hive-pre-add-relation-before-unit-complete";
                public const string PostRelationAdded = "hive-post-add-relation-before-unit-complete";
                public const string PreRelationRemoved = "hive-pre-remove-relation-before-unit-complete";
                public const string PostRelationRemoved = "hive-post-remove-relation-before-unit-complete";
            }
        }

        public const string GlobalError = "global-error";
        public const string PreAppStartupComplete = "pre-app-startup-complete";
        public const string PostAppStartup = "post-app-startup-complete";
        
        public const string PostPackageInstall = "post-package-install";
        public const string PostPackageUninstall = "post-package-uninstall";
        public const string PrePackageUninstall = "pre-package-uninstall";

        public const string ModelPreSendToView = "model-pre-send-to-view";

        
        /// <summary>
        /// Produces a trigger name prefixed with the name of type <typeparamref name="T"/>, useful
        /// for callers who want to register or execute a trigger name specific to their type or namespace region.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="triggerName">Name of the trigger.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string SafeTriggerName<T>(string triggerName)
        {
            return typeof (T).FullName + "-" + triggerName;
        }
    }
}
