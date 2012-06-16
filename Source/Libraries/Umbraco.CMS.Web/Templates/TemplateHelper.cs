using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Framework.Persistence.Model.IO;

namespace Umbraco.Cms.Web.Templates
{
    public class TemplateHelper
    {
        public static string ResolveLayoutPath(string path, File file)
        {
            if (path.StartsWith("~") || path.StartsWith("/"))
                path = HttpContext.Current.Server.MapPath(path);
            else if (file != null)
                path = file.RootedPath.Substring(0, file.RootedPath.LastIndexOf("\\")) + "\\" + path;
            else
                path = HttpContext.Current.Server.MapPath(UmbracoSettings.GetSettings().UmbracoFolders.TemplateFolder) + "\\" + path;

            return path;
        }

        public static Dictionary<string, IList<File>> CreateLayoutFileMap(IEnumerable<File> files)
        {
            var fileMap = new Dictionary<string, IList<File>>
            {
                {"None", new List<File>()}
            };

            foreach (var file in files)
            {
                if (file.Name.ToLower().StartsWith("_viewstart."))
                {
                    fileMap["None"].Add(file);
                }
                else
                {
                    var result = new TemplateParser().Parse(file);
                    var idx = "None";

                    if (!string.IsNullOrWhiteSpace(result.Layout))
                    {
                        idx = ResolveLayoutPath(result.Layout, file);
                    }

                    if (!fileMap.ContainsKey(idx))
                        fileMap.Add(idx, new List<File>());

                    fileMap[idx].Add(file);
                }
            }

            return fileMap;
        }
    }
}
