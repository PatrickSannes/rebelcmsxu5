namespace Umbraco.Framework.DependencyManagement
{
    /// <summary>Interface for dependency demand builders. Specific implementations act like 'modules' declaring their type dependencies with a container.</summary>
    public interface IDependencyDemandBuilder
    {
        /// <summary>Builds the dependency demands required by this implementation. </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="context">The context for this building session containing configuration etc.</param>
        void Build(IContainerBuilder containerBuilder, IBuilderContext context);
    }
}