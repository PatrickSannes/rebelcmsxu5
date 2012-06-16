using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{
    public class ContentController : BackOfficeController
    {
        public ContentController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        /// <summary>
        /// Gets the nices URL for .
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public JsonResult NiceUrl(HiveId id)
        {
            return Json(new
                {
                    niceUrl = BackOfficeRequestContext.RoutingEngine.GetUrl(id)
                });
        }
    }
}
