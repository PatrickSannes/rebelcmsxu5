using Autofac;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
	public class AutofacInstanceRegistrar<TContract> : IInstanceRegistrar<TContract>
	{

		private readonly AutofacContainerBuilder _masterBuilder;
		private readonly ContainerBuilder _autofacBuilder;

		internal AutofacInstanceRegistrar(AutofacContainerBuilder masterBuilder, ContainerBuilder autofacBuilder)
		{
			_autofacBuilder = autofacBuilder;
			_masterBuilder = masterBuilder;
		}

		protected internal AutofacContainerBuilder MasterBuilder
		{
			get { return _masterBuilder; }
		}

		protected internal ContainerBuilder AutofacBuilder
		{
			get { return _autofacBuilder; }
		}

		public IInstanceRegistrarModifier<TContract> Register<TImplementation>(TImplementation instance) where TImplementation : class, TContract
		{
			var simpleBuilder = (IInstanceRegistrar<TContract>)this;
			var autofacRegistrar = AutofacBuilder.RegisterInstance(instance).As<TContract>();

			return new AutofacInstanceRegistrarModifier<TContract, TImplementation>(autofacRegistrar);
		}

		public IInstanceRegistrarModifier<TContract> Register<TImplementation>(TImplementation instance, string name) where TImplementation : class, TContract
		{
			var simpleBuilder = (IInstanceRegistrar<TContract>)this;
			var autofacRegistrar = AutofacBuilder.RegisterInstance(instance).Named<TContract>(name);

			return new AutofacInstanceRegistrarModifier<TContract, TImplementation>(autofacRegistrar);
		}
	}
}