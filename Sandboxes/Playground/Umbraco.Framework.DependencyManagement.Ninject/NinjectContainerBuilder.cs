using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace Umbraco.Framework.DependencyManagement.Ninject
{
    public class NinjectContainerBuilder : ContainerBuilderBase
    {
        private IKernel _kernel;

        public NinjectContainerBuilder(IKernel kernel)
        {
            _kernel = kernel;
        }

        public NinjectContainerBuilder()
        {
            Reset();
        }

        protected IKernel Kernel
        {
            get { return _kernel; }
        }

        public override IServiceRegistrar<TContract, TImplementation> ForType<TContract, TImplementation>()
        {
            return new NinjectServiceRegistrar<TContract, TImplementation>(Kernel);
        }

        public override IInstanceRegistrar<TContract> ForInstanceOfType<TContract>()
        {
            return new NinjectInstanceRegistrar<TContract>(Kernel);
        }

        protected override void PerformReset()
        {
            _kernel = new StandardKernel();
        }

        /// <summary>Instructs the container to build its dependency map.</summary>
        /// <returns>An <see cref="IDependencyResolver"/> which may be used for resolving services.</returns>
        public override IDependencyResolver Build()
        {
            return new NinjectResolver(Kernel);
        }

        public override IRuntimeTypeRegistrar<TContract> ForType<TContract>(Type implementation)
        {
            return new NinjectRuntimeTypeRegistrar<TContract>(implementation, Kernel);
        }

        public override IRuntimeTypeRegistrar<object> ForType(Type implementation)
        {
            return new NinjectRuntimeTypeRegistrar<object>(implementation, Kernel);
        }
    }
}
