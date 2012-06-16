using System.Configuration;

namespace Umbraco.Hive.Providers.Membership.Config
{
    [ConfigurationCollection(typeof(ProviderElement))]
    public class ProviderCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderElement)element).Name;
        }
    }
}