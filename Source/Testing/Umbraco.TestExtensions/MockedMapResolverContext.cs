using Umbraco.Cms;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework.Context;
using Umbraco.Hive;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Extensions
{
    public class MockedMapResolverContext : MapResolverContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public MockedMapResolverContext(IFrameworkContext frameworkContext, IHiveManager hive, IPropertyEditorFactory propertyEditorFactory, IParameterEditorFactory parameterEditorFactory)
            : base(frameworkContext, hive, propertyEditorFactory, parameterEditorFactory)
        {
        }
    }
}