using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Surface;
using Umbraco.Framework;

namespace Umbraco.Tests.Cms.Stubs.Surface
{
    [Surface("E28D5B50-7BC6-4615-A3A9-BE2F29874E68")]
    internal class CustomSurfaceController : SurfaceController
    {
        public CustomSurfaceController(IBackOfficeRequestContext routableRequestContext)
            : base(routableRequestContext) { }

        public ActionResult Index(object id)
        {
            return null;
        }

        public ActionResult SomeAction(object id)
        {
            return null;
        }
    }
}
