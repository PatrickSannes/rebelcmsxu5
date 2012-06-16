using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using NuGet;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Packaging;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Tasks;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

//[assembly: WebResource("Umbraco.Cms.Packaging.Editors.Resources.Packaging.css", "text/css", PerformSubstitution = true)]
//[assembly: WebResource("Umbraco.Cms.Packaging.Editors.Resources.PackagingInstaller.js", "application/x-javascript")]
//[assembly: WebResource("Umbraco.Cms.Packaging.Editors.Resources.AppRestarter.js", "application/x-javascript")]

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.PackagingEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class PackagingEditorController : DashboardEditorController
    {
        private readonly PackageInstallUtility _packageInstallUtility;

        public PackagingEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _packageInstallUtility = new PackageInstallUtility(requestContext);
        }

        /// <summary>
        /// Displays the public repository
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PublicRepository()
        {
            return View();
        }

        /// <summary>
        /// Displays the local repository
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LocalRepository()
        {

            return View("LocalRepository", GetLocalPackages());
        }

        /// <summary>
        /// An action to recycle app domain
        /// </summary>
        /// <param name="id">The Hive id of the package</param>
        /// <param name="state">If it is installing or uninstalling</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RecycleApplication(string id, PackageInstallationState state)
        {
            if (id == null) throw new ArgumentNullException("id");
            return View(new PackageInstallerStateModel {PackageId = id, State = state});
        }

        /// <summary>
        /// Actually does the shutting down of the app which is called by ajax, if onlyCheck is true, this will just
        /// return the status
        /// </summary>
        /// <param name="onlyCheck"></param> 
        /// <returns></returns>
        [HttpPost]
        public JsonResult PerformRecycleApplication(bool onlyCheck)
        {
            if (!onlyCheck)
            {
                UmbracoApplicationContext.RestartApplicationPool(HttpContext);
                return Json(new { status = "restarting" });
            }

            return Json(new { status = "restarted" });
        }

        /// <summary>
        /// Handles the post back for installing/removing/deleting local packages
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [SuccessfulOnRedirect]
        [Save("install", "uninstall", "remove")]
        public ActionResult ManagePackage()
        {

            var toInstallVal = ValueProvider.GetValue("install") == null 
                ? new object[] { } 
                : ValueProvider.GetValue("install").AttemptedValue.Split('-');
            var toInstall = toInstallVal.Length > 0 ? toInstallVal[0] : null;

            var toUninstallVal = ValueProvider.GetValue("uninstall") == null
                ? new object[] { }
                : ValueProvider.GetValue("uninstall").AttemptedValue.Split('-');
            var toUninstall = toUninstallVal.Length > 0 ? toUninstallVal[0] : null;

            var toRemoveVal = ValueProvider.GetValue("remove") == null
                ? new object[] { }
                : ValueProvider.GetValue("remove").AttemptedValue.Split('-');
            var toRemove = toRemoveVal.Length > 0 ? toRemoveVal[0] : null;

            if (toInstall != null)
            {
                //get the package from the source
                var package = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(toInstall.ToString());
                var version = Version.Parse(toInstallVal[1].ToString());
                BackOfficeRequestContext.PackageContext.LocalPackageManager.InstallPackage(package.Id, version, false);

                Notifications.Add(new NotificationMessage(package.Title + " has been installed", "Package installed", NotificationType.Success));
                SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);

                return RedirectToAction("RecycleApplication", new { id = package.Id, state = PackageInstallationState.Installing });
            }

            if (toUninstall != null)
            {
                //get the package from the installed location
                var nugetPackage = BackOfficeRequestContext.PackageContext.LocalPackageManager.LocalRepository.FindPackage(toUninstall.ToString());
                var packageFolderName = BackOfficeRequestContext.PackageContext.LocalPathResolver.GetPackageDirectory(nugetPackage);
                
                //execute some tasks
                var taskExeContext = _packageInstallUtility.GetTaskExecutionContext(nugetPackage, packageFolderName, PackageInstallationState.Uninstalling, this);
                _packageInstallUtility.RunPrePackageUninstallActions(taskExeContext, packageFolderName);

                BackOfficeRequestContext.PackageContext.LocalPackageManager.UninstallPackage(nugetPackage, false, false);

                Notifications.Add(new NotificationMessage(nugetPackage.Title + " has been uninstalled", "Package uninstalled", NotificationType.Success));
                SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", nugetPackage.Id);

                return RedirectToAction("RecycleApplication", new { id = nugetPackage.Id, state = PackageInstallationState.Uninstalling });
            }
            
            if (toRemove != null)
            {

                var package = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(toRemove.ToString());
                var packageFile = BackOfficeRequestContext.PackageContext.LocalPathResolver.GetPackageFileName(package);


                //delete the package folder... this will check if the file exists by just the package name or also with the version
                if (BackOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.FileExists(packageFile))
                {
                    BackOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.DeleteFile(
                        Path.Combine(BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.Source, packageFile));    
                }
                else
                {
                    var fileNameWithVersion = packageFile.Substring(0, packageFile.IndexOf(".nupkg")) + "." + package.Version + ".nupkg";
                    BackOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.DeleteFile(
                        Path.Combine(BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.Source, fileNameWithVersion));    
                }
                    
                
                
                Notifications.Add(new NotificationMessage(package.Title + " has been removed from the local repository", "Package removed", NotificationType.Success));
                SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);
                return RedirectToAction("LocalRepository");

            }


            return HttpNotFound();
        }

        /// <summary>
        /// Handles uploading a new local package
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [SuccessfulOnRedirect(IsRequired = false)]
        [Save("upload")]
        public ActionResult AddLocalPackage(HttpPostedFileBase file)
        {
            if(file == null)
            {
                ModelState.AddModelError("PackageFileValidation", "No file selected. Please select a package file to upload.");
                return LocalRepository();
            }

            if (!Path.GetExtension(file.FileName).EndsWith("nupkg"))
            {
                ModelState.AddModelError("PackageFileValidation", "The file uploaded is not a valid package file, only Nuget packages are supported");
                return LocalRepository();
            }

            IPackage package;
            try
            {
                package = new ZipPackage(file.InputStream);
            }
            catch (Exception ex)
            {
                LogHelper.Error<PackagingEditorController>("Package could not be unziped.", ex);

                ModelState.AddModelError("PackageFileValidation", "The Nuget package file uploaded could not be read");
                return LocalRepository();
            }

            try
            {
                var fileName = Path.Combine(BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.Source, file.FileName);
                file.SaveAs(fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("PackageFileValidation", "The package file could not be saved. " + ex.Message);
                return LocalRepository();
            }

            if(!string.IsNullOrWhiteSpace(Request.Form["autoinstall"]))
            {
                BackOfficeRequestContext.PackageContext.LocalPackageManager.InstallPackage(package, false);

                Notifications.Add(new NotificationMessage(package.Title + " has been installed", "Package installed", NotificationType.Success));
                SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);

                return RedirectToAction("RecycleApplication", new { id = package.Id, state = PackageInstallationState.Installing });
            }

            Notifications.Add(new NotificationMessage(package.Title + " added to local repository", "Package added", NotificationType.Success));
            SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);
            return RedirectToAction("LocalRepository");
        }

        /// <summary>
        /// Runs the actions for a package, this is generally run after the app pool is recycled
        /// </summary>
        /// <param name="id">The nuget package id</param>
        /// <param name="state">If its installing or uninstalling</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RunPackageActions(string id, PackageInstallationState state)
        {
            var nugetPackage = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(id);
            
            //Run the configuration tasks
            //get the package with the name and ensure it exists
            
            if (nugetPackage != null)
            {
                //get the package folder name, create the task execution context and then execute the package tasks using the utility class
                var packageFolderName = BackOfficeRequestContext.PackageContext.LocalPathResolver.GetPackageDirectory(nugetPackage);
                var taskExeContext = _packageInstallUtility.GetTaskExecutionContext(nugetPackage, packageFolderName, state, this);
                _packageInstallUtility.RunPostPackageInstallActions(state, taskExeContext, packageFolderName);    
            
                //re-issue authentication token incase any permissions have changed so that a re-login is not required.
                if (User.Identity is UmbracoBackOfficeIdentity)
                {
                    var user = (UmbracoBackOfficeIdentity)User.Identity;
                    var userId = user.Id;

                    using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<ISecurityStore>())
                    {
                        var entity = uow.Repositories.Get<User>(userId);

                        HttpContext.CreateUmbracoAuthTicket(entity);
                    }
                }
            }

            //if the task did not redirect, then show the LocalRepositoryr
            return RedirectToAction("LocalRepository");
        }

        private IEnumerable<PackageModel> GetLocalPackages()
        {           
            var packages = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository
                .GetPackages()
                .ToArray();

            Func<IPackage, bool> checkLatest = (p) =>
                {
                    var packagesForId = packages.Where(x => x.Id == p.Id).ToArray();
                    //need to check major/minor version first, then need to go by DatePublished.
                    var biggestMajor = packagesForId.Max(x => x.Version.Major);
                    if (p.Version.Major < biggestMajor) return false;
                    //this means the major versions are the same, so now check minor
                    var biggestMinor = packagesForId
                        .Where(x => x.Version.Major == biggestMajor)
                        .Max(x => x.Version.Minor);
                    if (p.Version.Minor < biggestMinor) return false;
                    //this means the minor version are the same, so now check published date
                    var published = packagesForId
                        .Where(x => x.Version.Major == biggestMajor
                            && x.Version.Minor == biggestMinor
                            && x.Published.HasValue).ToArray();
                    if (!p.Published.HasValue && published.Count(x => x.Published.HasValue) > 0)
                        return false;
                    var maxPublish = published.Max(x => x.Published.Value);
                    if (p.Published.Value < maxPublish) return false;
                    
                    return true;
                };

            var models =  packages.Select(x => new PackageModel
                {
                    IsLatestVersion = checkLatest(x),
                    IsVersionInstalled = BackOfficeRequestContext.PackageContext.LocalPackageManager.LocalRepository.FindPackage(x.Id, x.Version) != null,
                    Metadata = x,
                    IsPackageInstalled = x.IsInstalled(BackOfficeRequestContext.PackageContext.LocalPackageManager.LocalRepository)
                }).ToArray();

            return models;
        }
    }
}
