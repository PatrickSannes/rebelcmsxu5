using System;

namespace Umbraco.Framework.DependencyManagement
{
    /// <summary>When implemented in a derived class, provides for registering services with a specific dependency management container.</summary>
    /// <remarks>Doc updated, 14-Jan-2011.</remarks>
    public interface IContainerBuilder
    {
        IBuilderContext Context { get; }

        IDependencyRegistrar<TContract> ForInstanceOfType<TContract>(TContract implementation) where TContract : class;
        IDependencyRegistrar<TContract> ForFactory<TContract>(Func<IResolutionContext, TContract> @delegate);

        /// <summary>Resets the container builder in order to start building from scratch.</summary>
        void Reset();

        /// <summary>Instructs the container to build its dependency map.</summary>
        /// <returns>An <see cref="IDependencyResolver"/> which may be used for resolving services.</returns>
        IDependencyResolver Build();

        /// <summary>Adds a dependency demand builder to the container and instructs it to register its dependencies.</summary>
        /// <param name="demandBuilder">The demand builder.</param>
        /// <returns>.</returns>
        IContainerBuilder AddDependencyDemandBuilder(IDependencyDemandBuilder demandBuilder);

        /// <summary>Adds dependency demand builders from the assembly containing <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Type from which to infer assembly.</typeparam>
        /// <returns>.</returns>
        IContainerBuilder AddDemandsFromAssemblyOf<T>();

        /// <summary>Add dependency demand builders from the assembly of the given type. </summary>
        /// <param name="type">Type from which to infer assembly.</param>
        /// <returns>.</returns>
        IContainerBuilder AddDemandsFromAssemblyOf(Type type);

        IDependencyRegistrar<object> For(Type type);
        IDependencyRegistrar<T> For<T>();
    }
}
