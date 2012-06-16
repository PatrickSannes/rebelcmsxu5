namespace Umbraco.Framework.Context
{
    /// <summary>
    /// Denotes that a component requires a <see cref="IFrameworkContext"/>
    /// </summary>
    /// <remarks></remarks>
    public interface IRequiresFrameworkContext
    {
        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        IFrameworkContext FrameworkContext { get; }
    }
}