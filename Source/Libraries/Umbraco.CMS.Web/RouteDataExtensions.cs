using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Mvc.Controllers;

namespace Umbraco.Cms.Web
{
    public static class RouteDataExtensions
    {
        /// <summary>
        /// Clones the RouteData to a new RouteData object
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public static RouteData Clone(this RouteData routeData)
        {
            var rd = new RouteData();
            foreach(var d in routeData.DataTokens)
            {
                rd.DataTokens.Add(d.Key, d.Value);
            }
            foreach(var v in routeData.Values)
            {
                rd.Values.Add(v.Key, v.Value);
            }
            rd.Route = routeData.Route;
            rd.RouteHandler = routeData.RouteHandler;
            return rd;
        }

        /// <summary>
        /// Returns true if the executing controller is a ControllerExtender
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public static bool IsControllerExtender(this RouteData routeData)
        {
            return (routeData.DataTokens.ContainsKey(ControllerExtender.ParentControllerDataTokenKey));
        }

        /// <summary>
        /// Returns the ControllerContext for the parent Controller of a ControllerExtender
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public static ControllerContext GetControllerExtenderParentContext(this RouteData routeData)
        {
            if (routeData.IsControllerExtender())
            {
                return routeData.DataTokens[ControllerExtender.ParentControllerDataTokenKey] as ControllerContext;
            }
            throw new InvalidOperationException("The executing controller is not executing in a ControllerExtender context");
        }

        /// <summary>
        /// Returns the ExtenderData for the ControllerExtender
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public static object[] GetControllerExtenderParameters(this RouteData routeData)
        {
            if (routeData.IsControllerExtender())
            {
                return routeData.DataTokens[ControllerExtender.ExtenderParametersDataTokenKey] as object[];
            }
            throw new InvalidOperationException("The executing controller is not executing in a ControllerExtender context");
        }


        public static object GetRequiredValue(this RouteData routeData, string key)
        {
            if (routeData.Values.Any(x => x.Key == key))
            {
                return routeData.Values.Where(x => x.Key == key).First().Value;
            }
            throw new InvalidOperationException("The RouteData must contain an item named '" + key + "' with a non-null value");
        }

        public static object GetOptionalValue(this RouteData routeData, string key)
        {
            return routeData.Values.Any(x => x.Key == key) 
                ? routeData.Values.Where(x => x.Key == key).First().Value 
                : null;
        }

        public static string GetOptionalString(this RouteData routeData, string key)
        {
            if (routeData.Values.Any(x => x.Key == key))
            {
                if (routeData.Values.Where(x => x.Key == key).First().Value is string)
                {
                    return routeData.GetRequiredString(key);   
                }                
                throw new InvalidOperationException("The value found with the specified key is not a string");
            }
            return "";
        }

    }
}
