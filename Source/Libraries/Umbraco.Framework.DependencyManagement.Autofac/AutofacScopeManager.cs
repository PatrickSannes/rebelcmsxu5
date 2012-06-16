using Autofac.Builder;
using Autofac.Integration.Mvc;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
    internal class AutofacScopeManager<T> : IScopeManager<T>
    {
        private readonly IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> _registration;
        private readonly AutofacDependencyRegistrar<T> _autofacDependencyRegistrar;

        public AutofacScopeManager(IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> registration, AutofacDependencyRegistrar<T> autofacDependencyRegistrar)
        {
            _registration = registration;
            _autofacDependencyRegistrar = autofacDependencyRegistrar;
        }

        public IDependencyRegistrar<T> Singleton()
        {
            _registration.SingleInstance();
            return _autofacDependencyRegistrar;
        }

        public IDependencyRegistrar<T> ForLifetime(string lifetimeName)
        {
            _registration.InstancePerMatchingLifetimeScope(lifetimeName);
            return _autofacDependencyRegistrar;
        }

        public IDependencyRegistrar<T> NewInstanceEachTime()
        {
            _registration.InstancePerDependency();
            return _autofacDependencyRegistrar;
        }

        public IDependencyRegistrar<T> HttpRequest()
        {
            _registration.InstancePerHttpRequest();
            return _autofacDependencyRegistrar;
        }
    }
}