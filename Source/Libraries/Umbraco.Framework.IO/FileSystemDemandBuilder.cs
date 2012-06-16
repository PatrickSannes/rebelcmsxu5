using System.Configuration;
using System.Linq;
using System.Web;


using Umbraco.Framework.Configuration;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.IO.Config;
using Umbraco.Framework.Persistence.Configuration;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Framework.IO
{
    public class FileSystemDemandBuilder : AbstractProviderDependencyBuilder
    {
        public override void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");
            Mandate.ParameterNotNull(context, "context");

            var configMgr = DeepConfigManager.Default;

            var configMain = context.ConfigurationResolver
                                 .GetConfigSection(HiveConfigurationSection.ConfigXmlKey) as HiveConfigurationSection;

            if (configMain == null)
                throw new ConfigurationErrorsException(
                    string.Format("Configuration section '{0}' not found when building packaging provider '{1}'",
                                  HiveConfigurationSection.ConfigXmlKey, ProviderKey));

            var readWriteConfig = configMain.AvailableProviders.ReadWriters[ProviderKey] ?? configMain.AvailableProviders.Readers[ProviderKey];

            if (readWriteConfig == null)
                throw new ConfigurationErrorsException(
                    string.Format("No configuration found for persistence provider '{0}'", ProviderKey));


            var localConfig = DeepConfigManager
                .Default
                .GetWebSettings<ProviderConfigurationSection, ProviderConfigurationSection>(readWriteConfig.ConfigSectionKey, x => x, "~/App_Plugins")
                .First();

            if (localConfig == null)
                throw new ConfigurationErrorsException(
                    "Unable to resolve the configuration for the FileSystem repository");

            //TODO: Fix hard-coded plugin folder path --Aaron
            var supportedExtensions =
                configMgr.GetWebSetting<ProviderConfigurationSection, string>(readWriteConfig.ConfigSectionKey,
                                                                              x => x.SupportedExtensions, "~/App_Plugins");
            var rootPath = configMgr.GetWebSetting<ProviderConfigurationSection, string>(readWriteConfig.ConfigSectionKey,
                                                                                         x => x.RootPath, "~/App_Plugins");
            var excludeExtensions = configMgr.GetWebSetting<ProviderConfigurationSection, string>(readWriteConfig.ConfigSectionKey,
                                                                                         x => x.ExcludeExetensions, "~/App_Plugins");

            if (!rootPath.EndsWith("/"))
                rootPath = rootPath + "/";

            containerBuilder
                .ForFactory(x => new ProviderBootstrapper(localConfig))
                .Named<AbstractProviderBootstrapper>(ProviderKey)
                .ScopedAs.Singleton();

            containerBuilder
                .ForFactory(c => new DataContextFactory(
                                                 supportedExtensions,
                                                 c.Resolve<HttpContextBase>().Server.MapPath(rootPath),
                                                 rootPath,
                                                 excludeExtensions)
                )
                .Named<AbstractDataContextFactory>(ProviderKey)
                .ScopedAs.NewInstanceEachTime();

            containerBuilder
                .For<ReadWriteUnitOfWorkFactory>()
                .Named<IReadOnlyUnitOfWorkFactory>(ProviderKey)
                .ScopedAs.Singleton();

            containerBuilder
                .For<ReadWriteUnitOfWorkFactory>()
                .Named<IReadWriteUnitOfWorkFactory>(ProviderKey)
                .ScopedAs.Singleton();
        }

    }
}
