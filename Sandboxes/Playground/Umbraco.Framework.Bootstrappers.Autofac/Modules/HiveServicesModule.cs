using System.Diagnostics.Contracts;
using Autofac;

namespace Umbraco.Framework.Bootstrappers.Autofac.Modules
{
	public class HiveServicesModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			Contract.Assert(builder != null, "Builder container is null");

			builder.RegisterType<HiveRepository>()
				.As<HiveRepository>()
					//TODO: Inject DependencyResolver but fix circular dependency on the fact these modules
					//get loaded during the setting of DependencyResolver.Current....
					//.WithParameter(new TypedParameter(typeof(IDependencyResolver), DependencyResolver.Current))
				.SingleInstance();
		}
	}
}
