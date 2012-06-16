using System;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Routing;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Context
{
    /// <summary>
    /// Encapsulates information specific to a request handled by Umbraco
    /// </summary>
    public interface IRoutableRequestContext
    {
        /// <summary>
        /// Gets the request id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        Guid RequestId { get; }

        /// <summary>
        /// Gets the Umbraco application context which contains services which last for the lifetime of the application.
        /// </summary>
        /// <value>The application.</value>
        IUmbracoApplicationContext Application { get; }

        /// <summary>
        /// Lists all plugin components registered
        /// </summary>
        ComponentRegistrations RegisteredComponents { get; }

        /// <summary>
        /// Gets the URL utility.
        /// </summary>
        IRoutingEngine RoutingEngine { get; }

    }
}
