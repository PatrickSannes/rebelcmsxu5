namespace Umbraco.Cms.Web
{
    using Umbraco.Cms.Web.Context;
    using Umbraco.Framework;
    using Umbraco.Framework.ProviderSupport;
    using global::System;
    using global::System.Linq;

    public static class UmbracoApplicationContextExtensions
    {
        #region Public Methods

        /// <summary>
        ///   Returns true if all providers have a Completed installation status
        /// </summary>
        /// <param name="appContext"> </param>
        /// <returns> </returns>
        public static bool AllProvidersInstalled(this IUmbracoApplicationContext appContext)
        {
            return appContext.FrameworkContext.ApplicationCache.GetOrCreate(
                "all-providers-installed",
                () =>
                    {
                        var isInstalled = appContext.GetInstallStatus().All(status => status.StatusType == InstallStatusType.Completed);

                        // If not all providers are installed, basically don't cache it
                        var cacheTime = isInstalled ? TimeSpan.FromMinutes(2) : TimeSpan.FromSeconds(0.1d);
                        return new HttpRuntimeCacheParameters<bool>(isInstalled) { SlidingExpiration = cacheTime };
                    });
        }

        public static bool AnyProvidersHaveStatus(this IUmbracoApplicationContext appContext, InstallStatusType status)
        {
            return appContext.GetInstallStatus().Any(s => s.StatusType == status);
        }

        #endregion
    }
}