using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Framework.Security;
using Umbraco.Framework.Tasks;
using Umbraco.Hive;

namespace Umbraco.Cms.Web.Context
{
    public class UmbracoApplicationContext : DisposableObject, IUmbracoApplicationContext
    {

        /// <summary>
        /// This will restart the application pool
        /// </summary>
        /// <param name="http"></param>
        public static void RestartApplicationPool(HttpContextBase http)
        {
            //NOTE: this only works in full trust :(
            //HttpRuntime.UnloadAppDomain();
            //so we'll do the dodgy hack instead            
            var configPath = http.Request.PhysicalApplicationPath + "\\web.config";
            File.SetLastWriteTimeUtc(configPath, DateTime.UtcNow);
        }

        private Guid? _appId = null;

        public UmbracoApplicationContext(
            //ICmsHiveManager hive, 
            //HiveManager hive, 
            UmbracoSettings settings, 
            IFrameworkContext frameworkContext,
            ISecurityService securityService)
        {
            //Hive = hive;
            Settings = settings;
            FrameworkContext = frameworkContext;
            Security = securityService;
            //Hive2 = hive2;
            //Hive2 = DependencyResolver.Current.GetService<HiveManager>();

            //TODO: Use this cache mechanism! But in order to do so , we need triggers working from Hive providers, currently they are not setup

            //clear our status cache when any hive install status changes
            frameworkContext.TaskManager.AddDelegateTask(
                TaskTriggers.Hive.InstallStatusChanged, 
                x => _installStatuses = null);
        }

        private IEnumerable<InstallStatus> _installStatuses;
        /// <summary>
        /// Return the installation status, this is based on all hive providers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InstallStatus> GetInstallStatus()
        {
            //TODO: Use this cache mechanism! But in order to do so , we need triggers working from Hive providers, currently they are not setup

            //return _installStatuses ?? (_installStatuses = Hive.GetAllReadWriteProviders()
            //                                                   .Select(x => x.ProviderContext.Bootstrapper.GetInstallStatus())
            //                                                   .ToArray());

            var allReadWriteProviders = Hive.GetAllReadWriteProviders();
            var allBootstrappers =
                allReadWriteProviders.Select(x => new {Key = x.ProviderMetadata.Alias, x.Bootstrapper}).
                    ToArray();

            var installStatuses = allBootstrappers
                .Select(x => x.Bootstrapper.GetInstallStatus())
                .ToArray();
            return installStatuses;
        }

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; private set; }

        

        /// <summary>
        /// Gets the application id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        public Guid ApplicationId
        {
            get
            {
                if (_appId == null)
                    _appId = Guid.NewGuid();
                return _appId.Value;
            }
        }


        ///// <summary>
        ///// Gets an insance of <see cref="ICmsHiveManager"/> associated with this application.
        ///// </summary>
        ///// <value>The hive.</value>
        //public ICmsHiveManager Hive { get; private set; }

        // Note: this is done this dirty way because Autofac appears to think that multiple resolve operations for IUmbracoApplicationContext
        // in the chain of service activation is a circular reference. Adding HiveManager as a ctor param on this type is one of a few ways to trigger it.
        // I found that it was because of our IoC abstraction passing a reference to the temporary IComponentContext service to factory delegates rather
        // than resolving a new IComponentContext on demand, but now enabling the ctor param on this type gives a very bizarre error "item with the same key has already been added"
        // from within Autofac.Core.Lifetime.LifetimeScope.GetOrCreateAndShare even though its code appears to contain a lock and the key is an Autofac-generated Guid.
        private readonly Lazy<IHiveManager> _resolveHiveManager =
            new Lazy<IHiveManager>(() => DependencyResolver.Current.GetService<IHiveManager>());

        public IHiveManager Hive
        {
            get { return _resolveHiveManager.Value; }
        }

        //public HiveManager Hive { get; protected set; }

        /// <summary>
        /// Gets the settings associated with this Umbraco application.
        /// </summary>
        /// <value>The settings.</value>
        public UmbracoSettings Settings { get; private set; }

        /// <summary>
        /// Gets the security service.
        /// </summary>
        public ISecurityService Security { get; private set; }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            Security.IfNotNull(x => x.DisposeIfDisposable());
            Settings.IfNotNull(x => x.DisposeIfDisposable());
            Hive.IfNotNull(x => x.DisposeIfDisposable());
            FrameworkContext.IfNotNull(x => x.Dispose());
        }

        #endregion
    }
}