using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Umbraco.Cms.Web
{
    public static class HttpServerUtilityExtensions
    {

        public static string RelativePath(this HttpServerUtilityBase srv, string path, HttpRequestBase context)
        {
            return path.Replace(context.ServerVariables["APPL_PHYSICAL_PATH"], "/").Replace(@"\", "~/");
        }

    }
}
