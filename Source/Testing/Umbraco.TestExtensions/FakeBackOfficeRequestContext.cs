using System;
using System.Web;
using Umbraco.Cms;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Packaging;
using Umbraco.Cms.Web.Routing;

namespace Umbraco.Tests.Extensions
{
    public class FakeBackOfficeRequestContext : FakeRoutableRequestContext, IBackOfficeRequestContext
    {
        public FakeBackOfficeRequestContext(IUmbracoApplicationContext application, IRoutingEngine engine)
            : base(application, engine)
        {
            
        }

        public FakeBackOfficeRequestContext(IUmbracoApplicationContext application)
            : base(application)
        {
        }

        public FakeBackOfficeRequestContext()
        {
        }

        public SpriteIconFileResolver DocumentTypeIconResolver
        {
            get { return new MockedIconFileResolver(); }
        }

        public SpriteIconFileResolver ApplicationIconResolver
        {
            get { return new MockedIconFileResolver(); }
        }

        public IResolver<Icon> DocumentTypeThumbnailResolver
        {
            get { return new MockedIconFileResolver(); }
        }

        public IPackageContext PackageContext
        {
            get { throw new NotImplementedException(); }
        }
    }
}