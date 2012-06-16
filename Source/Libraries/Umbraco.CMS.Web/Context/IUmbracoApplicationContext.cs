using System;
using System.Collections.Generic;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework.Context;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Framework.Security;
using Umbraco.Hive;

namespace Umbraco.Cms.Web.Context
{
    /// <summary>
    /// Encapsulates information specific to an Umbraco application
    /// </summary>
    public interface IUmbracoApplicationContext : IDisposable
    {
        IEnumerable<InstallStatus> GetInstallStatus();

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        IFrameworkContext FrameworkContext { get; }

        /// <summary>
        /// Gets the application id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        Guid ApplicationId { get; }

        /// <summary>
        /// Gets an instance of <see cref="HiveManager"/> for this application.
        /// </summary>
        /// <value>The hive.</value>
        IHiveManager Hive { get; }

        /// <summary>
        /// Gets the settings associated with this Umbraco application.
        /// </summary>
        /// <value>The settings.</value>
        UmbracoSettings Settings { get; }

        /// <summary>
        /// Gets the security service.
        /// </summary>
        ISecurityService Security { get; }
    }
}