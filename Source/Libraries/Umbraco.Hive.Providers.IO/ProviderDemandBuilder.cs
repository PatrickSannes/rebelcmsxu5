using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Framework;
using Umbraco.Framework.Configuration;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Hive.Providers.IO
{
    public class ProviderDemandBuilder : AbstractProviderDependencyBuilder
    {
        private ConfigSection _configSection;
        private Settings _settings;

        public override void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");
            Mandate.ParameterNotNull(context, "context");
        }

        public override void Initialise(IBuilderContext builderContext)
        {
            var configMain = builderContext.ConfigurationResolver
                                 .GetConfigSection(HiveConfigSection.ConfigXmlKey) as HiveConfigSection;

            if (configMain == null)
                throw new ConfigurationErrorsException(
                    string.Format("Configuration section '{0}' not found when building packaging provider '{1}'",
                                  HiveConfigSection.ConfigXmlKey, ProviderKey));

            var readWriteConfig = configMain.Providers.ReadWriters[ProviderKey] ?? configMain.Providers.Readers[ProviderKey];

            if (readWriteConfig == null)
                throw new ConfigurationErrorsException(
                    string.Format("No configuration found for persistence provider '{0}'", ProviderKey));

            var deepConfigManager = DeepConfigManager.Default;
            var localConfig = !string.IsNullOrEmpty(readWriteConfig.ConfigSectionKey)
                              ? deepConfigManager.GetFirstWebSetting<ConfigSection, ConfigSection>(readWriteConfig.ConfigSectionKey, x => x, "~/App_Plugins")
                                ??
                                readWriteConfig.GetLocalProviderConfig() as ConfigSection
                              : null;

            _configSection = localConfig;

            // ElementInformation.Source appears to be one of few ways to determine if the config-section exists or was returned on-demand with default values
            if (_configSection != null && !string.IsNullOrEmpty(_configSection.ElementInformation.Source))
            {
                CanBuild = true;
                var supportedExtensions = deepConfigManager.GetFirstWebSetting<ConfigSection, string>(readWriteConfig.ConfigSectionKey, x => x.SupportedExtensions, "~/App_Plugins");
                var rootPath = deepConfigManager.GetFirstWebSetting<ConfigSection, string>(readWriteConfig.ConfigSectionKey, x => x.RootPath, "~/App_Plugins");
                var excludedExtensions = deepConfigManager.GetFirstWebSetting<ConfigSection, string>(readWriteConfig.ConfigSectionKey, x => x.ExcludedExetensions, "~/App_Plugins");
                var excludedDirectories = deepConfigManager.GetFirstWebSetting<ConfigSection, string>(readWriteConfig.ConfigSectionKey, x => x.ExcludedDirectories, "~/App_Plugins");
                var rootPublicDomain = deepConfigManager.GetFirstWebSetting<ConfigSection, string>(readWriteConfig.ConfigSectionKey, x => x.RootPublicDomain, "~/App_Plugins");
                rootPath = rootPath.TrimEnd("/") + "/";
                _settings = new Settings(supportedExtensions, "", rootPath, excludedExtensions, excludedDirectories, rootPublicDomain);
            }
            else
            {
                LogHelper.Warn<ProviderDemandBuilder>("Cannot register dependencies for provider {0} because ProviderConfigurationSection was specified with key '{1}' but no matching configSection was found. The provider may not be installed correctly.",
                    ProviderKey,
                    readWriteConfig.ConfigSectionKey);
            }
        }

        public override Func<IResolutionContext, AbstractProviderBootstrapper> GetProviderBootstrapperFactory(IBuilderContext builderContext)
        {
            return null;
        }

        public override Func<IResolutionContext, ProviderDependencyHelper> GetProviderDependencyHelperFactory(IBuilderContext builderContext)
        {
            if (_settings != null)
            {
                return x =>
                    {
                        var resolveHttpContext = x.TryResolve<HttpContextBase>();
                        if (resolveHttpContext.Success)
                            _settings.AbsoluteRootedPath = resolveHttpContext.Value.Server.MapPath(_settings.ApplicationRelativeRoot);

                        return new DependencyHelper(_settings, x.Resolve<ProviderMetadata>(ProviderKey));
                    };
            }
            return null;
        }
    }
}
