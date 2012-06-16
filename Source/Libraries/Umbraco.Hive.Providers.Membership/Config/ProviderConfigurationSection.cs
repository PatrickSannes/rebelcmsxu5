using System.Configuration;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Hive.Providers.Membership.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class ProviderConfigurationSection : AbstractProviderConfigurationSection
    {
        [ConfigurationProperty("providers")]
        public ProviderCollection MembershipProviders
        {
            get { return (ProviderCollection) this["providers"]; }
            set { this["providers"] = value; }
        }
    }
}
