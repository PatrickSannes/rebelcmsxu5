using System;
using System.Xml.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Hive.Providers.Membership.Config;

namespace Umbraco.Hive.Providers.Membership.Hive
{
    public class ProviderBootstrapper : AbstractProviderBootstrapper
    {
        private readonly ProviderConfigurationSection _existingConfig;        
        private readonly IFrameworkContext _frameworkContext;
        private readonly InstallStatus _installStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderBootstrapper"/> class if insufficient configuration information is yet available.
        /// </summary>
        /// <param name="installStatus">The install status.</param>
        /// <remarks></remarks>
        public ProviderBootstrapper(InstallStatus installStatus)
        {
            _installStatus = installStatus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderBootstrapper"/> class if sufficient configuration information has been supplied by the user.
        /// </summary>
        /// <param name="existingConfig">The existing config.</param>
        /// <param name="frameworkContext"></param>
        /// <remarks></remarks>
        public ProviderBootstrapper(ProviderConfigurationSection existingConfig, IFrameworkContext frameworkContext)
        {
            _existingConfig = existingConfig;
            _frameworkContext = frameworkContext;

        
        }

        public override void ConfigureApplication(string providerKey, XDocument configXml, BendyObject installParams)
        {
            //TODO: Setup config
        }

        public override InstallStatus GetInstallStatus()
        {
            if (_installStatus != null) return _installStatus;
            if (_existingConfig == null) return new InstallStatus(InstallStatusType.RequiresConfiguration);

            return new InstallStatus(InstallStatusType.Completed);
        }

        public override InstallStatus TryInstall()
        {            
            return new InstallStatus(InstallStatusType.Completed);
        }
    }
}