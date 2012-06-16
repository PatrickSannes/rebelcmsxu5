using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Tasks;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    public class SupportModelEditingAttribute : ActionFilterAttribute
    {
        private readonly IFrameworkContext _frameworkContext;

        public SupportModelEditingAttribute()
        {
            _frameworkContext = DependencyResolver.Current.GetService<IFrameworkContext>();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result is ViewResult)
            {
                var viewResult = (ViewResult) filterContext.Result;
                _frameworkContext.TaskManager.ExecuteInContext(TaskTriggers.ModelPreSendToView, 
                    this, 
                    new TaskEventArgs(_frameworkContext, new ModelEventArgs(viewResult.Model)));
            }
        }
    }

    public class ModelEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelEventArgs"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ModelEventArgs(dynamic model)
        {
            Model = model;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public dynamic Model { get; protected set; }
    }
}
