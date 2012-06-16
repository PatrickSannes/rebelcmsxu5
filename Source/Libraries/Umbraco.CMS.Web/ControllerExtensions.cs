using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web
{
    public static class ControllerExtensions
    {

        /// <summary>
        /// Executes an Action on the specified controller and ensures that
        /// the proxied controller's context and Url helper, as well as other things  are set up.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TControllerType"></typeparam>
        /// <param name="controller"></param>
        /// <param name="methodSelector"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public static TResult ProxyRequestToController<TResult, TControllerType>(
            this ControllerBase controller,
            Expression<Func<TControllerType, TResult>> methodSelector,
            string area = "")
            where TControllerType : class
            where TResult : ActionResult
        {
            Mandate.ParameterNotNull(methodSelector, "methodSelector");
            var proxyController = DependencyResolver.Current.GetService<TControllerType>();
            Mandate.That(TypeFinder.IsTypeAssignableFrom<ControllerBase>(proxyController), x => new InvalidOperationException("TControllerType must be of type ControllerBase"));
            var methodInfo = Umbraco.Framework.ExpressionHelper.GetMethodInfo(methodSelector);
            var ctrlContext = InjectControllerPropertiesForProxying(controller, proxyController as ControllerBase, methodInfo, area);
            var result = methodSelector.Compile().Invoke(proxyController);
            if (result is ViewResultBase)
            {
                ctrlContext.EnsureViewObjectDataOnResult(result as ViewResultBase);
            }
            return result;
        }

        /// <summary>
        /// Executes an Action on the specified controller and ensures that
        /// the proxied controller's context and Url helper, as well as other things  are set up.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TControllerType"></typeparam>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="methodSelector"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public static TResult ProxyRequestToController<TResult, TControllerType>(
            this ControllerBase controller,
            TControllerType proxyController,
            Expression<Func<TControllerType, TResult>> methodSelector,
            string area = "")
            where TResult : ActionResult
            where TControllerType : class
        {
            Mandate.ParameterNotNull(proxyController, "proxyController");
            Mandate.ParameterNotNull(methodSelector, "methodSelector");
            Mandate.That(TypeFinder.IsTypeAssignableFrom<ControllerBase>(proxyController), x => new InvalidOperationException("TControllerType must be of type ControllerBase"));
            var methodInfo = Umbraco.Framework.ExpressionHelper.GetMethodInfo(methodSelector);
            var ctrlContext = InjectControllerPropertiesForProxying(controller, proxyController as ControllerBase, methodInfo, area);
            var result = methodSelector.Compile().Invoke(proxyController);
            if (result is ViewResultBase)
            {
                ctrlContext.EnsureViewObjectDataOnResult(result as ViewResultBase);
            }
            return result;
        }      

        /// <summary>
        /// Executes an Action on the specified controller and ensures that
        /// the proxied controller's context and Url helper, as well as other things  are set up.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="childAction"></param>
        /// <param name="actionParams"></param>
        /// <returns></returns>
        public static ActionResult ProxyRequestToController(
            this ControllerBase controller,
            ControllerBase proxyController,
            MethodInfo childAction,
            params object[] actionParams)
        {
            return controller.ProxyRequestToController(proxyController, childAction, "", actionParams);
        }

        /// <summary>
        /// Executes an Action on the specified controller and ensures that
        /// the proxied controller's context and Url helper, as well as other things  are set up.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="childAction"></param>
        /// <param name="area"></param>
        /// <param name="actionParams"></param>
        /// <returns></returns>
        public static ActionResult ProxyRequestToController(
            this ControllerBase controller,
            ControllerBase proxyController,
            MethodInfo childAction,
            string area,
            params object[] actionParams)
        {
            Mandate.ParameterNotNull(proxyController, "proxyController");
            Mandate.ParameterNotNull(childAction, "childAction");

            if (!TypeFinder.IsTypeAssignableFrom<ActionResult>(childAction.ReturnType))
            {
                throw new InvalidOperationException("The MethodInfo passed in must have a return type of ActionResult");
            }

            var ctrlContext = InjectControllerPropertiesForProxying(controller, proxyController, childAction, area);
            var result = (ActionResult)childAction.Invoke(proxyController, actionParams);
            if (result is ViewResultBase)
            {
                ctrlContext.EnsureViewObjectDataOnResult((ViewResultBase)result);
            }
            return result;
        }

        /// <summary>
        /// Executes an Action the specified controller by finding the action specified by name. 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="action"></param>
        /// <param name="actionParams"></param>
        /// <returns></returns>
        /// <remarks>
        /// If a proxy request is made to a custom controller with a custom ActionInvoker, the ActionInvoker will be ignored, this
        /// method simply finds the action by name.
        /// </remarks>
        public static ActionResult ProxyRequestToController(
            this ControllerBase controller,
            ControllerBase proxyController,
            string action,
            params object[] actionParams)
        {
            return controller.ProxyRequestToController(proxyController, action, "", actionParams);
        }

        /// <summary>
        /// Executes an Action the specified controller by finding the action specified by name. 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="action"></param>
        /// <param name="area"></param>
        /// <param name="actionParams"></param>
        /// <returns></returns>
        /// <remarks>
        /// If a proxy request is made to a custom controller with a custom ActionInvoker, the ActionInvoker will be ignored, this
        /// method simply finds the action by name.
        /// </remarks>
        public static ActionResult ProxyRequestToController(
            this ControllerBase controller,
            ControllerBase proxyController,
            string action,
            string area,
            params object[] actionParams)
        {
            Mandate.ParameterNotNull(proxyController, "proxyController");
            Mandate.ParameterNotNullOrEmpty(action, "action");

            Func<ParameterInfo[], bool> parametersMatch = p =>
            {
                if (p.Count() != actionParams.Count())
                    return false;
                //ensure the param types match based on index too
                if (actionParams.Select(t => t.GetType()).Where((aType, i) => !TypeFinder.IsTypeAssignableFrom(p[i].ParameterType, aType)).Any())
                {
                    return false;
                }
                return true;
            };

            var method = (from m in proxyController.GetType().GetMethods()
                          let p = m.GetParameters()
                          where m.Name.InvariantEquals(action) && TypeFinder.IsTypeAssignableFrom<ActionResult>(m.ReturnType) && parametersMatch(p)
                          select m).SingleOrDefault();

            if (method == null)
            {
                throw new InvalidOperationException("Could not find an Action with the name " + action + " with matching parameters (" + actionParams.Count() + ")");
            }
            return controller.ProxyRequestToController(proxyController, method, area, actionParams);
        }

        /// <summary>
        /// Sets the ControllerContext and UrlHelper on the destination controller if the types given have those properties available.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="action"></param>
        /// <param name="area">If the proxying should be to a different area, specify the area name</param>
        private static ControllerContext InjectControllerPropertiesForProxying(
            this ControllerBase controller, 
            ControllerBase proxyController, 
            MethodInfo action,
            string area = "")
        {

            //clone new route data to to execute the action
            var routeData = controller.ControllerContext.RouteData.Clone();
            if (!area.IsNullOrWhiteSpace())
            {
                routeData.DataTokens["area"] = area;
            }
            routeData.Values["action"] = action.Name;
            routeData.Values["controller"] = UmbracoController.GetControllerName(proxyController.GetType());
            var isChildAction = action.GetCustomAttributes(typeof(ChildActionOnlyAttribute), false).Any();
            if (isChildAction)
            {
                //this is how the controller context determines if it is rendering a child action
                //we need to pass in the current (Parent) controller's ViewContext object
                routeData.DataTokens["ParentActionViewContext"] = controller.ControllerContext.CreateEmptyViewContext();
            }
            //create a new controller context to execute the proxied action
            var ctrlContext = new ControllerContext(controller.ControllerContext.HttpContext, routeData, proxyController);

            proxyController.ControllerContext = ctrlContext;
            
            if (proxyController is Controller && controller is Controller)
            {
                ((Controller) proxyController).Url = ((Controller) controller).Url;
            }

            return ctrlContext;
        }

        /// <summary>
        /// Executes the secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <returns></returns>
        public static void ExecuteSecuredMethod<TControllerType>(this TControllerType controller,
            Expression<Action<TControllerType>> methodSelector)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var result = controller.TryExecuteSecuredMethod(methodSelector);
            if (result.ExecutionError != null) throw result.ExecutionError;
        }

        /// <summary>
        /// Executes the secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public static void ExecuteSecuredMethod<TControllerType>(this TControllerType controller,
            Expression<Action<TControllerType>> methodSelector, HiveId entityId)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var result = controller.TryExecuteSecuredMethod(methodSelector, entityId);
            if (result.ExecutionError != null) throw result.ExecutionError;
        }

        /// <summary>
        /// Executes the secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <typeparam name="TResultType">The type of the result type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <returns></returns>
        public static void ExecuteSecuredMethod<TControllerType, TResultType>(this TControllerType controller,
            Expression<Func<TControllerType, TResultType>> methodSelector)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null); 
            
            var result = controller.TryExecuteSecuredMethod(methodSelector);
            if (result.ExecutionError != null) throw result.ExecutionError;
        }

        /// <summary>
        /// Executes the secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <typeparam name="TResultType">The type of the result type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public static void ExecuteSecuredMethod<TControllerType, TResultType>(this TControllerType controller,
            Expression<Func<TControllerType, TResultType>> methodSelector, HiveId entityId)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var result = controller.TryExecuteSecuredMethod(methodSelector, entityId);
            if (result.ExecutionError != null) throw result.ExecutionError;
        }

        /// <summary>
        /// Tries to execute a secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <returns></returns>
        public static SecuredMethodResult TryExecuteSecuredMethod<TControllerType>(this TControllerType controller,
            Expression<Action<TControllerType>> methodSelector)
            where TControllerType : ControllerBase
        {
            return controller.TryExecuteSecuredMethod(methodSelector, HiveId.Empty);
        }

        /// <summary>
        /// Tries to execute a secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public static SecuredMethodResult TryExecuteSecuredMethod<TControllerType>(this TControllerType controller,
            Expression<Action<TControllerType>> methodSelector, HiveId entityId)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var authorised = controller.IsMethodAuthorized((MethodCallExpression)methodSelector.Body, entityId);

            if (!authorised)
                return SecuredMethodResult.False;

            try
            {
                methodSelector.Compile().Invoke(controller);
                return new SecuredMethodResult(true, true);
            }
            catch (Exception ex)
            {
                return new SecuredMethodResult(ex);
            }
        }

        /// <summary>
        /// Tries to execute a secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller.</typeparam>
        /// <typeparam name="TResultType">The type of the result.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <returns></returns>
        public static SecuredMethodResult<TResultType> TryExecuteSecuredMethod<TControllerType, TResultType>(this TControllerType controller,
            Expression<Func<TControllerType, TResultType>> methodSelector)
            where TControllerType : ControllerBase
        {
            return controller.TryExecuteSecuredMethod(methodSelector, HiveId.Empty);
        }

        /// <summary>
        /// Tries to execute a secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <typeparam name="TResultType">The type of the result type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public static SecuredMethodResult<TResultType> TryExecuteSecuredMethod<TControllerType, TResultType>(this TControllerType controller,
            Expression<Func<TControllerType, TResultType>> methodSelector, HiveId entityId)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var authorised = controller.IsMethodAuthorized((MethodCallExpression)methodSelector.Body, entityId);

            if (!authorised)
                return SecuredMethodResult<TResultType>.False;

            try
            {
                return new SecuredMethodResult<TResultType>(true, true, methodSelector.Compile().Invoke(controller));
            }
            catch (Exception ex)
            {
                return new SecuredMethodResult<TResultType>(ex);
            }
        }

        /// <summary>
        /// Determines whether the method call expression is authorized on the specified controller.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="mce">The mce.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns>
        ///   <c>true</c> if the is method authorized on the specified controller; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsMethodAuthorized<TControllerType>(this TControllerType controller, MethodCallExpression mce, HiveId entityId)
            where TControllerType : ControllerBase
        {
            var methodInfo = mce.Method;
            var attributes = methodInfo.GetCustomAttributes(typeof(UmbracoAuthorizeAttribute), true);
            var success = true; // If no authorize attributes, authorize by default

            // Validate attributes
            if (attributes.Length > 0)
            {
                success = false; // We have authorize attributes so set to false unless we are authorized

                foreach (UmbracoAuthorizeAttribute attribute in attributes)
                {
                    if (controller is BackOfficeController)
                        attribute.RoutableRequestContext = (controller as BackOfficeController).BackOfficeRequestContext;

                    var id = entityId;

                    if (id.IsNullValueOrEmpty())
                    {
                        // Try to get id from method info
                        var parameters = methodInfo.GetParameters();
                        var idParameter = parameters.FirstOrDefault(x => x.Name == attribute.IdParameterName && x.ParameterType == typeof(HiveId));
                        if (idParameter != null)
                        {
                            var arg = mce.Arguments[idParameter.Position];
                            id = (HiveId)Expression.Lambda(Expression.Convert(arg, arg.Type)).Compile().DynamicInvoke();
                        }

                        // Try to get id from route data
                        else if (controller.ControllerContext.RouteData != null
                            && controller.ControllerContext.RouteData.Values != null
                            && controller.ControllerContext.RouteData.Values.ContainsKey(attribute.IdParameterName))
                            id = new HiveId(controller.ControllerContext.RouteData.Values[attribute.IdParameterName].ToString());

                        // Try to get id from request collection
                        else if (controller.ControllerContext.HttpContext.Request != null
                            && !string.IsNullOrWhiteSpace(controller.ControllerContext.HttpContext.Request[attribute.IdParameterName]))
                            id = new HiveId(controller.ControllerContext.HttpContext.Request[attribute.IdParameterName]);

                        else
                            id = FixedHiveIds.SystemRoot;
                    }

                    success = success || attribute.IsAuthorized(controller.ControllerContext.HttpContext, id);
                }
            }

            return success;
        }
    }

    /// <summary>
    /// Represents the result of a secured method execution with return value.
    /// </summary>
    /// <typeparam name="TResultType">The type of the result type.</typeparam>
    public class SecuredMethodResult<TResultType> : SecuredMethodResult
    {
        public static readonly SecuredMethodResult<TResultType> False = new SecuredMethodResult<TResultType>();

        private SecuredMethodResult()
            : base(false, false)
        {
            
        }

        public SecuredMethodResult(bool authorized, bool success, TResultType result)
            : base(authorized, success)
        {
            Result = result;
        }

        public SecuredMethodResult(Exception error)
            : base(error)
        { }

        public TResultType Result { get; protected set; }
    }

    /// <summary>
    /// Represents the result of a secured method execution.
    /// </summary>
    public class SecuredMethodResult
    {
        public static readonly SecuredMethodResult False = new SecuredMethodResult();

        private SecuredMethodResult() : this(false, false)
        {}

        public SecuredMethodResult(bool authorized, bool success)
        {
            Authorized = authorized;
            Success = success;
        }

        public SecuredMethodResult(Exception executionError)
            : this(true, false)
        {
            ExecutionError = executionError;
        }

        public bool Authorized { get; protected set; }
        public bool Success { get; protected set; }
        public Exception ExecutionError { get; protected set; }
    }
}
