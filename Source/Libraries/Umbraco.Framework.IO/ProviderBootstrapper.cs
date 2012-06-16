using System.Xml.Linq;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.IO.Config;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Framework.IO
{
    public class ProviderBootstrapper : AbstractProviderBootstrapper
    {
        private readonly ProviderConfigurationSection _existingConfig;
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
        /// <remarks></remarks>
        public ProviderBootstrapper(ProviderConfigurationSection existingConfig)
        {
            _existingConfig = existingConfig;
        }

        public override void ConfigureApplication(string providerKey, string providerAlias, XDocument configXml, BendyObject installParams)
        {
            
        }

        public override InstallStatus GetInstallStatus()
        {
            if (_installStatus != null) return _installStatus;
            if (_existingConfig == null) return new InstallStatus(InstallStatusType.RequiresConfiguration);

            // TODO Install-check logic if any
            return new InstallStatus(InstallStatusType.Completed);
        }

        public override InstallStatus TryInstall()
        {
            // TODO Install logic if any
            return new InstallStatus(InstallStatusType.Completed);
        }
    }
}