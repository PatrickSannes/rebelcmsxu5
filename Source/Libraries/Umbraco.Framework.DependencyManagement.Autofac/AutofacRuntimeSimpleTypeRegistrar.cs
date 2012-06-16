using System;
using Autofac;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
    public class AutofacRuntimeSimpleTypeRegistrar : AutofacRuntimeTypeRegistrar<object>
    {
        internal AutofacRuntimeSimpleTypeRegistrar(AutofacContainerBuilder masterBuilder, ContainerBuilder autofacBuilder, Type implementationType) 
            : base(masterBuilder, autofacBuilder, implementationType)
        {
        }

        public override IRuntimeTypeRegistrarModifier<object> Register()
        {
            var simpleBuilder = (IRuntimeTypeRegistrar<object>)this;
            var autofacRegistrar = AutofacBuilder.RegisterType(ImplementationType);

            return new AutofacRuntimeTypeRegistrarModifier<object>(simpleBuilder, autofacRegistrar);
        }

        public override IRuntimeTypeRegistrarModifier<object> RegisterNamed(string name)
        {
            var simpleBuilder = (IRuntimeTypeRegistrar<object>)this;
            var autofacRegistrar = AutofacBuilder.RegisterType(ImplementationType).Named<object>(name);
            return new AutofacRuntimeTypeRegistrarModifier<object>(simpleBuilder, autofacRegistrar);
        }
    }
}