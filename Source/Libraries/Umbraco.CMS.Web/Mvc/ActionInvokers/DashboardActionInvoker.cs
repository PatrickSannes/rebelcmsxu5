using System;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration.Dashboards;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;

namespace Umbraco.Cms.Web.Mvc.ActionInvokers
{
    /// <summary>
    /// An action invoker that ensures that all of the dashboard filters are applied to the Dashboard action
    /// </summary>
    public class DashboardActionInvoker : UmbracoBackOfficeActionInvoker
    {
        public DashboardActionInvoker(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
        }
        
        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var standardFilters = base.GetFilters(controllerContext, actionDescriptor);
            if (actionDescriptor.ActionName == "RenderDashboard")
            {
                //get the filters applied to this 
                var paramValues = GetParameterValues(controllerContext, actionDescriptor);
                var model = (DashboardItemModel) paramValues["model"];
                foreach (var m in model.Matches.Cast<MatchElement>())
                {
                    //get the match rule from the registered plugins
                    var matchRule = BackOfficeRequestContext.RegisteredComponents.DashboardMatchRules
                        .Where(x => x.Metadata.ComponentType == m.MatchType)
                        .SingleOrDefault();
                    if (matchRule == null) 
                        throw new TypeLoadException("The dashboard match type " + m.MatchType.FullName + " was not found in the IoC container");
                    
                    //check if it matches by running it through the match processor
                    if (matchRule.Value.IsMatch(m.Condition))
                    {
                        //if the rule matches, we'll create and add our filters
                        foreach (var f in m.Filters.Cast<MatchFilterElement>())
                        {
                            var filter = BackOfficeRequestContext.RegisteredComponents.DashboardFilters
                                .Where(x => x.Metadata.ComponentType == f.MatchFilterType)
                                .SingleOrDefault();
                            if (filter == null)
                                throw new TypeLoadException("The match filter type " + m.MatchType.FullName + " was not found in the IoC container");

                            //initialize the filter with the parameters it needs and the allow/deny
                            var initializedFilter = filter.Value;
                            initializedFilter.Initialize(f.DataValue, f.Action);
                            //now add the filter to the outgoing list
                            standardFilters.ActionFilters.Add(initializedFilter);
                            standardFilters.ResultFilters.Add(initializedFilter);
                        }
                    }
                }
            }

            return standardFilters;
        }
    }
}