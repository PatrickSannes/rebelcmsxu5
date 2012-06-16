using System;
using System.Linq.Expressions;
using Autofac.Builder;
using Autofac.Integration.Mvc;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
	public class AutofacInstanceRegistrarModifier<TContract, TImplementation> : IInstanceRegistrarModifier<TContract> where TImplementation : TContract
	{
		private readonly IRegistrationBuilder<TImplementation, SimpleActivatorData, SingleRegistrationStyle> _containerRegistrationBuilder;

		internal AutofacInstanceRegistrarModifier(IRegistrationBuilder<TImplementation, SimpleActivatorData, SingleRegistrationStyle> containerRegistrationBuilder)
		{
			_containerRegistrationBuilder = containerRegistrationBuilder;
		}

		public IInstanceRegistrarModifier<TContract> ScopedAsSingleton()
		{
			_containerRegistrationBuilder.SingleInstance();
			return this;
		}

		public IInstanceRegistrarModifier<TContract> ScopedWithNestedLifetime()
		{
			_containerRegistrationBuilder.InstancePerLifetimeScope();
			return this;
		}

		public IInstanceRegistrarModifier<TContract> ScopedPerHttpRequest()
		{
			_containerRegistrationBuilder.InstancePerHttpRequest();
			return this;
		}

		public IInstanceRegistrarModifier<TContract> ExternallyOwned()
		{
			_containerRegistrationBuilder.ExternallyOwned();
			return this;
		}

	    public IInstanceRegistrarModifier<TContract> WithMetadata<TMetadata, TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value)
	    {
            _containerRegistrationBuilder.WithMetadata<TMetadata>(
              new Action<MetadataConfiguration<TMetadata>>(config => config.For(propertyAccessor, value)));
            return this;
	    }

        public IInstanceRegistrarModifier<TContract> AfterActivate(Action<IResolutionContext, TContract> callback)
        {
            _containerRegistrationBuilder.OnActivated(x => callback.Invoke(new AutofacResolutionContext(x.Context), x.Instance));
            return this;
        }
	}
}