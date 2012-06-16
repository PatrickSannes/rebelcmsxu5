using Umbraco.Framework.Localization.Configuration;

namespace Umbraco.Framework.DependencyManagement.DemandBuilders
{
    public class LocalizationSetup : IDependencyDemandBuilder
    {
        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            //TODO: Why is this done? --Aaron
            var textManager = LocalizationConfig.SetupDefault();

            containerBuilder.ForFactory(x => LocalizationConfig.TextManager)
                .ScopedAs.Singleton();
        }
    }
}
