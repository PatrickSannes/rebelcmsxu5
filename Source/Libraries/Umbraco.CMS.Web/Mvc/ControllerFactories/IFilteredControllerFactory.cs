using System;
using System.Collections.Concurrent;
using System.Web.Routing;

namespace Umbraco.Cms.Web.Mvc.ControllerFactories
{
    public interface IFilteredControllerFactory : IExtendedControllerFactory
    {
        /// <summary>
        /// Determines whether this instance can handle the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns><c>true</c> if this instance can handle the specified request; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        bool CanHandle(RequestContext request);
    }
}
