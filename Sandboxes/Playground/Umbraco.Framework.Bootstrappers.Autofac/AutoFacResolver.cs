using System;
using System.Collections.Generic;
using Autofac;
using Umbraco.Foundation;
using Umbraco.Framework.Bootstrappers.Autofac.Modules;
using Umbraco.Framework.DependencyManagement;

namespace Umbraco.Framework.Bootstrappers.Autofac
{
    public class AutoFacResolver : IDependencyResolver
    {
        public static void InitialiseFoundation()
        {
            DependencyResolver.Current = new AutoFacResolver();
        }

        private IContainer _container = null;

        public AutoFacResolver()
        {
            _container = GetNewContainer();
        }

        public AutoFacResolver(IContainer container)
        {
            _container = container;
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        /// <summary>
        /// Resolves all services given the required contract type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> of instances of type <typeparamref name="T"/>.</returns>
        public IEnumerable<T> ResolveAll<T>()
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>(string name)
        {
            return _container.ResolveNamed<T>(name);
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        /// <summary>
        /// Resolves all services given the required contract type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of instances of type <paramref name="type"/>.
        /// </returns>
        public IEnumerable<object> ResolveAll(Type type)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type type, string name)
        {
            return _container.ResolveNamed(name, type);
        }

        public ResolutionAttemptTuple<T> TryResolve<T>() where T : class
        {
            try
            {
                T tryResolve = Resolve<T>();
                return new ResolutionAttemptTuple<T>(tryResolve != null, tryResolve);
            }
            catch (Exception)
            {
                return new ResolutionAttemptTuple<T>(false, null);
            }
        }

        public ResolutionAttemptTuple<T> TryResolve<T>(string name) where T : class
        {
            try
            {
                T tryResolve = Resolve<T>(name);
                return new ResolutionAttemptTuple<T>(tryResolve != null, tryResolve);
            }
            catch (Exception)
            {
                return new ResolutionAttemptTuple<T>(false, null);
            }
        }

        public ResolutionAttemptTuple<object> TryResolve(Type type)
        {
            try
            {
                object tryResolve = Resolve(type);
                return new ResolutionAttemptTuple<object>(tryResolve != null, tryResolve);
            }
            catch (Exception)
            {
                return new ResolutionAttemptTuple<object>(false, null);
            }
        }

        public ResolutionAttemptTuple<object> TryResolve(Type type, string name)
        {
            try
            {
                object tryResolve = Resolve(type, name);
                return new ResolutionAttemptTuple<object>(tryResolve != null, tryResolve);
            }
            catch (Exception)
            {
                return new ResolutionAttemptTuple<object>(false, null);
            }
        }

        public static IContainer GetNewContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoadFromConfigurationModule());

            //TODO: Disabled to fix build, but needs replacing (along with entire Bootstrapper assembly)
            //DtoMappingHelper.RegisterAllFromAssemblyOf<PersistenceEntityDto>();
            //builder.RegisterType<Persistence.AutoMapperTypeMapper>().As<ITypeMapper>().SingleInstance();

            return builder.Build();
        }
    }
}
