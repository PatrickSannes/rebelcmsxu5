using System.Web.Mvc;
using Umbraco.Cms.Web.Context;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    internal static class PostViewPageActivatorExtensions
    {
        /// <summary>
        /// Checks the sources to return the current routable request context the quickest
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="view"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IRoutableRequestContext GetRoutableRequestContextFromSources(this IPostViewPageActivator activator, object view, ControllerContext context)
        {
            //check if the view is already IRequiresRoutableRequestContext and see if its set, if so use it, otherwise
            //check if the current controller is IRequiresRoutableRequest context as it will be a bit quicker
            //to get it from there than to use the resolver, otherwise use the resolver.
            IRoutableRequestContext routableRequestContext = null;
            if (view is IRequiresRoutableRequestContext && ((IRequiresRoutableRequestContext)view).RoutableRequestContext != null)
            {
                routableRequestContext = ((IRequiresRoutableRequestContext)view).RoutableRequestContext;
            }
            else if (context.Controller is IRequiresRoutableRequestContext)
            {
                routableRequestContext = ((IRequiresRoutableRequestContext)context.Controller).RoutableRequestContext;
            }
            else
            {
                routableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>();
            }
            return routableRequestContext;
        }
    }
}