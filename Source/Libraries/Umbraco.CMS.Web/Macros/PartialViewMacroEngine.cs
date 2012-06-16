using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Packaging;
using Umbraco.Cms.Web.System;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using File = Umbraco.Framework.Persistence.Model.IO.File;

namespace Umbraco.Cms.Web.Macros
{
    /// <summary>
    /// The PartialView MacroEngine
    /// </summary>
    [UmbracoMacroEngine]
    [MacroEngine("88750F37-ACE7-47C0-B633-2A8BD1544AB5", "PartialView")]
    public class PartialViewMacroEngine : AbstractMacroEngine
    {
        
        public PartialViewMacroEngine()
        {
        }

        public override ActionResult Execute(Content currentNode, IDictionary<string, string> macroParams, MacroDefinition macro, ControllerContext currentControllerContext, IRoutableRequestContext routableRequestContext)
        {
            //If this partial view is part of a package it will be prefixed with the package (area) name.
            //if so, then we need to ensure that the view is rendered in the context of the area so the view is found
            //properly.
            var macroParts = macro.SelectedItem.Split('-');
            var areaName = macroParts.Length > 1 ? macroParts[0] : "";

            var model = new PartialViewMacroModel(currentNode, new BendyObject(macroParams));

            var partialViewResult = new PartialViewResult()
            {
                //if someone has put '-' in the view name, then we need to include those too, we just delimit the area name and view name by the 'first' '-'
                //ViewName = macroParts.Length > 1 
                //        ? string.Join("", macroParts.Where((x, i) => i != 0))
                //        : string.Join("", macroParts),
                ViewName = GetFullViewPath(macro, routableRequestContext),
                ViewData = new ViewDataDictionary(model),
                TempData = new TempDataDictionary(),
            };

            var partialViewControllerContext = currentControllerContext;

            if (!areaName.IsNullOrWhiteSpace())
            {
                //need create a new controller context with the area info
                var areaRouteData = currentControllerContext.RouteData.Clone();
                areaRouteData.DataTokens["area"] = areaName;
                partialViewControllerContext = new ControllerContext(currentControllerContext.HttpContext, areaRouteData, currentControllerContext.Controller);                  
            }

            //wire up the view object/data
            partialViewControllerContext.EnsureViewObjectDataOnResult(partialViewResult);  

            return partialViewResult;
        }

        /// <summary>
        /// Returns the full view path without the extension
        /// </summary>
        /// <param name="macro"></param>
        /// <param name="routableRequestContext"></param>
        /// <returns></returns>
        protected string GetFullViewPath(MacroDefinition macro, IRoutableRequestContext routableRequestContext)
        {
            var macroParts = macro.SelectedItem.Split('-');
            var areaName = macroParts.Length > 1 ? macroParts[0] : "";

            if (areaName.IsNullOrWhiteSpace())
            {
                //TODO: this will obviously not support VB, the only way to do that is to change the macro's SelectedItem to store the extension
                return "~/Views/Umbraco/MacroPartials/" + string.Join("", macroParts) + ".cshtml";
            }
            else
            {
                //create the full path to the macro in it's package folder
                return "~/App_Plugins/Packages/" + areaName + "/Views/MacroPartials/" + macroParts[1] + ".cshtml";
            }
        }

        /// <summary>
        /// Returns the PartialView Macro parameters
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <param name="macroDefinition"></param>
        /// <returns></returns>
        /// <remarks>
        /// Partial view macro parameters are determined by getting an instance of the partial view and looking for it's publicly declared
        /// properties which are done by declaring a region in Razor like so:
        /// @functions {
        ///   public int Age {get;set;}
        /// }
        /// </remarks>
        public override IEnumerable<MacroParameter> GetMacroParameters(IBackOfficeRequestContext backOfficeRequestContext, MacroDefinition macroDefinition)
        {
            using (var uow = backOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                var partialView = uow.Repositories.Get<File>(new HiveId("MacroPartials/" + macroDefinition.SelectedItem));
                if (partialView == null)
                {
                    //localize this exception since it wil be displayed in the UI
                    throw new InvalidOperationException("Macro.ParamsFailedPartialView.Message".Localize());
                }

                var partialMacro = BuildManager.CreateInstanceFromVirtualPath(partialView.RootRelativePath, typeof(PartialViewMacroPage));

                return partialMacro.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Select(x => new MacroParameter
                        {
                            ParameterName = x.Name
                            //TODO: Implement way of detecting which macro param editor to use
                        });
                
            }           
        }

        /// <summary>
        /// Returns all of the PartialViews that can be used for Macros, including all partial views existing in the standard 
        /// Umbraco partials folder and also the ones found inside of packages.
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// Package PartialViews will be prefixed with the Package name '.'
        /// </remarks>
        public override IEnumerable<SelectListItem> GetMacroItems(IBackOfficeRequestContext backOfficeRequestContext)
        {
            using (var uow = backOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://macro-partials")))
            {
                //Get all the partial views in the standard Umbraco partials folder
                var partialViews = uow.Repositories.GetLazyDescendentRelations(new HiveId(new Uri("storage://macro-partials"), "", new HiveIdValue("")), FixedRelationTypes.DefaultRelationType)
                    .Where(x => !((File)x.Destination).IsContainer)
                    .Select(x => new
                        {
                            RootRelativePath = ((File)x.Destination).RootRelativePath,
                            Value = ((File) x.Destination).GetFilePathWithoutExtension(),
                            Name = ((File)x.Destination).GetFilePathForDisplay(),
                            PackageName = ""
                        })
                    .ToList();

                var views = new Dictionary<string, string>();

                //Now find all the partial views in the Packages folder's
                var packageFolders = backOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.GetDirectories(
                    backOfficeRequestContext.PackageContext.LocalPackageManager.LocalRepository.Source)
                    .ToArray();
                foreach(var p in packageFolders)
                {
                    var partialsPath = Path.Combine(p, "Views", "MacroPartials");
                    if (backOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.DirectoryExists(partialsPath))
                    {                        
                        foreach(var v in backOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.GetFiles(partialsPath, "*.cshtml"))
                        {
                            var viewName = Path.GetFileNameWithoutExtension(v);
                            var viewPath = backOfficeRequestContext.Application.Settings.PluginConfig.PluginsPath.TrimEnd('/')
                                           + "/" + PluginManager.PackagesFolderName + "/" + v.Replace("\\", "/");
                            //prefix the view name with the package name (area)
                            partialViews.Add(new
                                {
                                    RootRelativePath = viewPath,
                                    Value = string.Format("{0}-{1}", p, viewName),
                                    Name = string.Format("{0}-{1}", p, viewName),
                                    PackageName = p
                                });
                        }
                    }
                }

                foreach (var p in partialViews)
                {
                    try
                    {
                        var partialMacro = BuildManager.CreateInstanceFromVirtualPath(p.RootRelativePath, typeof(PartialViewMacroPage));
                        views.Add(p.Name, p.Value);
                    }
                    catch (HttpException)
                    {
                        //swallow this exception, this occurs when the partial view doesn't inherit from PartialViewMacro
                    }
                }

                return views.ToArray().Select(x => new SelectListItem {Text = x.Key, Value = x.Value});
            }                        
        }
    }
}