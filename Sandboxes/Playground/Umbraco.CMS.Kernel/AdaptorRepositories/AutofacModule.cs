using Autofac;
using Umbraco.Framework.EntityGraph.Domain.EntityAdaptors;

namespace Umbraco.CMS.Kernel.AdaptorRepositories
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ContentAdaptor>()
                .As<IContentResolver>()
                .InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
