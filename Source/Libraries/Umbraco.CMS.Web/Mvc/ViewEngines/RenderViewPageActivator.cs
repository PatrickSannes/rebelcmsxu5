using System;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// Ensures the DynamicModel property is set on the RenderViewPage
    /// </summary>
    public class RenderViewPageActivator : IPostViewPageActivator
    {
        private readonly IRenderModelFactory _renderModelFactory;

        public RenderViewPageActivator(IRenderModelFactory renderModelFactory)
        {
            _renderModelFactory = renderModelFactory;
        }

        public void OnViewCreated(ControllerContext context, Type type, object view)
        {
            if (view is RenderViewPage)
            {
                var renderView = view as RenderViewPage;
                var renderModel = _renderModelFactory.Create(context.HttpContext, context.RequestContext.HttpContext.Request.RawUrl);

                var routableRequestContext = this.GetRoutableRequestContextFromSources(view, context);
                
                //set the RenderViewPageContext of the view
                renderView.RenderViewPageContext = new RenderViewPageContext(context)
                    {
                        RenderModel = renderModel,                        
                        RoutableRequestContext = routableRequestContext,
                        RenderModelFactory = _renderModelFactory,
                        Umbraco = new UmbracoHelper(context, routableRequestContext, _renderModelFactory)
                    };

                //add the context to the HttpContext Items so it can be resolved if necessary by a RenderViewPage if it is not created
                //with this activator... this will occur for any Layout files that inherit from RenderViewPage because layout files are 
                //created differently in MVC.
                if (!context.HttpContext.Items.Contains(typeof(RenderViewPageContext).Name))
                    context.HttpContext.Items.Add(typeof(RenderViewPageContext).Name, renderView.RenderViewPageContext);
            }
        }
    }
}