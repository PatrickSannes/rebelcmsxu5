using System;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// Any object registered as an IPostViewPageActivator can take part in the view page activation after its created
    /// </summary>
    public interface IPostViewPageActivator
    {
        /// <summary>
        /// Executes when the view is created allowing the modification of the view before being returned by the activator
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="view"></param>
        void OnViewCreated(ControllerContext context, Type type, object view);

    }
}