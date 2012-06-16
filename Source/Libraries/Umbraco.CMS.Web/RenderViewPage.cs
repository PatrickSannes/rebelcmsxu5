using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Hive;

namespace Umbraco.Cms.Web
{

    /// <summary>
    /// An class containing all of the objects required for the RenderViewPage
    /// </summary>
    /// <remarks>
    /// Each property of the context is assigned in the Post view page activator except in the case of layouts since these are not 
    /// created by the post view page activators. In this case, this class will look into the HttpContext items for an existing RenderViewPageContext
    /// which should always exist, but if for some reason it does not, this class will try to resolve the dependencies required to create these 
    /// objects from the DependencyResolver.
    /// </remarks>
    internal class RenderViewPageContext
    {
        private readonly ControllerContext _controllerContext;
        private readonly HttpContextBase _http;

        public RenderViewPageContext(ControllerContext controllerContext)
        {
            _controllerContext = controllerContext;
            _http = _controllerContext.HttpContext;
        }

        /// <summary>
        /// Uses when the property isn't explicity set, this will try to resolve the service first from the RenderViewPageContext stored
        /// in the HttpContext items, 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromContext"></param>
        /// <param name="ifAllElseFails"> </param>
        /// <returns></returns>
        private T GetService<T>(Func<RenderViewPageContext, T> fromContext, Func<T> ifAllElseFails) where T : class
        {
            //this will only occur for layout files, so try to retreive it from HttpContextItems since the RenderViewPageActivator will store it there too
            if (!_http.Items.Contains(typeof(RenderViewPageContext).Name))
            {
                return ifAllElseFails();                
            }

            var ctx = _http.Items[typeof(RenderViewPageContext).Name] as RenderViewPageContext;
            if (ctx != null)
            {
                var service = fromContext(ctx);
                if (service == null)
                    throw new InvalidOperationException("Could not resolve RenderViewPageContext from HttpContext Items, this will only occur if you have a layout view inheriting from RenderViewPage but are not rendering a View inheriting from RenderViewPage which is not valid.");
                return service;
            }

            throw new InvalidOperationException("The item with key " + typeof(RenderViewPageContext).Name + " is not of type " + typeof(RenderViewPageContext).FullName + " inside the HttpContext Items");
        }

        private IRenderModelFactory _renderModelFactory;
        public IRenderModelFactory RenderModelFactory
        {
            get { return _renderModelFactory ?? (_renderModelFactory = GetService(ctx => ctx.RenderModelFactory, () => DependencyResolver.Current.GetService<IRenderModelFactory>())); }
            internal set { _renderModelFactory = value; }
        }

        private IRoutableRequestContext _routableRequestContext;

        /// <summary>
        /// The current routable request context
        /// </summary>
        public IRoutableRequestContext RoutableRequestContext
        {
            get { return _routableRequestContext ?? (_routableRequestContext = GetService(ctx => ctx.RoutableRequestContext, () => DependencyResolver.Current.GetService<IRoutableRequestContext>())); }
            internal set { _routableRequestContext = value; }
        }

        /// <summary>
        /// Gets the <see cref="IHiveManager"/> for the application. You can use this to run queries against the data for this application.
        /// </summary>
        /// <value>The hive.</value>
        public IRenderViewHiveManagerWrapper Hive { get { return new RenderViewHiveManagerWrapper(RoutableRequestContext.Application.Hive); } }

        private UmbracoHelper _umbraco;

        /// <summary>
        /// Gets an umbraco helper
        /// </summary>
        public UmbracoHelper Umbraco
        {
            get
            {
                return _umbraco ?? (_umbraco = GetService(ctx => ctx.Umbraco,
                                                          () => new UmbracoHelper(_controllerContext, RoutableRequestContext, RenderModelFactory)));
            }
            internal set { _umbraco = value; }
        }

        private IUmbracoRenderModel _model;
        public IUmbracoRenderModel RenderModel
        {
            get
            {
                return _model ?? (_model = GetService(ctx => ctx.RenderModel,
                                                      () => RenderModelFactory.Create(_http, _http.Request.RawUrl)));
            }
            internal set { _model = value; }
        }

        private dynamic _dynamicModel;
        public dynamic DynamicModel
        {
            get { return _dynamicModel ?? (_dynamicModel = RenderModel.CurrentNode.AsDynamic()); }
        }
    }

    /// <summary>
    /// The View that front-end templates inherit from
    /// </summary>
    public abstract class RenderViewPage : WebViewPage<Content>
    {


        private RenderViewPageContext _renderViewPageContext;
        
        /// <summary>
        /// Gets/sets the RenderViewPageContext which exposes all of the dependencies needed to return the custom properties of this view
        /// </summary>
        internal RenderViewPageContext RenderViewPageContext
        {
            get
            {
                //the context will only be null for layout pages
                if (_renderViewPageContext == null)
                {
                    //at no time should this be null unless someone is trying to execute this page outside of normal mvc scope
                    if (ViewContext == null)
                        throw new InvalidOperationException("Cannot access the RenderViewPage custom properties without a ViewContext");
                    _renderViewPageContext = new RenderViewPageContext(ViewContext.Controller.ControllerContext);
                }
                return _renderViewPageContext;
            }
            set { _renderViewPageContext = value; }
        }

        /// <summary>
        /// This will ensure that the Model and RenderViewPageContext are set. This is generally only called for Layouts.
        /// </summary>
        /// <param name="parentPage"></param>
        /// <remarks>
        /// This method is only required for Layout pages since they are not created by the IViewPageActivator and the Model is not set on them
        /// directly because it is normally only set for the view returned from the controller.
        /// </remarks>
        protected override void ConfigurePage(global::System.Web.WebPages.WebPageBase parentPage)
        {
            base.ConfigurePage(parentPage);
            
            //set the model to the current node if it is not set, this is generally not the case
            if (Model == null)
            {
                if (Model == null)
                {
                    this.ViewData.Model = RenderViewPageContext.RenderModel.CurrentNode;    
                }                
            }
        }
        
        /// <summary>
        /// The current routable request context
        /// </summary>
        public IRoutableRequestContext RoutableRequestContext
        {
            get { return RenderViewPageContext.RoutableRequestContext; }
        }

        /// <summary>
        /// Gets the <see cref="IHiveManager"/> for the application. You can use this to run queries against the data for this application.
        /// </summary>
        /// <value>The hive.</value>
        public IRenderViewHiveManagerWrapper Hive { get { return new RenderViewHiveManagerWrapper(RoutableRequestContext.Application.Hive); } }

        /// <summary>
        /// Gets an umbraco helper
        /// </summary>
        public UmbracoHelper Umbraco
        {
            get { return RenderViewPageContext.Umbraco; }
        }

        /// <summary>
        /// Gets the <see cref="Model"/> as a dynamic object.
        /// </summary>
        /// <remarks></remarks>
        public dynamic DynamicModel
        {
            get { return RenderViewPageContext.DynamicModel; }
        }

    }
}