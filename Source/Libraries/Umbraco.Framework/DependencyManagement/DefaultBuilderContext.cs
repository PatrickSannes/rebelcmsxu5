
using Umbraco.Framework.Configuration;

namespace Umbraco.Framework.DependencyManagement
{
    public class DefaultBuilderContext : IBuilderContext
    {
        private readonly IConfigurationResolver _configurationResolver;

        public DefaultBuilderContext(IConfigurationResolver configurationResolver)
        {
            _configurationResolver = configurationResolver;
        }

        public DefaultBuilderContext()
        {
            _configurationResolver = new DefaultConfigurationResolver();
        }

        public IConfigurationResolver ConfigurationResolver { get { return _configurationResolver; } }
    }
}