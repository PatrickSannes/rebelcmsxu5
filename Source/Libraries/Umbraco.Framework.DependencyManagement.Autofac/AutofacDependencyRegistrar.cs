using System;
using System.Linq.Expressions;
using Autofac;
using Autofac.Builder;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
    internal class AutofacDependencyRegistrar<T> : IDependencyRegistrar<T>
    {
        private readonly IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> _registration;
        private AutofacScopeManager<T> _autofacScopeManager;

        public AutofacDependencyRegistrar(IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            _registration = registration;
        }

        public IDependencyRegistrar<T> KnownAs(Type type)
        {
            _registration.As(type);
            return this;
        }

        public IDependencyRegistrar<T> KnownAs<TContract>()
        {
            _registration.As<TContract>();
            return this;
        }

        public IDependencyRegistrar<T> Named<TContract>(string name)
        {
            _registration.Named<TContract>(name);
            return this;
        }

        public IDependencyRegistrar<T> Named(string name, Type type)
        {
            _registration.Named(name, type);
            return this;
        }

        public IScopeManager<T> ScopedAs
        {
            get { return _autofacScopeManager ?? (_autofacScopeManager = new AutofacScopeManager<T>(_registration, this)); }
        }

        public Type RawType
        {
            get { return _registration.ActivatorData.Activator.LimitType; }
        }

        public IDependencyRegistrar<T> OnActivated(Action<IResolutionContext, T> action)
        {
            _registration.OnActivated(x => action.Invoke(new AutofacResolutionContext(x.Context), x.Instance));
            return this;
        }

        private void CheckValidForParameter()
        {
            if (!TypeFinder.IsTypeAssignableFrom<IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>>(_registration.GetType()))
            {
                throw new TypeAccessException("{0} is not assignable from {1}".InvariantFormat(typeof(IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>).Name, _registration.GetType().Name));
            }
        }

        public IDependencyRegistrar<T> WithResolvedParam<TParam>(Func<IResolutionContext, TParam> callback)
        {
            CheckValidForParameter();

            ((IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>)_registration)
                .WithParameter(AutofacContainerBuilder.GenerateResolvedParameter(callback));
            return this;
        }

        public IDependencyRegistrar<T> WithMetadata<TMetadata, TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value)
        {
            _registration.WithMetadata<TMetadata>(config => config.For(propertyAccessor, value));
            return this;
        }

        public IDependencyRegistrar<T> WithNamedParam(string paramterName, object value)
        {
            CheckValidForParameter();

            ((IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>)_registration)
                .WithParameter(paramterName, value);
            return this;
        }
    }
}