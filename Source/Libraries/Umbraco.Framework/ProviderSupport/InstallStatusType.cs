namespace Umbraco.Framework.ProviderSupport
{
    /// <summary>
    /// Represents the different stages of installation
    /// </summary>
    public enum InstallStatusType
    {
        /// <summary>
        /// Installation has completed
        /// </summary>
        Completed, 

        ///// <summary>
        ///// Installation has started
        ///// </summary>
        //Started,

        /// <summary>
        /// Application is awaiting being told to install, configuration is ready
        /// </summary>
        Pending,

        /// <summary>
        /// To proceed, the application requires some configuration
        /// </summary>
        RequiresConfiguration,

        /// <summary>
        /// Installation was attempted but failed
        /// </summary>
        TriedAndFailed
    }
}