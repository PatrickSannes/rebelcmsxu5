using System;
using System.Linq.Expressions;
using Autofac;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
	public class AutofacServiceRegistrar<TContract, TImplementation> : IServiceRegistrar<TContract, TImplementation>
		where TImplementation : TContract
	{
		private readonly AutofacContainerBuilder _masterBuilder;
		private readonly ContainerBuilder _autofacBuilder;

		internal AutofacServiceRegistrar(AutofacContainerBuilder masterBuilder, ContainerBuilder autofacBuilder)
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

		public IServiceRegistrarModifier<TContract, TImplementation> Register()
		{
			var simpleBuilder = (IServiceRegistrar<TContract, TImplementation>) this;
			var autofacRegistrar = AutofacBuilder.RegisterType<TImplementation>().As<TContract>();

			return new AutofacServiceRegistrarModifier<TContract, TImplementation>(simpleBuilder, autofacRegistrar);
		}

		public IServiceRegistrarModifier<TContract, TImplementation> Register(string name)
		{
			var simpleBuilder = (IServiceRegistrar<TContract, TImplementation>)this;
			var autofacRegistrar = AutofacBuilder.RegisterType<TImplementation>().Named<TContract>(name);
			return new AutofacServiceRegistrarModifier<TContract, TImplementation>(simpleBuilder, autofacRegistrar);
		}

	    public IInstanceRegistrarModifier<TContract> RegisterFactory(Func<IResolutionContext, TImplementation> @delegate)
		{
			var autofacRegistrar = AutofacBuilder.Register(x => @delegate.Invoke(new AutofacResolutionContext(x))).As<TContract>();

			return new AutofacInstanceRegistrarModifier<TContract, TImplementation>(autofacRegistrar);
		}

		public IInstanceRegistrarModifier<TContract> RegisterFactory(string serviceName, Func<IResolutionContext, TImplementation> @delegate)
		{
			var autofacRegistrar = AutofacBuilder.Register(x => @delegate.Invoke(new AutofacResolutionContext(x))).Named<TContract>(serviceName);

			return new AutofacInstanceRegistrarModifier<TContract, TImplementation>(autofacRegistrar);
		}
	}
}