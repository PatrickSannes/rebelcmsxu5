using System;
using System.Configuration;
using System.IO;
using System.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework;
using Umbraco.Framework.Tasks;
using File = Umbraco.Framework.Persistence.Model.IO.File;

namespace Umbraco.Cms.Web.Tasks
{
    [Task("A0005CE8-50EA-4A50-9005-DCDB1C11AB4C", "copy-file", ContinueOnFailure = false)]
    public class CopyFileTask : ConfigurationTask
    {
        /// <summary>
        /// delegate to get the current http context
        /// </summary>
        private readonly Func<HttpContextBase> _httpContextResolver;

        /// <summary>
        /// Constructor, uses HttpContext.Current as the http context
        /// </summary>
        /// <param name="configurationTaskContext"></param>
        public CopyFileTask(ConfigurationTaskContext configurationTaskContext)
            : base(configurationTaskContext)
        {
            _httpContextResolver = () => new HttpContextWrapper(HttpContext.Current);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configurationTaskContext"></param>
        /// <param name="httpContextResolver"></param>
        public CopyFileTask(ConfigurationTaskContext configurationTaskContext, Func<HttpContextBase> httpContextResolver)
            : base(configurationTaskContext)
        {
            _httpContextResolver = httpContextResolver;
        }

        public override void Execute(TaskExecutionContext context)
        {
            //ensure the correct parameters exist
            Mandate.That(ConfigurationTaskContext.Parameters.ContainsKey("source"), x => new ArgumentNullException("The 'source' configuration parameter is required for CopyFileTask"));
            Mandate.That(ConfigurationTaskContext.Parameters.ContainsKey("destination"), x => new ArgumentNullException("The 'destination' configuration parameter is required for CopyFileTask"));
            
            //check that paths to see if they're relative
            var sourceString = ConfigurationTaskContext.Parameters["source"];
            var destString = ConfigurationTaskContext.Parameters["destination"];

            var httpContext = _httpContextResolver();

            //delegate to get the absolute path from the source or destination and checks if a relative path has been entered
            //which bases it's path on the configuration elements path.
            Func<string, string, string> getAbsolutePath = (s, packageFolder)
                        => !s.StartsWith("~/")
                            ? Path.Combine(packageFolder, s.Replace("/", "\\"))
                            : httpContext.Server.MapPath(s);

            var sourcePath = getAbsolutePath(sourceString, ConfigurationTaskContext.Task.PackageFolder);
            var destPath = getAbsolutePath(destString, ConfigurationTaskContext.Task.PackageFolder);

            //ensure the source exists
            var source = new FileInfo(sourcePath);
            Mandate.That(source.Exists, x => new FileNotFoundException("The source file " + sourceString + " could not be found"));

            //get the dest, if it exists, remove it
            var dest = new FileInfo(destPath);
            if (dest.Exists)
                dest.Delete();

            var destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrWhiteSpace(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            source.CopyTo(dest.FullName);            
        }
    }
}