using System;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Cms.Web.Routing
{
    using Umbraco.Cms.Web.Context;

    public interface IRoutingEngine
    {
        /// <summary>
        /// returns the list of domains assigned to HiveIds
        /// </summary>
        ReadonlyHostnameCollection DomainList { get; }

        IUmbracoApplicationContext ApplicationContext { get; }

        /// <summary>
        /// Clears the Url and domain cache.
        /// </summary>
        /// <param name="clearDomains">Clears domain cache</param>
        /// <param name="clearForIds">Clears generated urls for the specified ids</param>
        /// <param name="clearMappedUrls">Clears cache for URLs that were routed to entities</param>
        /// <param name="clearGeneratedUrls">Clears all generated URLs</param>
        /// <param name="clearAll">Clears all routing cache</param>
        void ClearCache(bool clearDomains = false, HiveId[] clearForIds = null, bool clearMappedUrls = false, bool clearGeneratedUrls = false, bool clearAll = false);

        /// <summary>
        /// Gets the entity by URL
        /// </summary>
        /// <param name="fullUrlIncludingDomain">The route.</param>
        /// <param name="revisionStatusType">Type of the revision status.</param>
        /// <returns></returns>
        EntityRouteResult FindEntityByUrl(Uri fullUrlIncludingDomain, RevisionStatusType revisionStatusType);

        /// <summary>
        /// Resolves the url for the specified entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        UrlResolutionResult GetUrlForEntity(TypedEntity entity);

        /// <summary>
        /// Resolves the url for the specified entity with its full domain paths based on all hostnames assigned.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>All URLs for the entity based on all of its hostnames assigned, if no hostnames are assigned in this entities branch, then a null value is returned</returns>
        UrlResolutionResult[] GetAllUrlsForEntity(TypedEntity entity);

    }
}