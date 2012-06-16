using System;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// Any object registered as an IFilteredViewPageActivator has the ability to declare that it should the the activator to create the view
    /// </summary>
    public interface IFilteredViewPageActivator : IViewPageActivator
    {
        /// <summary>
        /// Determines whether this instance can handle the specified request.
        /// </summary>
        /// <param name="context">The current controller context.</param>
        /// <param name="type">The type that the activator is to instantiate</param>
        /// <returns><c>true</c> if this instance can handle the specified request; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        bool ShouldCreate(ControllerContext context, Type type);

    }
}