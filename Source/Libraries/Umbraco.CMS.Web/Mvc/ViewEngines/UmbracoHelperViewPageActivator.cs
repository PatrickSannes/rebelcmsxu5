using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Macros;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    public class UmbracoHelperViewPageActivator : IPostViewPageActivator
    {
        private readonly IRenderModelFactory _modelFactory;

        public UmbracoHelperViewPageActivator(IRenderModelFactory modelFactory)
        {
            _modelFactory = modelFactory;
        }

        public void OnViewCreated(ControllerContext context, Type type, object view)
        {
            if (!(view is IRequiresUmbracoHelper)) return;
            
            var typedView = view as IRequiresUmbracoHelper;

            //check if the view is already IRequiresRoutableRequestContext and see if its set, if so use it, otherwise
            //check if the current controller is IRequiresRoutableRequest context as it will be a bit quicker
            //to get it from there than to use the resolver, otherwise use the resolver.
            var routableRequestContext = this.GetRoutableRequestContextFromSources(view, context);           
            typedView.Umbraco = new UmbracoHelper(context, routableRequestContext, _modelFactory);

        }

    }

 
}
