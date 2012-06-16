using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ClientDependency.Core.Mvc;

namespace Umbraco.Cms.Web
{
    public static class HtmlHelperClientDependencyExtensions
    {
        public static HtmlHelper RequiresJsFolder(this HtmlHelper html, string folderPath)
        {
            return html.RequiresJsFolder(folderPath, 100);
        }

        public static HtmlHelper RequiresJsFolder(this HtmlHelper html, string folderPath, int priority)
        {
            var httpContext = html.ViewContext.HttpContext;
            var systemRootPath = httpContext.Server.MapPath("~/");
            var folderMappedPath = httpContext.Server.MapPath(folderPath);

            if (folderMappedPath.StartsWith(systemRootPath))
            {
                var files = Directory.GetFiles(folderMappedPath, "*.js", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    var absoluteFilePath = "~/" + file.Substring(systemRootPath.Length).Replace("\\", "/");
                    html.RequiresJs(absoluteFilePath, priority);
                }
            }

            return html;
        }
    }
}
