using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Umbraco.Cms.Web.Mvc.Controllers;

namespace Umbraco.Cms.Web.Mvc.ActionInvokers
{
    /// <summary>
    /// An action invoker allowing Controller Extender actions to execute.
    /// </summary>
    public class ControllerExtenderActionInvoker : ControllerActionInvoker
    {
        /// <summary>
        /// Checks if the action to execute exists on the main controller, if not checks any extender controllers for matching/executable actions and returns them.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="controllerDescriptor"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            var baseResult = base.FindAction(controllerContext, controllerDescriptor, actionName);
            if (baseResult == null)
            {
                //check if the controller is attributed with the extender
                foreach(var a in controllerDescriptor.ControllerType
                    .GetCustomAttributes(typeof (ExtendedByAttribute), true)
                    .OfType<ExtendedByAttribute>())                
                {
                    //though this gets called each request, the underlying collection uses ConcurrentDictionary so is thread safe
                    ControllerExtender.RegisterExtender(controllerContext.Controller, a.ControllerExtenderType, a.AdditionalParameters);                        
                }

                //get all extender actions for this controller matching the action name
                var extenderActions = ControllerExtender.GetRegistrations()
                    .Where(x => x.Key.ParentType == controllerContext.Controller.GetType())
                    .Where(x => x.Key.ExtenderType.GetMethods().Any(m => m.Name == actionName)).ToArray();
                //no extender actions found, return the base result (null)
                if (!extenderActions.Any())
                {
                    return baseResult;
                }
                //so now we have a list of potential extenders to use, we'll need to use the underlying system to determine 
                //which one is suitable (if any) since the .Net framework as the actual code to select actions (ReflectedControllerDescriptor)
                //is marked as internal
                foreach(var c in extenderActions)
                {
                    var extender = c.Value.Extender();
                    var ctlContext = new ControllerContext(controllerContext.RequestContext, extender);
                    var ctlDescriptor = GetControllerDescriptor(ctlContext);
                    var a = ctlDescriptor.FindAction(ctlContext, actionName);
                    if (a == null) continue;

                    return new ExtenderActionDescriptor(a, ctlContext, controllerContext, c.Value.AdditionalParameters);
                }
                return baseResult;
            }

            return baseResult;
        }

        /// <summary>
        /// Override the InvokeAction method to check if the actionDescriptor is an ExtenderActionDescriptor
        /// if it is, we will actually execute the controller for the extender action seperately
        /// so that everything is wired up correctly and all of the ActionFilters are executed.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public override bool InvokeAction(ControllerContext controllerContext, string actionName)
        {
            if (controllerContext == null) throw new ArgumentNullException("controllerContext");
            if (actionName == null) throw new ArgumentNullException("actionName");

            var controllerDescriptor = GetControllerDescriptor(controllerContext);
            var action = FindAction(controllerContext, controllerDescriptor, actionName);
            if (action is ExtenderActionDescriptor)
            {
                var extenderDescriptor = (ExtenderActionDescriptor) action;
                var controller = (IController) extenderDescriptor.ExtenderControllerContext.Controller;
                
                //store the reference to the parent controller context in data tokens
                extenderDescriptor.ExtenderControllerContext.RequestContext.RouteData.DataTokens.Add(
                    ControllerExtender.ParentControllerDataTokenKey, 
                    extenderDescriptor.ParentControllerContext);
                //store the reference to the ExtenderData object in data tokens
                extenderDescriptor.ExtenderControllerContext.RequestContext.RouteData.DataTokens.Add(
                    ControllerExtender.ExtenderParametersDataTokenKey,
                    extenderDescriptor.AdditionalParameters);

                try
                {
                    controller.Execute(extenderDescriptor.ExtenderControllerContext.RequestContext);
                    return true;
                }
                finally
                {
                    if (controller is IDisposable)
                        ((IDisposable) controller).Dispose();
                }
            }
            
            return base.InvokeAction(controllerContext, actionName);         
        }

        /// <summary>
        /// Internal class wrapping the real ActionDescriptor so that we can store the internal controller context for the 
        /// controller that will actually be doing the execution
        /// </summary>
        private class ExtenderActionDescriptor : ActionDescriptor
        {
            public ControllerContext ExtenderControllerContext { get; private set; }
            public ControllerContext ParentControllerContext { get; private set; }
            public object[] AdditionalParameters { get; private set; }
            private readonly ActionDescriptor _internalDescriptor;

            public ExtenderActionDescriptor(
                ActionDescriptor internalDescriptor, 
                ControllerContext extenderControllerContext, 
                ControllerContext parentControllerContext,
                object[] additionalParameters)
            {
                ExtenderControllerContext = extenderControllerContext;
                ParentControllerContext = parentControllerContext;
                AdditionalParameters = additionalParameters;
                _internalDescriptor = internalDescriptor;
            }

            public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters)
            {
                return _internalDescriptor.Execute(controllerContext, parameters);
            }

            public override ParameterDescriptor[] GetParameters()
            {
                return _internalDescriptor.GetParameters();
            }

            public override string ActionName
            {
                get { return _internalDescriptor.ActionName; }
            }

            public override ControllerDescriptor ControllerDescriptor
            {
                get { return _internalDescriptor.ControllerDescriptor; }
            }

        }

    }
}