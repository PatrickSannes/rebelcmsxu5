using System.IO;
using Umbraco.Cms.Packages.DevDataset;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence;

namespace Umbraco.Tests.Extensions
{
    public static class DemoDataHelper
    {
        public static DevDataset GetDemoData(IUmbracoApplicationContext appContext, IAttributeTypeRegistry attributeTypeRegistry)
        {
            return new DevDataset(new MockedPropertyEditorFactory(appContext), appContext.FrameworkContext, attributeTypeRegistry);
        }
    }
}