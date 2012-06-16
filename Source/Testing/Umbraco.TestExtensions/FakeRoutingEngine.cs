using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Tests.Extensions
{
    using Umbraco.Cms.Web.Context;

    public class FakeRoutingEngine : IRoutingEngine
    {
        public ReadonlyHostnameCollection DomainList
        {
            get { return new ReadonlyHostnameCollection(new HostnameCollection()); }
        }

        public IUmbracoApplicationContext ApplicationContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void ClearCache(bool clearDomains = false, HiveId[] clearForIds = null, bool clearMappedUrls = false, bool clearGeneratedUrls = false, bool clearAll = false)
        {
            
        }

        public EntityRouteResult FindEntityByUrl(Uri fullUrlIncludingDomain, RevisionStatusType revisionStatusType)
        {
            return null;
        }

        public UrlResolutionResult GetUrlForEntity(TypedEntity entity)
        {
            return new UrlResolutionResult("/this-is-a-test", UrlResolutionStatus.SuccessWithoutHostname);
        }

        public UrlResolutionResult[] GetAllUrlsForEntity(TypedEntity entity)
        {
            return new UrlResolutionResult[0];
        }

    }
}