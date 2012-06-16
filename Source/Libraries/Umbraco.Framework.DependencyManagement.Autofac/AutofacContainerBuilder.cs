using System;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace Umbraco.Framework.DependencyManagement.Autofac
{
    /// <summary>A specific implementation of <see cref="IContainerBuilder"/> using Autofac. </summary>
    /// <remarks>Doc updated, 14-Jan-2011.</remarks>
    public class AutofacContainerBuilder : AbstractContainerBuilder
    {
        ContainerBuilder _builder;
        private IContainer _autofacContainer;

        public AutofacContainerBuilder()
        {
            Reset();
        }

        public AutofacContainerBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public override IDependencyRegistrar<object> For(Type type)
        {
            var registration = _builder.RegisterType(type);
            return new AutofacDependencyRegistrar<object>(registration);
        }

        public override IDependencyRegistrar<T> For<T>()
        {
            var registration = _builder.RegisterType<T>();
            return new AutofacDependencyRegistrar<T>(registration);
        }

        public override IDependencyRegistrar<TContract> ForFactory<TContract>(Func<IResolutionContext, TContract> @delegate)
        {
            var registration = _builder.Register(x => @delegate.Invoke(new AutofacResolutionContext(x.Resolve<IComponentContext>())));
            return new AutofacDependencyRegistrar<TContract>(registration);
        }

        public override IDependencyRegistrar<TContract> ForInstanceOfType<TContract>(TContract implementation) 
        {
            var registration = _builder.RegisterInstance(implementation);

            return new AutofacDependencyRegistrar<TContract>(registration);
        }

        protected override void PerformReset()
        {
            _builder = new ContainerBuilder();
        }

        public override IDependencyResolver Build()
        {
            if (!IsBuilt)
            {
                //TODO: Thread-lock this check (APN)
                _autofacContainer = _builder.Build();
                //var registeredIds = _autofacContainer.ComponentRegistry.Registrations.Select(x => x.Id).ToArray();
                //var distinctIds = registeredIds.Distinct().ToArray();
                IsBuilt = true;
            }
            return new AutofacResolver(_autofacContainer);
        }

        internal bool IsBuilt { get; set; }

        internal static ResolvedParameter GenerateResolvedParameter<T>(string serviceName)
        {
            if (!String.IsNullOrWhiteSpace(serviceName))
            {
                return new ResolvedParameter(
                    (paramInfo, context) =>
                    (paramInfo.ParameterType == typeof(T) || paramInfo.ParameterType.IsAssignableFrom(typeof(T))),
                    (paramInfo, context) =>
                    (context.ResolveNamed<T>(serviceName)));
            }
            return new ResolvedParameter(
                (paramInfo, context) =>
                (paramInfo.ParameterType == typeof(T) || paramInfo.ParameterType.IsAssignableFrom(typeof(T))),
                (paramInfo, context) =>
                (context.Resolve<T>()));
        }

        internal static ResolvedParameter GenerateResolvedParameter(Type type, string serviceName)
        {
            if (!String.IsNullOrWhiteSpace(serviceName))
            {
                return new ResolvedParameter(
                    (paramInfo, context) =>
                    (paramInfo.ParameterType == type || paramInfo.ParameterType.IsAssignableFrom(type)),
                    (paramInfo, context) =>
                    (context.ResolveNamed(serviceName, type)));
            }
            return new ResolvedParameter(
                (paramInfo, context) =>
                (paramInfo.ParameterType == type || paramInfo.ParameterType.IsAssignableFrom(type)),
                (paramInfo, context) =>
                (context.Resolve(type)));
        }

        internal static ResolvedParameter GenerateResolvedParameter<T>(Func<IResolutionContext, T> callback)
        {
            return new ResolvedParameter(
                (paramInfo, context) =>
                (paramInfo.ParameterType == typeof(T) || paramInfo.ParameterType.IsAssignableFrom(typeof(T))),
                (paramInfo, context) =>
                (callback.Invoke(new AutofacResolutionContext(context))));
        }

        internal static ResolvedParameter GenerateResolvedParameter(Type type, Func<IResolutionContext, object> callback)
        {
            return new ResolvedParameter(
                (paramInfo, context) =>
                (paramInfo.ParameterType == type || paramInfo.ParameterType.IsAssignableFrom(type)),
                (paramInfo, context) =>
                (callback.Invoke(new AutofacResolutionContext(context))));
        }

        internal static ResolvedParameter GenerateResolvedParameter<T>(string name, Func<IResolutionContext, T> callback)
        {
            return new ResolvedParameter(
                (paramInfo, context) =>
                (paramInfo.Name == name && (paramInfo.ParameterType == typeof(T) || paramInfo.ParameterType.IsAssignableFrom(typeof(T)))),
                (paramInfo, context) =>
                (callback.Invoke(new AutofacResolutionContext(context))));
        }

        internal static ResolvedParameter GenerateResolvedParameter(Type type, string name, Func<IResolutionContext, object> callback)
        {
            return new ResolvedParameter(
                (paramInfo, context) =>
                (paramInfo.Name == name && (paramInfo.ParameterType == type || paramInfo.ParameterType.IsAssignableFrom(type))),
                (paramInfo, context) =>
                (callback.Invoke(new AutofacResolutionContext(context))));
        }
    }
}
