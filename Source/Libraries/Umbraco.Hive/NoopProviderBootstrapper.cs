using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Hive
{
    public class NoopProviderBootstrapper : AbstractProviderBootstrapper
    {
        public override void ConfigureApplication(string providerKey, XDocument configXml, BendyObject installParams)
        {
            return;
        }

        public override InstallStatus GetInstallStatus()
        {
            return new InstallStatus(InstallStatusType.Completed);
        }

        public override InstallStatus TryInstall()
        {
            return new InstallStatus(InstallStatusType.Completed);
        }
    }
}
