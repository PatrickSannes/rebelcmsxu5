using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ControllerFactories
{
    /// <summary>
    /// A controller factory which uses an internal list of <see cref="IFilteredControllerFactory"/> in order to invoke 
    /// different controller factories dependent upon their implementation of <see cref="IFilteredControllerFactory.CanHandle"/> for the current
    /// request. Allows circumvention of MVC3's singly registered IControllerFactory.
    /// </summary>
    /// <remarks></remarks>
    public class MasterControllerFactory : DefaultControllerFactory
    {
        private readonly HashSet<IFilteredControllerFactory> _slaveFactories;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        private readonly ConcurrentDictionary<string, Type> _controllerCache = new ConcurrentDictionary<string, Type>(); 

        protected MasterControllerFactory()
        {
            _slaveFactories = new HashSet<IFilteredControllerFactory>();
        }

        public MasterControllerFactory(IEnumerable<IFilteredControllerFactory> controllerFactories) : this()
        {
            _slaveFactories = new HashSet<IFilteredControllerFactory>(controllerFactories);
        }

        /// <summary>
        /// Registers the factory.
        /// </summary>
        /// <param name="slaveFactory">The slave factory.</param>
        /// <remarks></remarks>
        public void RegisterFactory(IFilteredControllerFactory slaveFactory)
        {
            using (new WriteLockDisposable(_locker))
            {
                if (slaveFactory != null && !_slaveFactories.Contains(slaveFactory))
                {
                    _slaveFactories.Add(slaveFactory);
                }
            }
        }

        /// <summary>
        /// Creates the specified controller by using the specified request context.
        /// </summary>
        /// <param name="requestContext">The context of the HTTP request, which includes the HTTP context and route data.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <returns>The controller.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="requestContext"/> parameter is null.</exception>
        ///   
        /// <exception cref="T:System.ArgumentException">The <paramref name="controllerName"/> parameter is null or empty.</exception>
        /// <remarks></remarks>
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            var factory = _slaveFactories.FirstOrDefault(x => x.CanHandle(requestContext));
            return factory != null ? factory.CreateController(requestContext, controllerName) : base.CreateController(requestContext, controllerName);
        }

        protected override global::System.Type GetControllerType(RequestContext requestContext, string controllerName)
        {
            var type = base.GetControllerType(requestContext, controllerName);
            if (type == null)
            {

                //TODO: if it is null, then check if we should check the BuildManager for the type... this will be caused by loading in a plugin
                //which bumps the web.config but doesn't touch the bin files which doesn't fully recycle the app pool. See this post here:
                // http://umbraco5contrib.codeplex.com/discussions/281446
                // for now, we'll get the type from the BuildManager and return it if it is found but this isn't quite ideal.
                
                //only do this if one namespace is specified
                object namespaceToken;
                if (requestContext.RouteData.DataTokens.TryGetValue("Namespaces", out namespaceToken))
                {
                    try
                    {
                        var namespaces = (string[]) namespaceToken;
                        if (namespaces.Length == 1)
                        {
                            var typeName = namespaces[0] + "." + controllerName + "Controller";
                            type = _controllerCache.GetOrAdd(typeName, 
                                x => global::System.Web.Compilation.BuildManager.GetType(namespaces[0] + "." + controllerName + "Controller", false));
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            return type;
        }

        /// <summary>
        /// Releases the specified controller.
        /// </summary>
        /// <param name="controller">The controller to release.</param>
        /// <remarks></remarks>
        public override void ReleaseController(IController controller)
        {
            using (new WriteLockDisposable(_locker))
            {
                bool released = false;
                if (controller is Controller)
                {
                    var requestContext = ((Controller) controller).ControllerContext.RequestContext;
                    var factory = _slaveFactories.FirstOrDefault(x => x.CanHandle(requestContext));
                    if (factory != null)
                    {
                        factory.ReleaseController(controller);
                        released = true;
                    }
                }
                if (!released) base.ReleaseController(controller);
            }
        }
    }
}