using System;
using Umbraco.Framework.Context;
using Umbraco.Hive;

namespace Umbraco.Cms.Web.Context
{
    /// <summary>
    /// A context object aggregating together data services useful during mapping operations.
    /// </summary>
    /// <remarks></remarks>
    public class MapResolverContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public MapResolverContext(
            IFrameworkContext frameworkContext,
            IHiveManager hive, 
            IPropertyEditorFactory propertyEditorFactory,
            IParameterEditorFactory parameterEditorFactory)
        {
            ApplicationId = Guid.NewGuid();
            FrameworkContext = frameworkContext;
            Hive = hive;

            PropertyEditorFactory = propertyEditorFactory;
            ParameterEditorFactory = parameterEditorFactory;
        }

        /// <summary>
        /// Gets or sets the property editor factory.
        /// </summary>
        /// <value>
        /// The property editor factory.
        /// </value>
        public IPropertyEditorFactory PropertyEditorFactory { get; private set; }

        /// <summary>
        /// Gets or sets the parameter editor factory.
        /// </summary>
        /// <value>
        /// The parameter editor factory.
        /// </value>
        public IParameterEditorFactory ParameterEditorFactory { get; private set; }

        /// <summary>
        /// Gets the application id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        public Guid ApplicationId { get; private set; }

        public IFrameworkContext FrameworkContext { get; private set; }

        /// <summary>
        /// Gets an insance of <see cref="HiveManager"/> associated with this application.
        /// </summary>
        /// <value>The hive.</value>
        public IHiveManager Hive { get; private set; }
        
    }
}
