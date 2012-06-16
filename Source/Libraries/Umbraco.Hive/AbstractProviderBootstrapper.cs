using System.Xml.Linq;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Hive
{
    /// <summary>
    /// An abstraction of a bootstrapper implemented by providers in order to handle any install tasks
    /// </summary>
    /// <remarks></remarks>
    public abstract class AbstractProviderBootstrapper
    {

        /// <summary>
        /// Creates any necessary configuration files/transforms for the provider to operate
        /// </summary>
        /// <param name="configXml">The configuration xml file that needs to be written to</param>
        /// <param name="installParams">
        /// TODO: This is only a temporary way of passing arbitrary parameters to a provider to create its configuration,
        /// we need to allow hive providers to return a model for which we display a form/installer for and then pass in that
        /// model to the installParams
        /// </param>
        /// <param name="providerKey">The provider key for the provider that is being configured</param>
        public abstract void ConfigureApplication(string providerKey, XDocument configXml, BendyObject installParams);

        /// <summary>
        /// Gets the current installation status of the provider.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract InstallStatus GetInstallStatus();

        /// <summary>
        /// Attempts to run installation tasks and reports status.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract InstallStatus TryInstall();
    }
}