using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Cms;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Dashboards.Filters;
using Umbraco.Cms.Web.Dashboards.Rules;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.Surface;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Security;
using Umbraco.Framework.Tasks;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Extensions
{
    public class FakeRoutableRequestContext : IRoutableRequestContext
    {
        private IRoutingEngine _engine;

        public FakeRoutableRequestContext(IUmbracoApplicationContext application)
        {
            Application = application;
        }

        public FakeRoutableRequestContext(IUmbracoApplicationContext application, IRoutingEngine engine)
        {
            _engine = engine;
            Application = application;
        }

        public FakeRoutableRequestContext()
            : this(new FakeUmbracoApplicationContext())
        { }

        private Guid? _requestId = null;

        /// <summary>
        /// Gets the request id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        public Guid RequestId
        {
            get
            {
                if (_requestId == null)
                    _requestId = Guid.NewGuid();
                return _requestId.Value;
            }
        }

        /// <summary>
        /// Gets the Umbraco application context which contains services which last for the lifetime of the application.
        /// </summary>
        /// <value>The application.</value>
        public IUmbracoApplicationContext Application { get; protected set; }


        public ComponentRegistrations RegisteredComponents
        {
            get
            {
                var propEditors = new FixedPropertyEditors(Application);

                return new ComponentRegistrations(new List<Lazy<MenuItem, MenuItemMetadata>>(),
                                                  new List<Lazy<AbstractEditorController, EditorMetadata>>(),
                                                  new List<Lazy<TreeController, TreeMetadata>>(),
                                                  new List<Lazy<SurfaceController, SurfaceMetadata>>(),
                                                  new List<Lazy<AbstractTask, TaskMetadata>>(),
                                                  propEditors.GetPropertyEditorDefinitions(),
                                                  new List<Lazy<AbstractParameterEditor, ParameterEditorMetadata>>(),
                                                  new List<Lazy<DashboardMatchRule, DashboardRuleMetadata>>(),
                                                  new List<Lazy<DashboardFilter, DashboardRuleMetadata>>(),
                                                  new List<Lazy<Permission, PermissionMetadata>>(),
                                                  new List<Lazy<AbstractMacroEngine, MacroEngineMetadata>>());
            }
        }

        public IRoutingEngine RoutingEngine
        {
            get
            {
                if (_engine == null)
                {
                    _engine = new FakeRoutingEngine();
                }
                return _engine;
            }
        }

    }
}
