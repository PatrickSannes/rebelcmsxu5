using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;

namespace Umbraco.Cms.Web.Mvc.ActionInvokers
{
    public class RoutableRequestActionInvoker : ControllerActionInvoker
    {
        private IRoutableRequestContext _routableRequestContext;
        protected IRoutableRequestContext RoutableRequestContext
        {
            get
            {
                if (_routableRequestContext == null)
                {
                    _routableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>();
                }
                return _routableRequestContext;
            }
            set { _routableRequestContext = value; }
        }

        public RoutableRequestActionInvoker(IRoutableRequestContext routableRequestContext)
        {
            RoutableRequestContext = routableRequestContext;
        }

        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor);
            foreach (var filter in filters.AuthorizationFilters.Cast<object>()
                .Concat(filters.ActionFilters.Cast<object>())
                .Concat(filters.ExceptionFilters.Cast<object>())
                .Concat(filters.ResultFilters.Cast<object>()))
            {
                var filterType = filter.GetType();
                if (typeof(IRequiresRoutableRequestContext).IsAssignableFrom(filterType))
                {
                    ((IRequiresRoutableRequestContext)filter).RoutableRequestContext = RoutableRequestContext;
                }
            }
            return filters;
        }
    }
}
