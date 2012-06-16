using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Routing
{
    public class MediaRouteConstraint : IRouteConstraint
    {
        public virtual bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if(values.ContainsKey("mediaId") && values["mediaId"].ToString().IsGuid(false)
                && values.ContainsKey("fileName") && values["fileName"] != null && !values["fileName"].ToString().IsNullOrWhiteSpace())
            {
                // TODO: Media ID is a guid and we have a filename so check the Hive for a valid file
                return true;
            }

            return false;
        }
    }
}
