using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Framework.Configuration;
using Umbraco.Framework.Context;
using System.Globalization;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.DependencyManagement.DemandBuilders
{
    public class FrameworkSetup : IDependencyDemandBuilder
    {
        #region Implementation of IDependencyDemandBuilder

        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            containerBuilder.For<DefaultConfigurationResolver>()
                .KnownAs<IConfigurationResolver>().ScopedAs.Singleton();

            new LocalizationSetup().Build(containerBuilder, context);

            containerBuilder.For<DefaultFrameworkContext>().KnownAs<IFrameworkContext>().ScopedAs.Singleton();
            
            containerBuilder
                .For<MappingEngineCollection>()
                .KnownAsSelf()
                .OnActivated((ctx, x) => x.Configure()) //once it's created, then we call Configure
                .ScopedAs.Singleton();
        }

        #endregion
    }
}
