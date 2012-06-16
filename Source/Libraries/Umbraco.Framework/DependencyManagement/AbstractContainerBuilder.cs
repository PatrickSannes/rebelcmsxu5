using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Framework.DependencyManagement.DemandBuilders;

namespace Umbraco.Framework.DependencyManagement
{
	public abstract class AbstractContainerBuilder : IContainerBuilder
	{
	    protected AbstractContainerBuilder()
	    {
	        AfterReset += InitDefaults;
            Context = new DefaultBuilderContext();
	    }

	    /// <summary>Adds a dependency demand builder to the container and instructs it to register its dependencies.</summary>
		/// <param name="demandBuilder">The demand builder.</param>
		/// <returns>.</returns>
		public IContainerBuilder AddDependencyDemandBuilder(IDependencyDemandBuilder demandBuilder)
	    {
	        Mandate.ParameterNotNull(demandBuilder, "demandBuilder");

            using (DisposableTimer.TraceDuration<AbstractContainerBuilder>("Start AddDependencyDemandBuilder " + demandBuilder.GetType().Name, "End AddDependencyDemandBuilder"))
            {
                demandBuilder.Build(this, Context);
                return this;
            }
	    }

		/// <summary>Adds dependency demand builders from the assembly containing <typeparamref name="T"/>.</summary>
		/// <typeparam name="T">Type from which to infer assembly.</typeparam>
		/// <returns>.</returns>
		public IContainerBuilder AddDemandsFromAssemblyOf<T>()
		{
			AddDemandsFromAssemblyOf(typeof (T));
			return this;
		}

		/// <summary>Add dependency demand builders from the assembly of the given type.</summary>
		/// <param name="exampleType">Type from which to infer assembly.</param>
		/// <returns>.</returns>
		public IContainerBuilder AddDemandsFromAssemblyOf(Type exampleType)
		{
		    Mandate.ParameterNotNull(exampleType, "exampleType");

            using (DisposableTimer.TraceDuration<AbstractContainerBuilder>("Start AddDemandsFromAssemblyOf", "End AddDemandsFromAssemblyOf"))
            {
                Type[] typesInAssembly = exampleType.Assembly.GetTypes();

                IEnumerable<Type> typesImplementingInterface = typesInAssembly
                    .Where(type => typeof (IDependencyDemandBuilder).IsAssignableFrom(type));

                IEnumerable<IDependencyDemandBuilder> demandBuilders = typesImplementingInterface
                    .Select(component => Activator.CreateInstance(component) as IDependencyDemandBuilder);

                demandBuilders
                    .ToList()
                    .ForEach(x => AddDependencyDemandBuilder(x));

                return this;
            }
		}

	    public abstract IDependencyRegistrar<object> For(Type type);
        public abstract IDependencyRegistrar<T> For<T>();
        public abstract IDependencyRegistrar<TContract> ForFactory<TContract>(Func<IResolutionContext, TContract> @delegate);

	    public IBuilderContext Context { get; protected set; }

        public abstract IDependencyRegistrar<TContract> ForInstanceOfType<TContract>(TContract implementation) where TContract : class;

        public void InitDefaults()
        {
            // Register the defaults for the Framework
            AddDependencyDemandBuilder(new FrameworkSetup());
        }

	    protected Action BeforeReset;
	    protected Action AfterReset;

	    protected abstract void PerformReset();

		/// <summary>Resets the container builder in order to start building from scratch.</summary>
		public void Reset()
		{
		    if (BeforeReset != null)
                BeforeReset.Invoke();
            PerformReset();
            if (AfterReset != null)
                AfterReset.Invoke();
		}

		/// <summary>Instructs the container to build its dependency map.</summary>
		/// <returns>An <see cref="IDependencyResolver"/> which may be used for resolving services.</returns>
		public abstract IDependencyResolver Build();
	}
}