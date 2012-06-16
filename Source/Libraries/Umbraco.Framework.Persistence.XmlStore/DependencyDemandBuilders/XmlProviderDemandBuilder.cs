
using System.Web.Hosting;

using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.XmlStore.DataManagement;
using Umbraco.Framework.Persistence.XmlStore.DataManagement.ReadWrite;

namespace Umbraco.Framework.Persistence.XmlStore.DependencyDemandBuilders
{
    public class XmlProviderDemandBuilder : AbstractProviderDependencyBuilder
    {
        public XmlProviderDemandBuilder()
        {
        }

        public XmlProviderDemandBuilder(string providerKey)
        {
            ProviderKey = providerKey;
        }

        /// <summary>
        /// Builds the dependency demands required by this implementation.
        /// </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="builderContext">The builder context.</param>
        public override void Build(IContainerBuilder containerBuilder, IBuilderContext builderContext)
        {
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");
            Mandate.ParameterNotNull(builderContext, "builderContext");

            // Configure type injection for this provider's implementation of the main interfaces
            containerBuilder.ForFactory(context => new DataContextFactory(HostingEnvironment.MapPath("~/App_Data/umbraco.config")))
                .Named<AbstractDataContextFactory>(ProviderKey)
                .ScopedAs.Singleton();
            
            containerBuilder.For<ReadOnlyUnitOfWorkFactory>()
                .Named<IReadOnlyUnitOfWorkFactory>(ProviderKey);

            containerBuilder.For<ReadWriteUnitOfWorkFactory>()
                .Named<IReadWriteUnitOfWorkFactory>(ProviderKey);
        }

       
    }
}