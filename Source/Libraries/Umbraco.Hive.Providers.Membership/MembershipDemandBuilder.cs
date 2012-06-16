using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Web;
using System.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Configuration;
using Umbraco.Framework.Context;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.Providers.Membership.Config;
using Umbraco.Hive.Providers.Membership.Hive;
using Umbraco.Hive.Providers.Membership.Mapping;
using ProviderMetadata = Umbraco.Framework.Persistence.ProviderSupport._Revised.ProviderMetadata;

namespace Umbraco.Hive.Providers.Membership
{
    /// <summary>
    /// Adds all of the Examine Hive dependencies to the IoC Container
    /// </summary>
    public class MembershipDemandBuilder : AbstractProviderDependencyBuilder
    {
        private ProviderConfigurationSection _localConfig;

        /// <summary>
        /// Initializes the provider and ensures that all configuration can be read
        /// </summary>
        /// <param name="builderContext"></param>
        public override void Initialise(IBuilderContext builderContext)
        {
            Mandate.ParameterNotNull(builderContext, "builderContext");

            var configMain = builderContext.ConfigurationResolver.GetConfigSection(HiveConfigSection.ConfigXmlKey) as HiveConfigSection;

            if (configMain == null)
                throw new ConfigurationErrorsException(
                    string.Format("Configuration section '{0}' not found when building packaging provider '{1}'",
                                  HiveConfigSection.ConfigXmlKey, ProviderKey));

            var config2Rw = configMain.Providers.ReadWriters[ProviderKey] ?? configMain.Providers.Readers[ProviderKey];

            if (config2Rw == null)
                throw new ConfigurationErrorsException(
                    string.Format("No configuration found for persistence provider '{0}'", ProviderKey));

            //get the Hive provider config section
            _localConfig = DeepConfigManager.Default.GetFirstPluginSection<ProviderConfigurationSection>(config2Rw.ConfigSectionKey);

            if (!ValidateProviderConfigSection<MembershipDemandBuilder>(_localConfig, config2Rw))
            {
                CanBuild = false;
                return;
            }

            CanBuild = true;
        }

        public override void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            //register the type mapper
            containerBuilder
                .For<MembershipWrapperModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();          
        }

        public override Func<IResolutionContext, AbstractProviderBootstrapper> GetProviderBootstrapperFactory(IBuilderContext builderContext)
        {
            if (!CanBuild)
            {
                return null;
            }
            return x => new ProviderBootstrapper(_localConfig, x.Resolve<IFrameworkContext>());
        }

        public override Func<IResolutionContext, ProviderDependencyHelper> GetProviderDependencyHelperFactory(IBuilderContext builderContext)
        {
            if (_localConfig != null)
            {
                //NOTE: This must be lazy call because we cannot initialize membership providers now as exceptions will occur
                // because our Hive membersip provider hasn't been initialized yet, plus its a bit more performant.
                return x => new DependencyHelper(
                    _localConfig.MembershipProviders.Cast<ProviderElement>(),
                    new Lazy<IEnumerable<MembershipProvider>>(() =>
                    {
                        var providers = new List<MembershipProvider>();
                        var castedProviders = global::System.Web.Security.Membership.Providers.Cast<MembershipProvider>();
                        //get reference to all membership providers referenced in our config
                        foreach (var m in _localConfig.MembershipProviders.Cast<ProviderElement>())
                        {
                            var found = castedProviders.SingleOrDefault(provider => provider.Name == m.Name);
                            if (found != null)
                            {
                                providers.Add(found);
                            }
                            else
                            {
                                LogHelper.Warn<MembershipDemandBuilder>("Could not load a MembershipProvider with the name " + m.Name);
                            }
                        }
                        return providers;
                    }), x.Resolve<ProviderMetadata>(ProviderKey));
            }
            return null;
        }
    }
}