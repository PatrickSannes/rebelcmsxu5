
namespace Umbraco.Framework.EntityProviderFederation
{
    /// <summary>
    /// Holds the current mediator instance for the framework
    /// </summary>
    public static class MediatorProviders
    {
        private static Mediator current;

        /// <summary>
        /// Gets or sets the mediator for the framework
        /// </summary>
        /// <value>The current.</value>
        public static Mediator Current
        {
            get
            {
                return current;
            }
            set
            {
                current = value;
            }
        }
    }
}
