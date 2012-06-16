
using Umbraco.Framework.Configuration;
using Umbraco.Framework.Context;
using System.Globalization;

namespace Umbraco.Framework.DependencyManagement.DemandBuilders
{
    public class FrameworkSetupHttpLifetime : IDependencyDemandBuilder
    {
        #region Implementation of IDependencyDemandBuilder

        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            containerBuilder.For<DefaultConfigurationResolver>()
                .KnownAs<IConfigurationResolver>()
                .ScopedAs.Singleton();

            new LocalizationSetup().Build(containerBuilder, context);

            containerBuilder.For<DefaultFrameworkContext>().KnownAs<IFrameworkContext>()
                .ScopedAs.HttpRequest();
        }

        #endregion
    }
}