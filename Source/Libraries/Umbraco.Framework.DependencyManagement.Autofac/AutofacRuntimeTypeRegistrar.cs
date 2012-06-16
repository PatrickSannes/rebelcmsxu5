using System;
using Autofac;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
    public class AutofacRuntimeTypeRegistrar<TContract> : IRuntimeTypeRegistrar<TContract>
    {
        private readonly AutofacContainerBuilder _masterBuilder;
        private readonly ContainerBuilder _autofacBuilder;
        private readonly Type _implementationType;

        internal AutofacRuntimeTypeRegistrar(AutofacContainerBuilder masterBuilder, ContainerBuilder autofacBuilder, Type implementationType)
        {
            _implementationType = implementationType;
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

        protected internal Type ImplementationType
        {
            get { return _implementationType; }
        }

        public virtual IRuntimeTypeRegistrarModifier<TContract> Register()
        {
            var simpleBuilder = (IRuntimeTypeRegistrar<TContract>)this;
            var autofacRegistrar = AutofacBuilder.RegisterType(ImplementationType).As<TContract>();

            return new AutofacRuntimeTypeRegistrarModifier<TContract>(simpleBuilder, autofacRegistrar);
        }

        public virtual IRuntimeTypeRegistrarModifier<TContract> RegisterNamed(string name)
        {
            var simpleBuilder = (IRuntimeTypeRegistrar<TContract>)this;
            var autofacRegistrar = AutofacBuilder.RegisterType(ImplementationType).Named<TContract>(name);
            return new AutofacRuntimeTypeRegistrarModifier<TContract>(simpleBuilder, autofacRegistrar);
        }
    }
}