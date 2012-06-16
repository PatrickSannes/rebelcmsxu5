using System;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// A filter to generate a HiveId Json path which is stored in a cookie which is referenced by a query string Id. This path is used to sync the tree
    /// in the UI to the path that is generated.
    /// </summary>
    public class SupportsPathGeneration : AbstractTempDataCookieFilter
    {

        public SupportsPathGeneration()
        {
            IsRequired = true;
        }

        public bool IsRequired { get; set; }
    
        /// <summary>
        /// Returns 'path_' to prefix the cookie name
        /// </summary>
        public override string CookieNamePrefix
        {
            get { return "path_"; }
        }

        /// <summary>
        /// Returns 'path' for the query string name
        /// </summary>
        public override string QueryStringName
        {
            get { return "path"; }
        }

        /// <summary>
        /// Uses the IBackOfficeRequestContext request id as the temp data key
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        protected override string GetTempDataKey(ControllerContext filterContext)
        {
            var backOfficeController = GetBackOfficeRequestContext(filterContext.Controller);
            return backOfficeController.BackOfficeRequestContext.RequestId.ToString("N");
        }

        /// <summary>
        /// Returns the path to be stored
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        protected override object GetTempDataValue(ControllerContext filterContext)
        {
            var backOfficeController = GetPathGenerator(filterContext.Controller);
            var path = backOfficeController.GetPaths();
            if (path == null)
            {
                if (IsRequired && filterContext.Controller.ViewData.ModelState.IsValid)
                {
                    throw new NullReferenceException("Any Action/Controller decorated with filter " + GetType().Name + " must return a non-null value for the GetPath() implementation of " + typeof (IGeneratePath).Name + ". This is normally done by setting the path via the GeneratePathForCurrentEntity method.");
                }
                return null;
            }

            return path.ToJson();
        }

        /// <summary>
        /// Gets the IRequiresBackOfficeRequestContext type from the filter context's controller instance
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        protected IRequiresBackOfficeRequestContext GetBackOfficeRequestContext(IController controller)
        {
            var backOfficeController = controller as IRequiresBackOfficeRequestContext;
            if (backOfficeController != null)
            {
                return backOfficeController;
            }
            else
            {
                throw new NotSupportedException("The " + GetType().Name + " can only be used on controllers of type " + typeof(IRequiresBackOfficeRequestContext).Name);
            }
        }

        /// <summary>
        /// Gets the IGeneratePath type from the filter context's controller instance
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        protected IGeneratePath GetPathGenerator(IController controller)
        {
            var backOfficeController = controller as IGeneratePath;
            if (backOfficeController != null)
            {
                return backOfficeController;
            }
            else
            {
                throw new NotSupportedException("The " + GetType().Name + " can only be used on controllers of type " + typeof(IGeneratePath).Name);
            }
        }

    }
}