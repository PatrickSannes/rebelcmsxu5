using System.Configuration;

namespace Umbraco.Framework.Configuration
{
    public class DefaultConfigurationResolver : IConfigurationResolver
    {
        public object GetConfigSection(string name)
        {
            var config = ConfigurationManager.GetSection(name);
            if (config == null)
                throw new ConfigurationErrorsException(
                    string.Format("Cannot find '{0}' configuration section in application configuration.", name));
            return config;
        }
    }
}