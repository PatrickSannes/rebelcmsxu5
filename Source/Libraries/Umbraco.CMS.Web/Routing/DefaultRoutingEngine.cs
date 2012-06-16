using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;

using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Hive;
namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// A utility for resolving urls and looking up entities by URL
    /// </summary>
    public class DefaultRoutingEngine : IRoutingEngine
    {
        private readonly IUmbracoApplicationContext _applicationContext;
        private readonly HttpContextBase _httpContext;
        public const string DomainListKey = "domain-list";
        public const string ContentUrlKey = "content-url";
        public const string HostnameUrlKey = "hostname-url";
        public const string EntityMappedKey = "entity-url";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRoutingEngine"/> class.
        /// </summary>
        /// <param name="appContext">The routable request context.</param>
        /// <param name="httpContext"></param>
        public DefaultRoutingEngine(IUmbracoApplicationContext appContext, HttpContextBase httpContext)
        {
            _applicationContext = appContext;
            _httpContext = httpContext;
        }

        /// <summary>
        /// Clears the cache, removes domain cache items and content-url items
        /// </summary>
        /// <param name="clearDomains">true to clear all domain cache</param>
        /// <param name="clearForIds">will clear the URL</param>
        /// <param name="clearMappedUrls">will clear the cache for all URLs mapped to entities</param>
        /// <param name="clearGeneratedUrls">clears cache for all generated urls</param>
        /// <param name="clearAll">Clears all cache</param>
        public void ClearCache(bool clearDomains = false, HiveId[] clearForIds = null, bool clearMappedUrls = false, bool clearGeneratedUrls = false, bool clearAll = false)
        {
            if (clearAll)
            {
                ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(DomainListKey);
                ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(ContentUrlKey + ".+");
                ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(HostnameUrlKey + ".+");
                ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(EntityMappedKey + ".+");
            }
            else
            {
                if (clearMappedUrls)
                {
                    ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(EntityMappedKey + ".+");
                }
                if (clearDomains)
                {
                    ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(DomainListKey);
                }
                if (clearGeneratedUrls)
                {
                    ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(ContentUrlKey + ".+");
                    ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(HostnameUrlKey + ".+");
                }
                if (clearForIds != null && clearForIds.Any())
                {
                    foreach (var id in clearForIds)
                    {
                        ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(Regex.Escape(ContentUrlKey + id) + ".*");
                        ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(Regex.Escape(HostnameUrlKey + id) + ".*");
                    }
                }
            }
        }

        /// <summary>
        /// Finds a TypedEntity based on the Uri
        /// </summary>
        /// <param name="fullUrlIncludingDomain"></param>
        /// <param name="revisionStatusType"></param>
        /// <returns></returns>
        public EntityRouteResult FindEntityByUrl(Uri fullUrlIncludingDomain, RevisionStatusType revisionStatusType)
        {
            Mandate.ParameterNotNull(fullUrlIncludingDomain.Scheme, "Scheme");

            //cache key is full uri except query string and revision status
            var cacheKey = EntityMappedKey + "-" + fullUrlIncludingDomain.GetLeftPart(UriPartial.Path) + "-" + revisionStatusType;

            //TODO: We need to change how the NiceUrls are cached because if we store them in one entry as a dictionary in cache, then
            // we can do a reverse lookup to see if we've already generated the URL for the entity which may match the fullUrlIncludingDomain,
            // if so, then all we have to do is return the entity with the cached ID.

            return ApplicationContext.FrameworkContext.ApplicationCache
                .GetOrCreate(cacheKey, () =>
                    {
                        using (DisposableTimer.Start(timer =>
                            LogHelper.TraceIfEnabled<DefaultRoutingEngine>("FindEntityByUrl for URL {0} took {1}ms", () => fullUrlIncludingDomain, () => timer)))
                        {
                            var persistenceManager = ApplicationContext.Hive.GetReader<IContentStore>(fullUrlIncludingDomain);
                            if (persistenceManager != null)
                            {
                                using (var uow = persistenceManager.CreateReadonly())
                                {
                                    //first, lets check if it's an ID URL
                                    var path = fullUrlIncludingDomain.AbsolutePath.Substring(
                                        _httpContext.Request.ApplicationPath.TrimEnd('/').Length,
                                        fullUrlIncludingDomain.AbsolutePath.Length - _httpContext.Request.ApplicationPath.TrimEnd('/').Length);
                                    var urlId = HiveId.TryParse(path.TrimStart('/'));
                                    if (urlId.Success && urlId.Result.ProviderGroupRoot != null)
                                    {
                                        LogHelper.TraceIfEnabled<DefaultRoutingEngine>("Resolving entity by Id URL (Id: {0} ", () => urlId.Result.ToFriendlyString());
                                        try
                                        {
                                            var entityById = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(urlId.Result, revisionStatusType);
                                            if (entityById == null)
                                            {
                                                LogHelper.Warn<DefaultRoutingEngine>("Resolving entity by Id URL failed (Id: {0} ", urlId.Result.ToFriendlyString());
                                                return null;
                                            }
                                            return new HttpRuntimeCacheParameters<EntityRouteResult>(
                                                   new EntityRouteResult(entityById.Item, EntityRouteStatus.SuccessById));
                                        }
                                        catch (ArgumentException)
                                        {
                                            //this occurs if the Id parsed but 'not really'
                                            return null;
                                        }
                                    }

                                    Revision<TypedEntity> lastRevisionFound;
                                    TypedEntity lastItemFound;
                                    //is the current requesting hostname/port in our list ?
                                    if (DomainList.ContainsHostname(fullUrlIncludingDomain.Authority))
                                    {
                                        //domain found so get the first item assigned to this domain
                                        LogHelper.TraceIfEnabled<DefaultRoutingEngine>("Resolving entity by Domain URL");
                                        var hostnameEntry = DomainList[fullUrlIncludingDomain.Authority];
                                        lastRevisionFound = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(hostnameEntry.ContentId, revisionStatusType);
                                        Mandate.That(lastRevisionFound != null, x => new InvalidOperationException("Could not find an entity with a revision status of '" + revisionStatusType.Alias + "', having a hostname '" + fullUrlIncludingDomain.Authority + "' and id: " + hostnameEntry.ContentId));
                                        lastItemFound = lastRevisionFound.Item;
                                    }
                                    else
                                    {
                                        //no domain found for the current request, so we need to find the first routable node that doesn't require a domain
                                        LogHelper.TraceIfEnabled<DefaultRoutingEngine>("Resolving entity by Non-Domain URL");
                                        //var root = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(FixedHiveIds.ContentVirtualRoot, revisionStatusType);
                                        //Mandate.That(root != null, x => new InvalidOperationException("Could not find the content root"));
                                        var domainListIds = DomainList.Select(d => d.ContentId);

                                        var firstLevelRelations =
                                            uow.Repositories.GetChildRelations(FixedHiveIds.ContentVirtualRoot, FixedRelationTypes.DefaultRelationType).OrderBy(
                                                x => x.Ordinal).ToArray();

                                        //try to find a first level node that doesn't exist in our domain list
                                        var firstNonHostnameEntity = firstLevelRelations.FirstOrDefault(x => !domainListIds.Contains(x.DestinationId));

                                        //if there is no first level node anywhere, then there is no content. Show a friendly message
                                        if (firstNonHostnameEntity == null && firstLevelRelations.Count() == 0)
                                            return null;

                                        //if we have a first level node not assigned to a domain, use the first, otherwise if all nodes are assigned to domains, then just use the first
                                        lastRevisionFound = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(
                                            firstNonHostnameEntity == null
                                                ? firstLevelRelations.First().DestinationId
                                                : firstNonHostnameEntity.DestinationId, revisionStatusType);

                                        lastItemFound = lastRevisionFound != null ? lastRevisionFound.Item : null;
                                    }


                                    // Now we will have the path from the current application root like:
                                    //      /this/is/a/path/to/a/document
                                    // Now we need to walk down the tree
                                    if (lastItemFound != null && !string.IsNullOrWhiteSpace(path) && path != "/")
                                        lastItemFound = uow.Repositories.GetEntityByPath<TypedEntity>(lastItemFound.Id, path, revisionStatusType, true);

                                    if(lastItemFound == null)
                                        return new HttpRuntimeCacheParameters<EntityRouteResult>(
                                                new EntityRouteResult(null, EntityRouteStatus.FailedNotFoundByName));

                                    return new HttpRuntimeCacheParameters<EntityRouteResult>(
                                        new EntityRouteResult(lastItemFound, EntityRouteStatus.SuccessWithoutHostname));
                                }
                            }

                            return null; 
                        }
                    });
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// This takes into account the current host name in the request. If the current host name matches a host name
        /// defined in the domain list for the entity being looked up, then the hostname of the current request will 
        /// be used, otherwise the primary (first ordinal) domain will be used for the url.
        /// </remarks>
        public UrlResolutionResult GetUrlForEntity(TypedEntity entity)
        {
            Mandate.ParameterNotNull(entity, "entity");

            using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
            {
                if (entity.IsContent(uow))
                    return GetContentUrl(entity);

                if (entity.IsMedia(uow))
                    return GetMediaUrl(entity);
            }
            throw new NotSupportedException("Unknown entity type");
        }

        /// <summary>
        /// Returns the list of domains and their asigned hive ids
        /// </summary>
        /// <remarks>
        /// The domain list is stored in application cache
        /// </remarks>
        public ReadonlyHostnameCollection DomainList
        {
            get
            {
                return ApplicationContext
                    .FrameworkContext.ApplicationCache
                    .GetOrCreate(DomainListKey,
                                 () =>
                                 {
                                     using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
                                     {
                                         //Gets all latest hostname revisions with it's relation to content

                                         //TODO: This needs to be changed so that we can request the latest hostnames
                                         //in a query, otherwise this may end up returning A LOT of data
                                         var allHostnameEntities = (from e in uow.Repositories.QueryContext.Query()
                                                                    where e.EntitySchema.Alias == HostnameSchema.SchemaAlias
                                                                    select e).ToArray();
                                         var latestHostnames = (from revision in allHostnameEntities
                                                                group revision by revision.Id
                                                                    into g
                                                                    select g.OrderByDescending(t => t.UtcStatusChanged).First()).ToArray();

                                         var d = new HostnameCollection();
                                         foreach (var e in latestHostnames)
                                         {
                                             var name = e.Attribute<string>(HostnameSchema.HostnameAlias);

                                             var relation = uow.Repositories.GetParentRelations(e.Id, FixedRelationTypes.HostnameRelationType).FirstOrDefault();

                                             // Check if the relation is null, so that we account for hostnames that are orphaned and not linked to anywhere
                                             if (relation != null)
                                                d.Add(new HostnameEntry(name, relation.SourceId, relation.Ordinal));
                                         }
                                         return new HttpRuntimeCacheParameters<ReadonlyHostnameCollection>(new ReadonlyHostnameCollection(d));
                                     }
                                 });
            }
        }

        public IUmbracoApplicationContext ApplicationContext
        {
            get
            {
                return _applicationContext;
            }
        }

        /// <summary>
        /// Resolves the url for the specified entity with its full domain paths based on all hostnames assigned.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>All URLs for the entity based on all of its hostnames assigned, if no hostnames are assigned in this entities branch, then a null value is returned</returns>
        public UrlResolutionResult[] GetAllUrlsForEntity(TypedEntity entity)
        {
            //return from cache if its there, otherwise go get it
            return ApplicationContext.FrameworkContext.ApplicationCache
                .GetOrCreate(HostnameUrlKey + entity.Id, () =>
                    {
                        using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
                        {
                            var ancestorsOrSelf =
                                uow.Repositories.GetAncestorsOrSelf(entity, FixedRelationTypes.DefaultRelationType)
                                    .Where(x => !x.Id.IsSystem()).Cast<TypedEntity>().ToArray();

                            // (APN Oct 2011) In latest codebase ancestors are returned in reverse document order, same as Xml
                            var reverse = ancestorsOrSelf.Reverse();

                            var nonDomainurl = GetNonDomainUrl(reverse);
                            var domainUrls = GetDomainUrls(reverse);

                            var results = new[] { nonDomainurl }
                                .Union(domainUrls)
                                .Where(x => x != null)
                                .ToList();

                            //if there's no results, then it can't be routed to as it requires a domain
                            if (!results.Any())
                            {
                                results.Add(new UrlResolutionResult(string.Empty, UrlResolutionStatus.FailedRequiresHostname));
                            }

                            return new HttpRuntimeCacheParameters<UrlResolutionResult[]>(results.ToArray());
                        }
                    });
        }

        /// <summary>
        /// Returns a list of URLs with the domains assigned based on the list of ancestorsOrSelf. If no domain is assigned to the branch that the entity exists in a null value is returned.
        /// </summary>
        /// <param name="ancestorsOrSelf">The ancestorsOrSelf list to create the URL for</param>
        /// <returns></returns>        
        protected IEnumerable<UrlResolutionResult> GetDomainUrls(IEnumerable<TypedEntity> ancestorsOrSelf)
        {
            var reversed = ancestorsOrSelf.Reverse();
            var parts = new List<string>();

            //need to figure out a domain assigned to this node
            foreach (var a in reversed)
            {
                if (DomainList.ContainsContentId(a.Id))
                {
                    //ok, we've found a node with a domain assigned, return a list of URLs with all domains assigned to this id
                    return DomainList[a.Id]
                        .Select(d =>
                            new UrlResolutionResult(
                                (d.Hostname + _httpContext.Request.ApplicationPath.TrimEnd('/') + "/" + string.Join("/", Enumerable.Reverse(parts).ToArray())).TrimEnd('/'),
                                UrlResolutionStatus.SuccessWithHostname))
                        .ToArray();
                }
                else
                {
                    var urlAlias = a.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "UrlName");
                    Mandate.That(!urlAlias.IsNullOrWhiteSpace(), x => new InvalidOperationException("UrlName cannot be empty"));
                    parts.Add(urlAlias);
                }
            }

            //this will occur if no domains are found
            return Enumerable.Empty<UrlResolutionResult>();
        }

        /// <summary>
        /// Returns the non-domain URL for the 'self' node in ancestorsOrSelf or null if this node cannot
        /// exist in a non-domain branch.
        /// </summary>
        /// <param name="ancestorsOrSelf"></param>
        /// <returns></returns>
        protected UrlResolutionResult GetNonDomainUrl(IEnumerable<TypedEntity> ancestorsOrSelf)
        {
            using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var domainListIds = DomainList.Select(x => x.ContentId);
                //need to get the content root to check how many root branches there are
                var contentRoot = uow.Repositories.Get<TypedEntity>(FixedHiveIds.ContentVirtualRoot);
                Mandate.That(contentRoot != null, x => new NullReferenceException("No ContentVirtualRoot exists!"));

                var firstLevelRelations = uow.Repositories.GetChildRelations(FixedHiveIds.ContentVirtualRoot, FixedRelationTypes.DefaultRelationType).OrderBy(x => x.Ordinal).ToArray();
                var firstLevelIds = firstLevelRelations.Select(x => x.DestinationId).ToArray();
                var ancestorIds = ancestorsOrSelf.Select(x => x.Id).ToArray();

                //returns true if the branch passed in is part of the first branch found that doesn't have a domain assigned
                Func<IEnumerable<HiveId>, bool> isFirstBranchNotAssignedADomain = branchIds =>
                    {
                        var firstLevelIdsWithoutDomains = firstLevelIds.Where(x => !domainListIds.Contains(x));
                        var firstBranchWithoutDomain = firstLevelIdsWithoutDomains.First();
                        return branchIds.Contains(firstBranchWithoutDomain);
                    };

                //if there's only one node, 
                // OR the current branch isnt't assigned to a domain and its part of the first branch that isn't assigned to a domain, 
                // OR all first level nodes are assigned to domains and the requesting node is part of the very first branch

                if (firstLevelRelations.Count() == 1
                    || (!domainListIds.ContainsAny(ancestorIds) && isFirstBranchNotAssignedADomain(ancestorIds))
                    || (domainListIds.ContainsAll(firstLevelIds) && ancestorIds.Contains(firstLevelIds.First())))
                {
                    //if there's only one node, just pick it, otherwise get the first without hostnames
                    var firstLevelRelationWithoutHostname = firstLevelRelations.Count() == 1
                        ? firstLevelRelations.First()
                        : firstLevelRelations.FirstOrDefault(x => !domainListIds.Contains(x.DestinationId));

                    if (firstLevelRelationWithoutHostname == null)
                    {
                        //NOTE: I don't think this ever will happen! The initial thought wa that the relations api will ony return the latest published versions but this is not true, they return the latest version

                        //can't find a first routable branch, so must not be published
                        return new UrlResolutionResult(string.Empty, UrlResolutionStatus.FailedNotPublished);
                    }

                    //ok, now we can give it a URL

                    var appPath = _httpContext.Request.ApplicationPath.TrimEnd('/');
                    var level = 0;
                    var urlBuilder = new StringBuilder();
                    urlBuilder.Append(appPath);

                    foreach (var e in ancestorsOrSelf)
                    {
                        if (level > 0)
                        {
                            var urlAlias = e.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "UrlName");
                            if (!string.IsNullOrEmpty(urlAlias))
                            {
                                urlBuilder.Append("/");
                                urlBuilder.Append(urlAlias);
                            }
                        }
                        level++;
                    }

                    var url = "/"+ urlBuilder.ToString().Trim('/');

                    return new UrlResolutionResult(url, UrlResolutionStatus.SuccessWithoutHostname);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the content URL.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// This takes into account the current host name in the request. If the current host name matches a host name
        /// defined in the domain list for the entity being looked up, then the hostname of the current request will 
        /// be used, otherwise the primary (first ordinal) domain will be used for the url.
        /// </remarks>
        protected UrlResolutionResult GetContentUrl(TypedEntity entity)
        {
            Mandate.ParameterNotNull(entity, "entity");

            //the cache key must include the host since it must be unique depending on what the current host/port is
            var cacheKey = ContentUrlKey + entity.Id + _httpContext.Request.Url.Authority;

            //return from cache if its there, otherwise go get it
            return ApplicationContext.FrameworkContext.ApplicationCache
                .GetOrCreate(cacheKey, () =>
                    {
                        using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
                        {
                            //need to check if this node is in a branch with an assigned domain                            
                            var ancestorsOrSelf =
                                uow.Repositories.GetAncestorsOrSelf(entity, FixedRelationTypes.DefaultRelationType)
                                    .Where(x => !x.Id.IsSystem()).Cast<TypedEntity>().ToArray();

                            // (APN Oct 2011) In latest codebase ancestors are returned in reverse document order, same as Xml
                            var reverse = ancestorsOrSelf.Reverse();

                            var nonDomainUrl = GetNonDomainUrl(reverse);
                            if (nonDomainUrl != null)
                            {
                                return new HttpRuntimeCacheParameters<UrlResolutionResult>(nonDomainUrl);
                            }

                            //this is a branch that has a hostname assigned, so need to route by that
                            var domainUrls = GetDomainUrls(reverse);
                            if (domainUrls == null || !domainUrls.Any())
                            {
                                return new HttpRuntimeCacheParameters<UrlResolutionResult>(new UrlResolutionResult(string.Empty, UrlResolutionStatus.FailedRequiresHostname));
                            }

                            //now we need to determine what the current authority is, and if it matches one of the domain URLs, then we can remove the authority since
                            //we'll be returning a relative URL to the current request

                            var relativeHostUrl = domainUrls.Where(x => x.Url.StartsWith(_httpContext.Request.Url.Authority)).FirstOrDefault();
                            if (relativeHostUrl != null)
                            {
                                //return a relative URL for the current request authority
                                //var url = relativeHostUrl.Url.Substring(0, _httpContext.Request.Url.Authority.Length);
                                var url = relativeHostUrl.Url.Substring(_httpContext.Request.Url.Authority.Length, relativeHostUrl.Url.Length - _httpContext.Request.Url.Authority.Length);
                                return new HttpRuntimeCacheParameters<UrlResolutionResult>(new UrlResolutionResult(url, UrlResolutionStatus.SuccessWithoutHostname));
                            }

                            //if the request has come in on a domain that isn't in this entity's domain urls, then return the first domain url assigned to it
                            return new HttpRuntimeCacheParameters<UrlResolutionResult>(domainUrls.First());

                        }
                    });
        }

        /// <summary>
        /// Gets the media URL.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected UrlResolutionResult GetMediaUrl(TypedEntity entity)
        {
            Mandate.ParameterNotNull(entity, "entity");

            var requestContext = new RequestContext(_httpContext, new RouteData());
            var urlHelper = new UrlHelper(requestContext);

            return new UrlResolutionResult(urlHelper.GetMediaUrl(entity), UrlResolutionStatus.SuccessWithoutHostname);
        }
    }
}
